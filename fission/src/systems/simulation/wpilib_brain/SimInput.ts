import World from "@/systems/World"
import EncoderStimulus from "../stimulus/EncoderStimulus"
import { SimCANEncoder, SimGyro } from "./WPILibBrain"
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
        SimCANEncoder.SetPosition(`${this._device}`, this._stimulus.positionValue)
        SimCANEncoder.SetVelocity(`${this._device}`, this._stimulus.velocityValue)
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

    public Update(_deltaT: number) {
        const x = this.GetX()
        const y = this.GetY()
        const z = this.GetZ()
        // console.log(`${this._device}\n${x}\n${y}\n${z}`)
        SimGyro.SetAngleX(this._device, x)
        SimGyro.SetAngleY(this._device, y)
        SimGyro.SetAngleZ(this._device, z)
    }
}
