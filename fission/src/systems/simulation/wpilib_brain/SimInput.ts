import World from "@/systems/World"
import EncoderStimulus from "../stimulus/EncoderStimulus"
import { SimCANEncoder, SimGyro, SimAccel } from "./WPILibBrain"
import Mechanism from "@/systems/physics/Mechanism"
import Jolt from "@barclah/jolt-physics"
import JOLT from "@/util/loading/JoltSyncLoader"

export interface SimInput {
    Update: (deltaT: number) => void
}

export class SimEncoderInput implements SimInput {
    private _device: string
    private _stimulus: EncoderStimulus

    constructor(device: string, stimulus: EncoderStimulus) {
        this._device = device
        this._stimulus = stimulus
    }

    public Update(_deltaT: number) {
        SimCANEncoder.SetPosition(this._device, this._stimulus.positionValue)
        SimCANEncoder.SetVelocity(this._device, this._stimulus.velocityValue)
    }
}

export class SimGyroInput implements SimInput {
    private _device: string
    private _robot: Mechanism
    private _joltID?: Jolt.BodyID
    private _joltBody?: Jolt.Body

    private static AXIS_X: Jolt.Vec3 = new JOLT.Vec3(1, 0, 0)
    private static AXIS_Y: Jolt.Vec3 = new JOLT.Vec3(0, 1, 0)
    private static AXIS_Z: Jolt.Vec3 = new JOLT.Vec3(0, 0, 1)

    constructor(device: string, robot: Mechanism) {
        this._device = device
        this._robot = robot
        this._joltID = this._robot.nodeToBody.get(this._robot.rootBody)

        if (this._joltID) this._joltBody = World.PhysicsSystem.GetBody(this._joltID)
    }

    private GetAxis(axis: Jolt.Vec3): number {
        return ((this._joltBody?.GetRotation().GetRotationAngle(axis) ?? 0) * 180) / Math.PI
    }

    private GetX(): number {
        return this.GetAxis(SimGyroInput.AXIS_X)
    }

    private GetY(): number {
        return this.GetAxis(SimGyroInput.AXIS_Y)
    }

    private GetZ(): number {
        return this.GetAxis(SimGyroInput.AXIS_Z)
    }

    private GetAxisVelocity(axis: "x" | "y" | "z"): number {
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

    public Update(_deltaT: number) {
        const x = this.GetX()
        const y = this.GetY()
        const z = this.GetZ()

        SimGyro.SetAngleX(this._device, x)
        SimGyro.SetAngleY(this._device, y)
        SimGyro.SetAngleZ(this._device, z)
        SimGyro.SetRateX(this._device, this.GetAxisVelocity("x"))
        SimGyro.SetRateY(this._device, this.GetAxisVelocity("y"))
        SimGyro.SetRateZ(this._device, this.GetAxisVelocity("z"))
    }
}

export class SimAccelInput implements SimInput {
    private _device: string
    private _robot: Mechanism
    private _joltID?: Jolt.BodyID
    private _joltBody?: Jolt.Body

    constructor(device: string, robot: Mechanism) {
        this._device = device
        this._robot = robot
        this._joltID = this._robot.nodeToBody.get(this._robot.rootBody)

        if (this._joltID) this._joltBody = World.PhysicsSystem.GetBody(this._joltID)
    }

    private GetAxis(axis: "x" | "y" | "z"): number {
        const forces = this._joltBody?.GetAccumulatedForce()
        if (!forces) return 0

        switch (axis) {
            case "x":
                return forces.GetX()
            case "y":
                return forces.GetY()
            case "z":
                return forces.GetZ()
        }
    }

    public Update(_deltaT: number) {
        SimAccel.SetX(this._device, this.GetAxis("x"))
        SimAccel.SetY(this._device, this.GetAxis("y"))
        SimAccel.SetZ(this._device, this.GetAxis("z"))
    }
}
