import Jolt from "@barclah/jolt-physics"
import Driver, { DriverControlMode } from "./Driver"
import { GetLastDeltaT } from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { mirabuf } from "@/proto/mirabuf"

class HingeDriver extends Driver {
    private _constraint: Jolt.HingeConstraint

    private _controlMode: DriverControlMode = DriverControlMode.Velocity
    private _targetAngle: number
    public accelerationDirection: number = 0.0
    public maxVelocity: number

    private _prevAng: number = 0.0

    public get targetAngle(): number {
        return this._targetAngle
    }
    public set targetAngle(rads: number) {
        this._targetAngle = Math.max(this._constraint.GetLimitsMin(), Math.min(this._constraint.GetLimitsMax(), rads))
    }

    public get maxForce() {
        return this._constraint.GetMotorSettings().mMaxTorqueLimit
    }

    public set maxForce(nm: number) {
        const motorSettings = this._constraint.GetMotorSettings()
        motorSettings.mMaxTorqueLimit = nm
        motorSettings.mMinTorqueLimit = -nm
    }

    public get controlMode(): DriverControlMode {
        return this._controlMode
    }

    public set controlMode(mode: DriverControlMode) {
        this._controlMode = mode
        switch (mode) {
            case DriverControlMode.Velocity:
                this._constraint.SetMotorState(JOLT.EMotorState_Velocity)
                break
            case DriverControlMode.Position:
                this._constraint.SetMotorState(JOLT.EMotorState_Position)
                break
            default:
                // idk
                break
        }
    }

    public constructor(constraint: Jolt.HingeConstraint, maxVelocity: number, info?: mirabuf.IInfo) {
        super(info)

        this._constraint = constraint
        this.maxVelocity = maxVelocity
        this._targetAngle = this._constraint.GetCurrentAngle()

        const motorSettings = this._constraint.GetMotorSettings()
        const springSettings = motorSettings.mSpringSettings

        // These values were selected based on the suggestions of the documentation for stiff control.
        springSettings.mFrequency = 20 * (1.0 / GetLastDeltaT())
        springSettings.mDamping = 0.995
        motorSettings.mSpringSettings = springSettings

        this.controlMode = DriverControlMode.Velocity
    }

    public Update(_: number): void {
        if (this._controlMode == DriverControlMode.Velocity) {
            this._constraint.SetTargetAngularVelocity(this.accelerationDirection * this.maxVelocity)
        } else if (this._controlMode == DriverControlMode.Position) {
            let ang = this._targetAngle

            if (ang - this._prevAng < -this.maxVelocity) ang = this._prevAng - this.maxVelocity
            if (ang - this._prevAng > this.maxVelocity) ang = this._prevAng + this.maxVelocity
            this._constraint.SetTargetAngle(ang)
        }
    }
}

export default HingeDriver
