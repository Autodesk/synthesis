import type Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import type { CameraPreferences } from "@/systems/preferences/PreferenceTypes"
import SceneObject from "@/systems/scene/SceneObject"
import { sendCameraFrame } from "@/systems/simulation/wpilib_brain/CameraFrameSocket"
import SimCamera from "@/systems/simulation/wpilib_brain/sim/SimCamera"
import World from "@/systems/World"
import { convertArrayToThreeMatrix4, convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"
import type MirabufSceneObject from "./MirabufSceneObject"

// Guard rails so a misconfigured camera can't tank frame rate.
const MAX_DIMENSION = 1280
const MIN_DIMENSION = 16
const MAX_FPS = 60
const JPEG_QUALITY = 0.6

// A THREE.PerspectiveCamera looks down its local -Z, but the placeholder mesh in the
// config gizmo points down +Z. Rotate the camera 180° about its up axis so it looks where
// the gizmo points (away from the robot) instead of back into it.
const FORWARD_FLIP = new THREE.Matrix4().makeRotationY(Math.PI)

/**
 * A USB camera mounted to a robot. Each frame (throttled to the configured fps) it
 * positions a secondary perspective camera relative to a robot rigid node, renders the
 * scene into an offscreen target, and reads the pixels back. The latest frame is kept on
 * an internal canvas for the preview UI, and—when the robot code has created the matching
 * camera sim device—is JPEG encoded and streamed to the robot code via {@link SimCamera}.
 *
 * Mounting math mirrors {@link IntakeSensorSceneObject}: the world transform is
 * `deltaTransformation * parentBodyWorldTransform`.
 */
class RobotCameraSceneObject extends SceneObject {
    // Number of active frame consumers (e.g. open preview panels). Capture is skipped
    // entirely unless something needs the frame, so simply having a camera configured
    // (e.g. while in the config panel) costs nothing.
    private static _previewConsumers = 0
    public static get previewConsumers(): number {
        return RobotCameraSceneObject._previewConsumers
    }
    public static addPreviewConsumer(): void {
        RobotCameraSceneObject._previewConsumers++
    }
    public static removePreviewConsumer(): void {
        RobotCameraSceneObject._previewConsumers = Math.max(0, RobotCameraSceneObject._previewConsumers - 1)
    }

    private _parentAssembly: MirabufSceneObject
    private _prefs: CameraPreferences

    private _camera: THREE.PerspectiveCamera
    private _renderTarget?: THREE.WebGLRenderTarget
    private _parentBodyId?: Jolt.BodyID
    private _deltaTransformation: THREE.Matrix4

    private _width = 0
    private _height = 0
    private _pixelBuffer?: Uint8Array
    private _frameCanvas?: HTMLCanvasElement
    private _frameCtx?: CanvasRenderingContext2D
    private _imageData?: ImageData

    private _timeSinceCapture = Number.POSITIVE_INFINITY

    public constructor(parentAssembly: MirabufSceneObject, prefs: CameraPreferences) {
        super()
        this._parentAssembly = parentAssembly
        this._prefs = prefs
        this._camera = new THREE.PerspectiveCamera(prefs.fovDegrees, 1, 0.05, 1000)
        this._deltaTransformation = convertArrayToThreeMatrix4(prefs.deltaTransformation)
    }

    /** The sim device key, e.g. `"USB Camera 0[0]"`. */
    public get deviceName(): string {
        return `${this._prefs.name}[${this._prefs.id}]`
    }

    /** Human-readable label used by the preview panel. */
    public get displayName(): string {
        return `${this._parentAssembly.assemblyName} – ${this._prefs.name}`
    }

    /** Canvas holding the most recently captured frame (for previews). */
    public get frameCanvas(): HTMLCanvasElement | undefined {
        return this._frameCanvas
    }

    /** Current capture width in pixels (falls back to the configured default). */
    public get width(): number {
        return this._width || this._prefs.resolutionWidth
    }

    /** Current capture height in pixels (falls back to the configured default). */
    public get height(): number {
        return this._height || this._prefs.resolutionHeight
    }

    public setup(): void {
        this._parentBodyId = this._parentAssembly.mechanism.nodeToBody.get(
            this._prefs.parentNode ?? this._parentAssembly.rootNodeId
        )
        this.resize(this._prefs.resolutionWidth, this._prefs.resolutionHeight)
    }

    private resize(width: number, height: number): void {
        const w = Math.max(MIN_DIMENSION, Math.min(MAX_DIMENSION, Math.round(width)))
        const h = Math.max(MIN_DIMENSION, Math.min(MAX_DIMENSION, Math.round(height)))
        if (w === this._width && h === this._height && this._renderTarget) return

        this._width = w
        this._height = h

        this._renderTarget?.dispose()
        this._renderTarget = new THREE.WebGLRenderTarget(w, h)

        this._pixelBuffer = new Uint8Array(w * h * 4)
        this._imageData = new ImageData(w, h)

        const canvas = this._frameCanvas ?? document.createElement("canvas")
        canvas.width = w
        canvas.height = h
        this._frameCanvas = canvas
        this._frameCtx = canvas.getContext("2d") ?? undefined

        this._camera.aspect = w / h
        this._camera.updateProjectionMatrix()
    }

    public update(): void {
        if (!this._parentBodyId || !this._renderTarget || !this._pixelBuffer || !this._imageData) return

        const device = this.deviceName
        // Stream to robot code once it has created the camera (discovered over HALSim).
        const streaming = SimCamera.isPresent(device)
        // Skip all rendering/readback unless someone actually needs a frame. This keeps the
        // config panel (and idle robots) fully interactive — the GPU readback below is a
        // synchronous stall that must not run every frame for no reason.
        if (!streaming && RobotCameraSceneObject.previewConsumers === 0) return

        // Pick up resolution / fps / fov that the robot code requested (defaults to prefs).
        const reqWidth = SimCamera.getWidth(device, this._prefs.resolutionWidth)
        const reqHeight = SimCamera.getHeight(device, this._prefs.resolutionHeight)
        const fps = Math.max(1, Math.min(MAX_FPS, SimCamera.getFps(device, this._prefs.fps)))
        this.resize(reqWidth, reqHeight)

        if (this._camera.fov !== this._prefs.fovDegrees) {
            this._camera.fov = this._prefs.fovDegrees
            this._camera.updateProjectionMatrix()
        }

        // Throttle capture to the configured frame rate.
        this._timeSinceCapture += World.currentDeltaT
        if (this._timeSinceCapture < 1 / fps) return
        this._timeSinceCapture = 0

        // Position the camera relative to its parent rigid node, then flip it to look
        // forward (where the gizmo points) rather than back into the robot.
        const parentBody = World.physicsSystem.getBody(this._parentBodyId)
        if (!parentBody) return
        const worldTransform = this._deltaTransformation
            .clone()
            .premultiply(convertJoltMat44ToThreeMatrix4(parentBody.GetWorldTransform()))
            .multiply(FORWARD_FLIP)
        this._camera.position.setFromMatrixPosition(worldTransform)
        this._camera.quaternion.setFromRotationMatrix(worldTransform)
        this._camera.updateMatrixWorld()

        // A failure here must never abort SceneRenderer.update (which would freeze the
        // scene render, camera controls, and input handling for the whole app).
        const renderer = World.sceneRenderer.renderer
        // The postprocessing EffectComposer leaves autoClear=false, so we must clear the
        // (depth) buffer ourselves or the camera renders a stale, partially depth-rejected
        // strip. setRenderTarget already applies the target's full-size viewport (without
        // pixel-ratio scaling), so we must NOT call setViewport — doing so re-scales by
        // devicePixelRatio and crops the capture.
        const prevTarget = renderer.getRenderTarget()
        const prevAutoClear = renderer.autoClear
        try {
            renderer.autoClear = true
            renderer.setRenderTarget(this._renderTarget)
            renderer.clear()
            renderer.render(World.sceneRenderer.scene, this._camera)
            renderer.readRenderTargetPixels(this._renderTarget, 0, 0, this._width, this._height, this._pixelBuffer)

            // GL pixels are bottom-up; flip into the (top-down) ImageData for the canvas.
            this.flipInto(this._imageData, this._pixelBuffer)
            this._frameCtx?.putImageData(this._imageData, 0, 0)

            // Frames travel over the dedicated frame socket, only while streaming.
            if (streaming && this._frameCanvas) {
                const dataUrl = this._frameCanvas.toDataURL("image/jpeg", JPEG_QUALITY)
                const base64 = dataUrl.slice(dataUrl.indexOf(",") + 1)
                sendCameraFrame(device, base64)
            }
        } catch (e) {
            console.error(`Camera capture failed for '${device}'`, e)
        } finally {
            // Restore renderer state so the main scene render is unaffected.
            renderer.setRenderTarget(prevTarget)
            renderer.autoClear = prevAutoClear
        }
    }

    private flipInto(target: ImageData, source: Uint8Array): void {
        const w = this._width
        const h = this._height
        const rowBytes = w * 4
        const dst = target.data
        for (let y = 0; y < h; y++) {
            const srcStart = (h - 1 - y) * rowBytes
            dst.set(source.subarray(srcStart, srcStart + rowBytes), y * rowBytes)
        }
    }

    public dispose(): void {
        this._renderTarget?.dispose()
        this._renderTarget = undefined
        this._frameCanvas = undefined
        this._frameCtx = undefined
    }
}

export default RobotCameraSceneObject
