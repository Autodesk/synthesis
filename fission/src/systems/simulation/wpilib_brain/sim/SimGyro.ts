import type Jolt from "@azaleacolburn/jolt-physics"
import type Mechanism from "@/systems/physics/Mechanism"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"
import { SimInput } from "../SimInput"
import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimGyro {
    private constructor() {}

    public static setAngleX(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_x", angle)
    }

    public static setAngleY(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_y", angle)
    }

    public static setAngleZ(device: string, angle: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">angle_z", angle)
    }

    public static setRateX(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_x", rate)
    }

    public static setRateY(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_y", rate)
    }

    public static setRateZ(device: string, rate: number): boolean {
        return SimGeneric.set(SimType.GYRO, device, ">rate_z", rate)
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

    public update(_deltaT: number) {
        if (!this._joltBody) return

        const rot = this._joltBody.GetRotation()
        const angVel = this._joltBody.GetAngularVelocity()

        const RAD2DEG = 180 / Math.PI
        SimGeneric.setMany(SimType.GYRO, this._device, {
            ">angle_x": rot.GetRotationAngle(SimGyroInput.AXIS_X) * RAD2DEG,
            ">angle_y": rot.GetRotationAngle(SimGyroInput.AXIS_Y) * RAD2DEG,
            ">angle_z": rot.GetRotationAngle(SimGyroInput.AXIS_Z) * RAD2DEG,
            ">rate_x": angVel.GetX(),
            ">rate_y": angVel.GetY(),
            ">rate_z": angVel.GetZ(),
        })

        JOLT.destroy(rot)
        JOLT.destroy(angVel)
    }
}
