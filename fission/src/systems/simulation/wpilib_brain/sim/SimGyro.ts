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
