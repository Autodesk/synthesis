import Jolt from "@barclah/jolt-physics"
import Driver from "./Driver"
import JOLT from "@/util/loading/JoltSyncLoader"
import { SimType } from "../wpilib_brain/WPILibBrain"
import { mirabuf } from "@/proto/mirabuf"

const LATERIAL_FRICTION = 0.6
const LONGITUDINAL_FRICTION = 0.8

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

    public _targetVelocity = () => {
        let vel = this.accelerationDirection * (this._reversed ? -1 : 1) * this.maxVelocity

        if (vel - this._prevVel < -this._maxAcceleration) vel = this._prevVel - this._maxAcceleration
        if (vel - this._prevVel > this._maxAcceleration) vel = this._prevVel + this._maxAcceleration

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
        constraint: Jolt.VehicleConstraint,
        maxVel: number,
        info?: mirabuf.IInfo,
        deviceType?: SimType,
        device?: string,
        reversed: boolean = false
    ) {
        super(info)

        this._constraint = constraint
        this.maxVelocity = maxVel
        const controller = JOLT.castObject(this._constraint.GetController(), JOLT.WheeledVehicleController)
        this._maxAcceleration = controller.GetEngine().mMaxTorque

        this._reversed = reversed
        this.deviceType = deviceType
        this.device = device
        this._wheel = JOLT.castObject(this._constraint.GetWheel(0), JOLT.WheelWV)
        this._wheel.set_mCombinedLateralFriction(LATERIAL_FRICTION)
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION)
    }

    public Update(_: number): void {
        const vel = this._targetVelocity()
        this._wheel.SetAngularVelocity(vel)
        this._prevVel = vel
    }

    public set reversed(val: boolean) {
        this._reversed = val
    }
}

export default WheelDriver
