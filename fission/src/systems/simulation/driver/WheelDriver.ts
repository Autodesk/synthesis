import type Jolt from "@azaleacolburn/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import JOLT from "@/util/loading/JoltSyncLoader"
import { type NoraNumber, NoraTypes } from "../Nora"
import type { SimType } from "../wpilib_brain/WPILibTypes"
import Driver, { type DriverID } from "./Driver"

const LATERIAL_FRICTION = 1.0
const LONGITUDINAL_FRICTION = 1.0

class WheelDriver extends Driver {
    private _constraint: Jolt.VehicleConstraint
    private _wheel: Jolt.WheelWV
    public deviceType?: SimType
    public device?: string
    private _reversed: boolean

    // Friction curves captured at construction, used to toggle wheel friction on/off
    // for swerve modules (azimuth wheels must be able to slip while rotating to angle).
    private _normalFrictionLong: Jolt.LinearCurve
    private _normalFrictionLat: Jolt.LinearCurve
    private _noFriction: Jolt.LinearCurve

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
        this._wheel.set_mCombinedLateralFriction(LATERIAL_FRICTION)
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION)

        // Capture the wheel's normal friction curves so they can be toggled off and
        // restored later (used by swerve to let modules pivot without lateral grip).
        this._normalFrictionLong = this._wheel.GetSettings().get_mLongitudinalFriction()
        this._normalFrictionLat = this._wheel.GetSettings().get_mLateralFriction()
        this._noFriction = new JOLT.LinearCurve()
        this._noFriction.AddPoint(0, 0)
        this._noFriction.AddPoint(10000, 0)
    }

    /** Enables or disables the wheel's longitudinal and lateral friction curves. */
    public setFrictionEnabled(enabledLong: boolean, enabledLat: boolean = enabledLong) {
        this._wheel.GetSettings().set_mLongitudinalFriction(enabledLong ? this._normalFrictionLong : this._noFriction)
        this._wheel.GetSettings().set_mLateralFriction(enabledLat ? this._normalFrictionLat : this._noFriction)
    }

    /** Sets the wheel's steer angle directly on the underlying Jolt wheel. */
    public setSteeringAngle(angle: number) {
        this._wheel.SetSteerAngle(angle)
    }

    /** @returns the underlying Jolt wheel for this driver. */
    public getWheel(): Jolt.WheelWV {
        return this._wheel
    }

    public update(_: number): void {
        const vel = this._targetVelocity()
        this._wheel.SetAngularVelocity(vel)
        this._prevVel = vel
    }

    public set reversed(val: boolean) {
        this._reversed = val
    }
    public get reversed(): boolean {
        return this._reversed
    }

    public getReceiverType(): NoraTypes {
        return NoraTypes.NUMBER
    }
    public setReceiverValue(val: NoraNumber): void {
        this.accelerationDirection = val
    }
    public displayName(): string {
        return `${this.info?.name ?? "-"} [Wheel]`
    }
}

export default WheelDriver
