import Jolt from "@barclah/jolt-physics"
import Driver, { DriverControlMode } from "./Driver"
import { GetLastDeltaT } from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { mirabuf } from "@/proto/mirabuf"
import PreferencesSystem, { PreferenceEvent } from "@/systems/preferences/PreferencesSystem"

const MAX_FORCE_WITHOUT_GRAV = 500

class SliderDriver extends Driver {
    private _constraint: Jolt.SliderConstraint

    private _controlMode: DriverControlMode = DriverControlMode.Velocity
    private _targetPosition: number = 0.0
    private _maxForceWithGrav: number = 0.0
    public accelerationDirection: number = 0.0
    public maxVelocity: number = 1.0

    private _prevPos: number = 0.0

    private _gravityChange?: (event: PreferenceEvent) => void


    public get targetPosition(): number {
        return this._targetPosition
    }
    public set targetPosition(position: number) {
        this._targetPosition = Math.max(
            this._constraint.GetLimitsMin(),
            Math.min(this._constraint.GetLimitsMax(), position)
        )
    }

    public get maxForce(): number {
        return this._constraint.GetMotorSettings().mMaxForceLimit
    }
    public set maxForce(newtons: number) {
        const motorSettings = this._constraint.GetMotorSettings()
        motorSettings.set_mMaxForceLimit(newtons)
        motorSettings.set_mMinForceLimit(-newtons)
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
        this.maxVelocity = maxVelocity

        const motorSettings = this._constraint.GetMotorSettings()
        const springSettings = motorSettings.mSpringSettings
        springSettings.mFrequency = 20 * (1.0 / GetLastDeltaT())
        springSettings.mDamping = 0.999
        motorSettings.mSpringSettings = springSettings

        this._maxForceWithGrav = motorSettings.get_mMaxForceLimit()
        if (!PreferencesSystem.getGlobalPreference("SubsystemGravity")) {
            motorSettings.set_mMaxForceLimit(MAX_FORCE_WITHOUT_GRAV)
            motorSettings.set_mMinForceLimit(-MAX_FORCE_WITHOUT_GRAV)
        }

        this._constraint.SetMotorState(JOLT.EMotorState_Velocity)
        this.controlMode = DriverControlMode.Velocity

        this._gravityChange = (event: PreferenceEvent) => {
            if (event.prefName == "SubsystemGravity") {
                const motorSettings = this._constraint.GetMotorSettings()
                if (event.prefValue) {
                    motorSettings.set_mMaxForceLimit(this._maxForceWithGrav)
                    motorSettings.set_mMinForceLimit(-this._maxForceWithGrav)
                } else {
                    motorSettings.set_mMaxForceLimit(MAX_FORCE_WITHOUT_GRAV)
                    motorSettings.set_mMinForceLimit(-MAX_FORCE_WITHOUT_GRAV)
                }
            }
        }

        PreferencesSystem.addEventListener(this._gravityChange)
    }

    public Update(_: number): void {
        if (this._controlMode == DriverControlMode.Velocity) {
            this._constraint.SetTargetVelocity(this.accelerationDirection * this.maxVelocity)
        } else if (this._controlMode == DriverControlMode.Position) {
            let pos = this._targetPosition

            if (pos - this._prevPos < -this.maxVelocity) pos = this._prevPos - this.maxVelocity
            if (pos - this._prevPos > this.maxVelocity) pos = this._prevPos + this.maxVelocity

            this._constraint.SetTargetPosition(pos)
        }
    }
}

export default SliderDriver
