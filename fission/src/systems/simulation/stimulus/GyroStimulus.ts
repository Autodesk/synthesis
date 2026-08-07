import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type { mirabuf } from "@/proto/mirabuf"
import World from "@/systems/World"
import {
    convertArrayToThreeMatrix4,
    convertJoltQuatToThreeQuaternion,
    convertJoltVec3ToThreeVector3,
} from "@/util/TypeConversions"
import { BaseUnit, DerivativeOrder, noraType, type NoraValueOf, num } from "../Nora"
import { SimType } from "../wpilib_brain/WPILibTypes"
import SimGeneric from "../wpilib_brain/sim/SimGeneric"
import Stimulus, { type StimulusID } from "./Stimulus"

const GYRO_TYPE = noraType([
    num(BaseUnit.ANGLE, DerivativeOrder.ZERO),
    num(BaseUnit.ANGLE, DerivativeOrder.ZERO),
    num(BaseUnit.ANGLE, DerivativeOrder.ZERO),
    num(BaseUnit.ANGLE, DerivativeOrder.ONE),
    num(BaseUnit.ANGLE, DerivativeOrder.ONE),
    num(BaseUnit.ANGLE, DerivativeOrder.ONE),
])

class GyroStimulus extends Stimulus<typeof GYRO_TYPE> {
    private _body: Jolt.Body
    private _mountRotation: THREE.Quaternion
    private _device: string

    // WPILib uses deg and deg/s
    private _angle = { x: 0, y: 0, z: 0 }
    private _rate = { x: 0, y: 0, z: 0 }

    private _offset = { x: 0, y: 0, z: 0 }
    private _accumulated = { x: 0, y: 0, z: 0 }
    private _lastWritten = { x: 0, y: 0, z: 0 }

    private static readonly ANGLE_FIELD = { x: ">angle_x", y: ">angle_z", z: ">angle_y" } as const

    public constructor(
        id: StimulusID,
        bodyId: Jolt.BodyID,
        deltaTransformation: number[],
        device: string,
        info?: mirabuf.IInfo
    ) {
        super(id, info)

        this._body = World.physicsSystem.getBody(bodyId)!
        this._device = device

        const rot = new THREE.Quaternion()
        convertArrayToThreeMatrix4(deltaTransformation).decompose(new THREE.Vector3(), rot, new THREE.Vector3())
        this._mountRotation = rot
    }

    /** Body angular velocity in the sensor's mount frame. */
    private mountAngularVelocity(): THREE.Vector3 {
        const worldOmega = convertJoltVec3ToThreeVector3(this._body.GetAngularVelocity(), false)
        const bodyRot = convertJoltQuatToThreeQuaternion(this._body.GetRotation(), false)
        return worldOmega.applyQuaternion(bodyRot.invert()).applyQuaternion(this._mountRotation.clone().invert())
    }

    private integrateAngle(axis: "x" | "y" | "z", rate: number, deltaT: number): number {
        this._accumulated[axis] += rate * deltaT

        const external = SimGeneric.getUnsafe<number>(SimType.GYRO, this._device, GyroStimulus.ANGLE_FIELD[axis])
        if (external !== undefined && external !== this._lastWritten[axis]) {
            this._offset[axis] = this._accumulated[axis] - external
        }

        const angle = this._accumulated[axis] - this._offset[axis]
        this._lastWritten[axis] = angle
        return angle
    }

    public update(deltaT: number): void {
        const omega = this.mountAngularVelocity()

        this._rate.x = THREE.MathUtils.radToDeg(omega.x)
        this._rate.y = THREE.MathUtils.radToDeg(omega.y)
        this._rate.z = THREE.MathUtils.radToDeg(omega.z)

        this._angle.x = this.integrateAngle("x", this._rate.x, deltaT)
        this._angle.y = this.integrateAngle("y", this._rate.y, deltaT)
        this._angle.z = this.integrateAngle("z", this._rate.z, deltaT)
    }

    public get supplierType() {
        return GYRO_TYPE
    }

    public supplyValue(): NoraValueOf<typeof GYRO_TYPE> {
        return [this._angle.x, this._angle.y, this._angle.z, this._rate.x, this._rate.y, this._rate.z]
    }

    public displayName(): string {
        return `${this.info?.name ?? "-"} [Gyro]`
    }
}

export default GyroStimulus
