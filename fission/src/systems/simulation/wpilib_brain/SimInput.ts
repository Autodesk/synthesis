import World from "@/systems/World"
import EncoderStimulus from "../stimulus/EncoderStimulus"
import { SimCANEncoder, SimGyro, SimAccel, SimDIO, SimAI } from "./WPILibBrain"
import Mechanism from "@/systems/physics/Mechanism"
import Jolt from "@azaleacolburn/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions"
import * as THREE from "three"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

export abstract class SimInput {
    constructor(protected _device: string) {}

    public abstract update(deltaT: number): void

    public get device(): string {
        return this._device
    }
}

export class SimEncoderInput extends SimInput {
    private _stimulus: EncoderStimulus

    constructor(device: string, stimulus: EncoderStimulus) {
        super(device)
        this._stimulus = stimulus
    }

    public update(_deltaT: number) {
        SimCANEncoder.setPosition(this._device, this._stimulus.positionValue)
        SimCANEncoder.setVelocity(this._device, this._stimulus.velocityValue)
    }
}

export class SimGyroInput extends SimInput {
    private _robot: Mechanism
    private _joltID?: Jolt.BodyID
    private _joltBody?: Jolt.Body

    private static readonly AXIS_X: Jolt.Vec3 = new JOLT.Vec3(1, 0, 0)
    private static readonly AXIS_Y: Jolt.Vec3 = new JOLT.Vec3(0, 1, 0)
    private static readonly AXIS_Z: Jolt.Vec3 = new JOLT.Vec3(0, 0, 1)

    constructor(device: string, robot: Mechanism) {
        super(device)
        this._robot = robot
        this._joltID = this._robot.nodeToBody.get(this._robot.rootBody)

        if (this._joltID) this._joltBody = World.physicsSystem.getBody(this._joltID)
    }

    private getAxis(axis: Jolt.Vec3): number {
        return ((this._joltBody?.GetRotation().GetRotationAngle(axis) ?? 0) * 180) / Math.PI
    }

    private getX(): number {
        return this.getAxis(SimGyroInput.AXIS_X)
    }

    private getY(): number {
        return this.getAxis(SimGyroInput.AXIS_Y)
    }

    private getZ(): number {
        return this.getAxis(SimGyroInput.AXIS_Z)
    }

    private getAxisVelocity(axis: "x" | "y" | "z"): number {
        const axes = this._joltBody?.GetAngularVelocity()
        if (!axes) return 0

        switch (axis) {
            case "x":
                return axes.GetX()
            case "y":
                return axes.GetY()
            case "z":
                return axes.GetZ()
        }
    }

    public update(_deltaT: number) {
        const x = this.getX()
        const y = this.getY()
        const z = this.getZ()

        SimGyro.setAngleX(this._device, x)
        SimGyro.setAngleY(this._device, y)
        SimGyro.setAngleZ(this._device, z)
        SimGyro.setRateX(this._device, this.getAxisVelocity("x"))
        SimGyro.setRateY(this._device, this.getAxisVelocity("y"))
        SimGyro.setRateZ(this._device, this.getAxisVelocity("z"))
    }
}

export class SimAccelInput extends SimInput {
    private _robot: Mechanism
    private _joltID?: Jolt.BodyID
    private _prevVel: THREE.Vector3

    constructor(device: string, robot: Mechanism) {
        super(device)
        this._robot = robot
        this._joltID = this._robot.nodeToBody.get(this._robot.rootBody)
        this._prevVel = new THREE.Vector3(0, 0, 0)
    }

    public update(deltaT: number) {
        if (!this._joltID) return
        const body = World.physicsSystem.getBody(this._joltID)

        const rot = convertJoltQuatToThreeQuaternion(body.GetRotation())
        const mat = new THREE.Matrix4().makeRotationFromQuaternion(rot).transpose()
        const newVel = convertJoltVec3ToThreeVector3(body.GetLinearVelocity()).applyMatrix4(mat)

        const x = (newVel.x - this._prevVel.x) / deltaT
        const y = (newVel.y - this._prevVel.y) / deltaT
        const z = (newVel.z - this._prevVel.z) / deltaT

        SimAccel.setX(this._device, x)
        SimAccel.setY(this._device, y)
        SimAccel.setZ(this._device, z)

        this._prevVel = newVel
    }
}

export class SimCameraInput extends SimInput {
    private _defaultWidth: number = 320
    private _defaultHeight: number = 240
    private _defaultFPS: number = 30
    private _isInitialized: boolean = false
    private _cameraRenderer?: SimCameraRenderer
    private _robot: MirabufSceneObject
    private _frameInterval: number = 0
    private _lastFrameTime: number = 0

    constructor(device: string, robot: MirabufSceneObject, width?: number, height?: number, fps?: number) {
        super(device)
        this._robot = robot
        if (width) this._defaultWidth = width
        if (height) this._defaultHeight = height  
        if (fps) this._defaultFPS = fps
        this._frameInterval = 1000 / this._defaultFPS // ms between frames
        
        console.log(`🎬 [CONSTRUCTOR] SimCameraInput created for ${device} (${this._defaultWidth}x${this._defaultHeight} @ ${this._defaultFPS}fps, interval=${this._frameInterval}ms)`)
    }

    public update(deltaT: number) {
        // Add occasional logging to confirm update is being called
        if (Math.random() < 0.01) { // ~1% chance per frame
            console.log(`🔄 [UPDATE] SimCameraInput.update() called for ${this.device} (initialized: ${this._isInitialized})`)
        }
        
        if (!this._isInitialized) {
            this.initializeCamera()
            this._isInitialized = true
        }

        this.updateCameraSettings()
        this.generateVideoFrame(deltaT)
    }

    private initializeCamera() {
        console.log(`🎥 [INIT] Starting camera initialization for ${this.device}`)
        
        // Initialize metadata
        SimCamera.setConnected(this.device, true)
        SimCamera.setResolutionWidth(this.device, this._defaultWidth)
        SimCamera.setResolutionHeight(this.device, this._defaultHeight)
        SimCamera.setFPS(this.device, this._defaultFPS)
        SimCamera.setBrightness(this.device, 50)
        SimCamera.setExposure(this.device, 50)
        SimCamera.setAutoExposure(this.device, true)

        console.log(`🎥 [INIT] Camera metadata set, creating renderer...`)

        try {
            // Initialize video renderer for 3D scene capture
            this._cameraRenderer = new SimCameraRenderer(this._robot, this._defaultWidth, this._defaultHeight)
            console.log(`✅ [INIT] Camera ${this.device} initialized successfully - 3D frames will be generated`)
            
            // Force immediate test frame to verify renderer works
            console.log(`🧪 [TEST] Attempting immediate test frame capture...`)
            this._cameraRenderer.captureFrameAsJPEG().then(blob => {
                console.log(`🧪 [TEST] Initial test frame captured: ${blob.size} bytes - renderer is working!`)
                this.sendFrameToRobot(blob)
            }).catch(error => {
                console.error(`❌ [TEST] Initial test frame failed:`, error)
            })
        } catch (error) {
            console.error(`❌ [INIT] Failed to create camera renderer:`, error)
        }
    }

    private updateCameraSettings() {
        const requestedWidth = SimCamera.getRequestedWidth(this._device)
        const requestedHeight = SimCamera.getRequestedHeight(this._device)
        const requestedFPS = SimCamera.getRequestedFPS(this._device)

        // Check if resolution changed
        const currentWidth = SimCamera.getRequestedWidth(this._device)
        const currentHeight = SimCamera.getRequestedHeight(this._device)
        if (requestedWidth !== currentWidth || requestedHeight !== currentHeight) {
            this.updateResolution(requestedWidth, requestedHeight)
        }

        // Check if frame rate changed - ensure FPS is valid
        if (requestedFPS && !isNaN(requestedFPS) && requestedFPS > 0 && requestedFPS !== this._defaultFPS) {
            this.updateFrameRate(requestedFPS)
            this._defaultFPS = requestedFPS
        }

        SimCamera.setResolutionWidth(this.device, requestedWidth)
        SimCamera.setResolutionHeight(this.device, requestedHeight)
        SimCamera.setFPS(this.device, requestedFPS)

        const requestedBrightness = SimCamera.getRequestedBrightness(this._device)
        const requestedExposure = SimCamera.getRequestedExposure(this._device)
        const requestedAutoExposure = SimCamera.getRequestedAutoExposure(this._device)

        SimCamera.setBrightness(this.device, requestedBrightness)
        SimCamera.setExposure(this.device, requestedExposure)
        SimCamera.setAutoExposure(this.device, requestedAutoExposure)
    }

    private generateVideoFrame(deltaT: number) {
        if (!this._cameraRenderer) {
            console.warn(`📹 [FRAME] No camera renderer for ${this.device} - skipping frame generation`)
            return
        }
        
        this._lastFrameTime += deltaT * 1000 // Convert to ms
        
        // Add timing debug logs occasionally
        if (Math.random() < 0.01) { // ~1% chance per frame
            console.log(`⏱️ [TIMING] ${this.device}: lastFrameTime=${this._lastFrameTime.toFixed(1)}ms, interval=${this._frameInterval}ms, deltaT=${(deltaT*1000).toFixed(1)}ms`)
        }
        
        // Generate frame at specified FPS
        if (this._lastFrameTime >= this._frameInterval) {
            this._lastFrameTime = 0
            
            console.log(`📹 [FRAME] Capturing 3D frame for ${this.device}`)
            
            // Capture frame from 3D scene (robot perspective)
            this._cameraRenderer.captureFrameAsJPEG().then(blob => {
                console.log(`📹 [FRAME] Successfully captured ${blob.size} bytes, sending to robot`)
                this.sendFrameToRobot(blob)
            }).catch(error => {
                console.error(`❌ [FRAME] Failed to capture camera frame:`, error)
            })
        }
    }

    private async sendFrameToRobot(frameBlob: Blob) {
        try {
            // Convert blob to base64 for WebSocket transmission
            const arrayBuffer = await frameBlob.arrayBuffer()
            const base64Frame = btoa(String.fromCharCode(...new Uint8Array(arrayBuffer)))
            
            console.log(`🚀 [SEND] Converting frame: ${arrayBuffer.byteLength} bytes → ${base64Frame.length} chars`)

            console.log(`📡 [SEND] WebSocket frame sent for ${this.device}: ${success ? 'SUCCESS' : 'FAILED'}`)
            
        } catch (error) {
            console.error(`❌ [SEND] Failed to send camera frame:`, error)
        }
    }

    private updateResolution(width: number, height: number) {
        if (this._cameraRenderer) {
            this._cameraRenderer.setResolution(width, height)
        }
    }

    private updateFrameRate(fps: number) {
        this._frameInterval = 1000 / fps
    }

    public reconnect() {
        SimCamera.setConnected(this._device, true)
        this._isInitialized = false // set false so reinitialize on next update
    }
    
    public disconnect() {
        SimCamera.setConnected(this._device, false)
        this._isInitialized = false 
        if (this._cameraRenderer) {
            this._cameraRenderer.dispose()
            this._cameraRenderer = undefined
        }
    }
}

export class SimDigitalInput extends SimInput {
    private _valueSupplier: () => boolean

    /**
     * Creates a Simulation Digital Input object.
     *
     * @param device Device ID
     * @param valueSupplier Called each frame and returns what the value should be set to
     */
    constructor(device: string, valueSupplier: () => boolean) {
        super(device)
        this._valueSupplier = valueSupplier
    }

    private setValue(value: boolean) {
        SimDIO.setValue(this._device, value)
    }

    public getValue(): boolean {
        return SimDIO.getValue(this._device)
    }

    public update(_deltaT: number) {
        if (this._valueSupplier) this.setValue(this._valueSupplier())
    }
}

export class SimAnalogInput extends SimInput {
    private _valueSupplier: () => number

    constructor(device: string, valueSupplier: () => number) {
        super(device)
        this._valueSupplier = valueSupplier
    }

    public update(_deltaT: number) {
        SimAI.setValue(this._device, this._valueSupplier())
    }
}
