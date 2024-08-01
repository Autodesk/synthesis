import Jolt from "@barclah/jolt-physics"
import Driver, { DriverControlMode } from "./Driver"
import { GetLastDeltaT } from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { mirabuf } from "@/proto/mirabuf"

class SliderDriver extends Driver {
    private _constraint: Jolt.SliderConstraint

    private _controlMode: DriverControlMode = DriverControlMode.Velocity
    private _accelerationDirection: number = 0.0
    private _targetPosition: number = 0.0
    private _maxVelocity: number = 1.0

    private _prevPos: number = 0.0

    public get accelerationDirection(): number {
        return this._accelerationDirection
    }
    public set accelerationDirection(radsPerSec: number) {
        this._accelerationDirection = radsPerSec
    }

    public get targetPosition(): number {
        return this._targetPosition
    }
    public set targetPosition(position: number) {
        this._targetPosition = Math.max(
            this._constraint.GetLimitsMin(),
            Math.min(this._constraint.GetLimitsMax(), position)
        )
    }

    public get maxVelocity(): number {
        return this._maxVelocity
    }
    public set maxVelocity(radsPerSec: number) {
        this._maxVelocity = radsPerSec
    }

    public get maxForce(): number {
        return this._constraint.GetMotorSettings().mMaxForceLimit
    }
    public set maxForce(newtons: number) {
        const motorSettings = this._constraint.GetMotorSettings()
        motorSettings.mMaxForceLimit = newtons
        motorSettings.mMinForceLimit = -newtons
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

    public constructor(constraint: Jolt.SliderConstraint, maxVelocity: number, info?: mirabuf.IInfo) {
        super(info)

        this._constraint = constraint
        this._maxVelocity = maxVelocity

        const motorSettings = this._constraint.GetMotorSettings()
        const springSettings = motorSettings.mSpringSettings
        springSettings.mFrequency = 20 * (1.0 / GetLastDeltaT())
        springSettings.mDamping = 0.999
        motorSettings.mSpringSettings = springSettings

        this._constraint.SetMotorState(JOLT.EMotorState_Velocity)
        this.controlMode = DriverControlMode.Velocity
    }

    public Update(_: number): void {
        if (this._controlMode == DriverControlMode.Velocity) {
            this._constraint.SetTargetVelocity(this._accelerationDirection * this._maxVelocity)
        } else if (this._controlMode == DriverControlMode.Position) {
            let pos = this._targetPosition
            
            if (pos - this._prevPos < -this.maxVelocity) pos = this._prevPos - this._maxVelocity
            if (pos - this._prevPos > this.maxVelocity) pos = this._prevPos + this._maxVelocity
            
            this._constraint.SetTargetPosition(pos)
        }
    }
}

export default SliderDriver
