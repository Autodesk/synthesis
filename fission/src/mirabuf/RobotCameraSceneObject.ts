import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type { CameraPreferences } from "@/systems/preferences/PreferenceTypes"
import SceneObject from "@/systems/scene/SceneObject"
import { sendCameraFrame } from "@/systems/simulation/wpilib_brain/CameraFrameSocket"
import SimCamera from "@/systems/simulation/wpilib_brain/sim/SimCamera"
import World from "@/systems/World"
import { convertArrayToThreeMatrix4, convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"
import type MirabufSceneObject from "./MirabufSceneObject"

const MAX_DIMENSION = 1280
const MIN_DIMENSION = 16
const MAX_FPS = 60
const JPEG_QUALITY = 0.6

// camera looks down local -Z but the gizmo placeholder points +Z; flip so it faces where
// the gizmo points (away from the robot), not back into it
const FORWARD_FLIP = new THREE.Matrix4().makeRotationY(Math.PI)

class RobotCameraSceneObject extends SceneObject {
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
    private readonly _worldTransform = new THREE.Matrix4()

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

    /** sim device key, e.g. `"USB Camera 0[0]"` */
    public get deviceName(): string {
        return `${this._prefs.name}[${this._prefs.id}]`
    }

    public get displayName(): string {
        return `${this._parentAssembly.assemblyName} – ${this._prefs.name}`
    }

    public get frameCanvas(): HTMLCanvasElement | undefined {
        return this._frameCanvas
    }

    public get width(): number {
        return this._width || this._prefs.resolutionWidth
    }

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

        if (World.physicsSystem.isPaused) return

        const device = this.deviceName
        const streaming = SimCamera.isPresent(device)

        if (!streaming && RobotCameraSceneObject.previewConsumers === 0) return

        let reqWidth = this._prefs.resolutionWidth
        let reqHeight = this._prefs.resolutionHeight
        let reqFps = this._prefs.fps

        // lets the camera preview panel work without robot code sim running
        if (streaming) {
            reqWidth = SimCamera.getWidth(device, reqWidth)
            reqHeight = SimCamera.getHeight(device, reqHeight)
            reqFps = SimCamera.getFps(device, reqFps)
        }
        const fps = Math.max(1, Math.min(MAX_FPS, reqFps))
        this.resize(reqWidth, reqHeight)

        if (this._camera.fov !== this._prefs.fovDegrees) {
            this._camera.fov = this._prefs.fovDegrees
            this._camera.updateProjectionMatrix()
        }

        this._timeSinceCapture += World.currentDeltaT
        if (this._timeSinceCapture < 1 / fps) return
        this._timeSinceCapture = 0

        const parentBody = World.physicsSystem.getBody(this._parentBodyId)
        if (!parentBody) return
        const worldTransform = this._worldTransform
            .copy(this._deltaTransformation)
            .premultiply(convertJoltMat44ToThreeMatrix4(parentBody.GetWorldTransform()))
            .multiply(FORWARD_FLIP)

        this._camera.position.setFromMatrixPosition(worldTransform)
        this._camera.quaternion.setFromRotationMatrix(worldTransform)
        this._camera.updateMatrixWorld()

        const renderer = World.sceneRenderer.renderer
        const prevTarget = renderer.getRenderTarget()
        const prevAutoClear = renderer.autoClear
        try {
            renderer.autoClear = true
            renderer.setRenderTarget(this._renderTarget)
            renderer.clear()
            renderer.render(World.sceneRenderer.scene, this._camera)
            renderer.readRenderTargetPixels(this._renderTarget, 0, 0, this._width, this._height, this._pixelBuffer)

            this.flipInto(this._imageData, this._pixelBuffer)
            this._frameCtx?.putImageData(this._imageData, 0, 0)

            if (streaming && this._frameCanvas) {
                const dataUrl = this._frameCanvas.toDataURL("image/jpeg", JPEG_QUALITY)
                const base64 = atob(dataUrl.slice(dataUrl.indexOf(",") + 1))
                const bytes = new Uint8Array(base64.length)
                for (let i = 0; i < base64.length; i++) bytes[i] = base64.charCodeAt(i)
                sendCameraFrame(device, bytes)
            }
        } catch (e) {
            console.error(`Camera capture failed for '${device}'`, e)
        } finally {
            renderer.setRenderTarget(prevTarget)
            renderer.autoClear = prevAutoClear
        }
    }

    // GL pixels are bottom-up; flip rows into the top-down ImageData
    private flipInto(target: ImageData, source: Uint8Array): void {
        const rowBytes = this._width * 4
        const dst = target.data
        for (let y = 0; y < this._height; y++) {
            const srcStart = (this._height - 1 - y) * rowBytes
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
