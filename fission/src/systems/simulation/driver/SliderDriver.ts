import Jolt from "@barclah/jolt-physics"
import Driver, { DriverControlMode } from "./Driver"
import { GetLastDeltaT } from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { mirabuf } from "@/proto/mirabuf"

class SliderDriver extends Driver {
    private _constraint: Jolt.SliderConstraint

    private _controlMode: DriverControlMode = DriverControlMode.Velocity
    private _targetVelocity: number = 0.0
    private _targetPosition: number = 0.0
    private _maxVelocity: number = 1.0

    public get maxVelocity(): number {
        return this._maxVelocity
    }

    public set maxVelocity(radsPerSec: number) {
        this._maxVelocity = radsPerSec
    }

    public get targetVelocity(): number {
        return this._targetVelocity
    }
    public set targetVelocity(radsPerSec: number) {
        this._targetVelocity = radsPerSec
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

    public get minForceLimit(): number {
        return this._constraint.GetMotorSettings().get_mMinForceLimit()
    }
    public set minForceLimit(newtons: number) {
        const motorSettings = this._constraint.GetMotorSettings()
        motorSettings.mMinForceLimit = newtons
    }
    public get maxForceLimit(): number {
        return this._constraint.GetMotorSettings().get_mMaxForceLimit()
    }
    public set maxForceLimit(newtons: number) {
        const motorSettings = this._constraint.GetMotorSettings()
        motorSettings.mMaxForceLimit = newtons
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

        const motorSettings = this._constraint.GetMotorSettings()
        const springSettings = motorSettings.mSpringSettings
        springSettings.mFrequency = 20 * (1.0 / GetLastDeltaT())
        springSettings.mDamping = 0.999

        motorSettings.mSpringSettings = springSettings

        this._maxVelocity = maxVelocity

        this._constraint.SetMotorState(JOLT.EMotorState_Velocity)
        this.controlMode = DriverControlMode.Velocity
    }

    public Update(_: number): void {
        if (this._controlMode == DriverControlMode.Velocity) {
            this._constraint.SetTargetVelocity(this._targetVelocity * this._maxVelocity)
        } else if (this._controlMode == DriverControlMode.Position) {
            //TODO: MaxVel checks diff
            this._constraint.SetTargetPosition(this._targetPosition)
        }
    }
}

export default SliderDriver
