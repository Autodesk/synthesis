import Jolt from "@azaleacolburn/jolt-physics"
import Driver, { DriverID } from "./Driver"
import JOLT from "@/util/loading/JoltSyncLoader"
import { SimType } from "../wpilib_brain/WPILibBrain"
import { mirabuf } from "@/proto/mirabuf"
import { NoraNumber, NoraTypes } from "../Nora"
import World from "@/systems/World"
import { JoltRVec3_ThreeVector3, JoltVec3_JoltRVec3, JoltVec3_ThreeVector3 } from "@/util/TypeConversions"
import THREE from "three"

const LATERIAL_FRICTION = 1.0
const LONGITUDINAL_FRICTION = 1.0

class WheelDriver extends Driver {
    private _constraint: Jolt.VehicleConstraint
    private _wheel: Jolt.WheelWV
    public deviceType?: SimType
    public device?: string
    private _reversed: boolean

    public accelerationDirection: number = 0.0
    private _prevVel: number = 0.0
    public maxVelocity = 30.0
    private _maxAcceleration = 1.5

    public _targetVelocity = (deltaT: number) => {
        let vel = this.accelerationDirection * (this._reversed ? -1 : 1) * this.maxVelocity

        let realAccel = (vel - this._prevVel) / deltaT

        if (realAccel < -this._maxAcceleration) vel = this._prevVel - this._maxAcceleration
        if (realAccel > this._maxAcceleration) vel = this._prevVel + this._maxAcceleration

        return vel
    }

    public get maxForce(): number {
        return this._maxAcceleration
    }
    public set maxForce(acc: number) {
        this._maxAcceleration = acc
    }

    public get constraint(): Jolt.VehicleConstraint {
        return this._constraint
    }

    public constructor(
        id: DriverID,
        constraint: Jolt.VehicleConstraint,
        maxVel: number,
        info?: mirabuf.IInfo,
        deviceType?: SimType,
        device?: string,
        reversed: boolean = false
    ) {
        super(id, info)

        this._constraint = constraint
        this.maxVelocity = maxVel
        const controller = JOLT.castObject(this._constraint.GetController(), JOLT.WheeledVehicleController)
        this._maxAcceleration = controller.GetEngine().mMaxTorque

        this._reversed = reversed
        this.deviceType = deviceType
        this.device = device
        this._wheel = JOLT.castObject(this._constraint.GetWheel(0), JOLT.WheelWV)
        this._wheel.set_mCombinedLateralFriction(LATERIAL_FRICTION * 10)
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION * 10)

        // const wheelUp = this._wheel.GetSettings().get_mWheelForward()
        // console.log(`x: ${wheelUp.GetX()} y: ${wheelUp.GetY()} z: ${wheelUp.GetZ()}`)
    }

    public Update(deltaT: number): void {
        const vel = this._targetVelocity(deltaT)
        if (vel != 0) console.log(`deltaT: ${deltaT} vel: ${vel} name: ${this.info?.name}`)
        this._wheel.SetAngularVelocity(vel)
        this._prevVel = vel
    }

    private visualizeVector(): void {
        // I have no idea how to get the unit vector for angular velocity since there
        const unit = this._wheel
            .GetSettings()
            .get_mWheelForward()
            .Cross(this._wheel.GetSettings().get_mWheelUp())
            .Mul(this._wheel.GetAngularVelocity())

        const start = JoltRVec3_ThreeVector3(this._wheel.GetContactPosition())
        const end = JoltVec3_ThreeVector3(unit.Cross(this._wheel.GetSettings().get_mWheelUp()))

        const geometry = new THREE.BufferGeometry().setFromPoints([start, end])
        const vector = new THREE.Line()
        World.SceneRenderer.AddObject()
    }

    public set reversed(val: boolean) {
        this._reversed = val
    }

    public getReceiverType(): NoraTypes {
        return NoraTypes.Number
    }
    public setReceiverValue(val: NoraNumber): void {
        this.accelerationDirection = val
    }
    public DisplayName(): string {
        return `${this.info?.name ?? "-"} [Wheel]`
    }
}

export default WheelDriver
