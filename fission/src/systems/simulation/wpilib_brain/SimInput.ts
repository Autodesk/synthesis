import World from "@/systems/World"
import EncoderStimulus from "../stimulus/EncoderStimulus"
import { SimCANEncoder, SimGyro, SimAccel, SimDIO, SimAI } from "./WPILibBrain"
import Mechanism from "@/systems/physics/Mechanism"
import Jolt from "@azaleacolburn/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions"
import * as THREE from "three"

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
        const z = (newVel.y - this._prevVel.y) / deltaT

        SimAccel.setX(this._device, x)
        SimAccel.setY(this._device, y)
        SimAccel.setZ(this._device, z)

        this._prevVel = newVel
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
