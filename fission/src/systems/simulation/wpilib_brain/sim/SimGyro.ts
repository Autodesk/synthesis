import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type Mechanism from "@/systems/physics/Mechanism"
import World from "@/systems/World"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions"
import { SimInput } from "../SimInput"
import { SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"
import type { SimReceiver } from "../SimDataFlow"
import { receiverTypeMap } from "../WPILibState"
import type { NoraNumber6 } from "../../Nora"

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

    public static genReceiver(device: string): SimReceiver {
        return {
            getReceiverType: () => receiverTypeMap[SimType.GYRO]!,
            setReceiverValue: ([ax, ay, az, rx, ry, rz]: NoraNumber6) => {
                SimGyro.setAngleX(device, ax)
                SimGyro.setAngleY(device, ay)
                SimGyro.setAngleZ(device, az)
                SimGyro.setRateX(device, rx)
                SimGyro.setRateY(device, ry)
                SimGyro.setRateZ(device, rz)
            },
        }
    }
}

export class SimGyroInput extends SimInput {
    private _robot: Mechanism
    private _joltID?: Jolt.BodyID
    private _joltBody?: Jolt.Body

    private _offset = { x: 0, y: 0, z: 0 }
    private _accumulated = { x: 0, y: 0, z: 0 }
    private _lastWritten = { x: 0, y: 0, z: 0 }

    private static readonly ANGLE_FIELD = { x: ">angle_x", y: ">angle_y", z: ">angle_z" } as const

    constructor(device: string, robot: Mechanism) {
        super(device)
        this._robot = robot
        this._joltID = this._robot.nodeToBody.get(this._robot.rootBody)

        if (this._joltID) this._joltBody = World.physicsSystem.getBody(this._joltID)!
    }

    private getBodyAngularVelocity(): THREE.Vector3 {
        if (!this._joltBody) return new THREE.Vector3(0, 0, 0)

        const worldOmega = convertJoltVec3ToThreeVector3(this._joltBody.GetAngularVelocity(), false)
        const rot = convertJoltQuatToThreeQuaternion(this._joltBody.GetRotation(), false)
        return worldOmega.applyQuaternion(rot.invert())
    }

    private integrateAngle(axis: "x" | "y" | "z", rate: number, deltaT: number): number {
        this._accumulated[axis] += rate * deltaT

        const external = SimGeneric.getUnsafe<number>(SimType.GYRO, this._device, SimGyroInput.ANGLE_FIELD[axis])
        if (external !== undefined && external !== this._lastWritten[axis]) {
            this._offset[axis] = this._accumulated[axis] - external
        }

        const angle = this._accumulated[axis] - this._offset[axis]
        this._lastWritten[axis] = angle
        return angle
    }

    public update(deltaT: number) {
        const omega = this.getBodyAngularVelocity()

        // WPILib uses deg and deg/s
        const rateX = THREE.MathUtils.radToDeg(omega.x)
        const rateY = THREE.MathUtils.radToDeg(omega.y)
        const rateZ = THREE.MathUtils.radToDeg(omega.z)

        SimGyro.setAngleX(this._device, this.integrateAngle("x", rateX, deltaT))
        SimGyro.setAngleY(this._device, this.integrateAngle("y", rateY, deltaT))
        SimGyro.setAngleZ(this._device, this.integrateAngle("z", rateZ, deltaT))
        SimGyro.setRateX(this._device, rateX)
        SimGyro.setRateY(this._device, rateY)
        SimGyro.setRateZ(this._device, rateZ)
    }
}
