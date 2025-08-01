import * as THREE from "three"
import World from "@/systems/World"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

export class SimCameraRenderer {
    private _camera: THREE.PerspectiveCamera
    private _renderTarget: THREE.WebGLRenderTarget
    private _canvas: OffscreenCanvas
    private _ctx: OffscreenCanvasRenderingContext2D
    private _robot: MirabufSceneObject
    private _cameraPosition: THREE.Vector3
    private _cameraQuaternion: THREE.Quaternion

    constructor(robot: MirabufSceneObject, width: number = 640, height: number = 480) {
        this._robot = robot

        // Create camera for robot perspective
        this._camera = new THREE.PerspectiveCamera(75, width / height, 0.1, 1000)

        // Create render target for off-screen rendering
        this._renderTarget = new THREE.WebGLRenderTarget(width, height, {
            minFilter: THREE.LinearFilter,
            magFilter: THREE.LinearFilter,
            format: THREE.RGBAFormat,
        })

        // Create canvas for frame capture
        this._canvas = new OffscreenCanvas(width, height)
        this._ctx = this._canvas.getContext("2d")!

        // Camera position relative to robot
        this._cameraPosition = new THREE.Vector3(0, 0.5, 0.2) // Mounted on robot
        this._cameraQuaternion = new THREE.Quaternion()

        this.updateCameraTransform()
    }

    private updateCameraTransform() {
        if (!this._robot.mechanism.rootBody) return

        // Get robot's transform
        const robotBody = World.physicsSystem.getBody(
            this._robot.mechanism.nodeToBody.get(this._robot.mechanism.rootBody)!
        )

        if (!robotBody) return

        // Position camera relative to robot
        const robotPos = robotBody.GetPosition()
        const robotRot = robotBody.GetRotation()

        // Convert Jolt to Three.js
        const robotPosition = new THREE.Vector3(robotPos.GetX(), robotPos.GetY(), robotPos.GetZ())
        const robotQuaternion = new THREE.Quaternion(robotRot.GetX(), robotRot.GetY(), robotRot.GetZ(), robotRot.GetW())

        // Apply camera offset
        const worldCameraPos = this._cameraPosition.clone()
        worldCameraPos.applyQuaternion(robotQuaternion)
        worldCameraPos.add(robotPosition)

        this._camera.position.copy(worldCameraPos)
        this._camera.quaternion.copy(robotQuaternion)
        this._camera.updateMatrixWorld()
    }

    public renderFrame(): ImageData | null {
        if (!World.sceneRenderer) return null

        this.updateCameraTransform()

        // Render scene from camera perspective
        const renderer = (World.sceneRenderer as any)._renderer as THREE.WebGLRenderer
        const scene = (World.sceneRenderer as any)._scene as THREE.Scene

        // Store original render target
        const originalTarget = renderer.getRenderTarget()

        // Render to our target
        renderer.setRenderTarget(this._renderTarget)
        renderer.render(scene, this._camera)

        // Read pixels
        const pixels = new Uint8Array(this._renderTarget.width * this._renderTarget.height * 4)
        renderer.readRenderTargetPixels(
            this._renderTarget,
            0,
            0,
            this._renderTarget.width,
            this._renderTarget.height,
            pixels
        )

        // Restore original target
        renderer.setRenderTarget(originalTarget)

        // Convert to ImageData
        const imageData = new ImageData(
            new Uint8ClampedArray(pixels),
            this._renderTarget.width,
            this._renderTarget.height
        )

        return imageData
    }

    public captureFrameAsJPEG(): Promise<Blob> {
        const imageData = this.renderFrame()
        if (!imageData) return Promise.reject("No frame data")

        // Draw to canvas
        this._ctx.putImageData(imageData, 0, 0)

        // Convert to JPEG blob
        return this._canvas.convertToBlob({ type: "image/jpeg", quality: 0.8 })
    }

    public setResolution(width: number, height: number) {
        this._renderTarget.setSize(width, height)
        this._canvas.width = width
        this._canvas.height = height
        this._camera.aspect = width / height
        this._camera.updateProjectionMatrix()
    }

    public setCameraOffset(position: THREE.Vector3, rotation?: THREE.Quaternion) {
        this._cameraPosition.copy(position)
        if (rotation) {
            this._cameraQuaternion.copy(rotation)
        }
    }

    public dispose() {
        this._renderTarget.dispose()
    }
}
