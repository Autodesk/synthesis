import type Jolt from "@azaleacolburn/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import { getLastDeltaT } from "@/systems/physics/PhysicsSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { type NoraNumber, NoraTypes } from "../Nora"
import Driver, { DriverControlMode, type DriverID } from "./Driver"

const MAX_FORCE_WITHOUT_GRAV = 500

class SliderDriver extends Driver {
    private _constraint: Jolt.SliderConstraint

    private _controlMode: DriverControlMode = DriverControlMode.VELOCITY
    private _targetPosition: number = 0.0
    private _maxForceWithGrav: number = 0.0
    public accelerationDirection: number = 0.0
    public maxVelocity: number = 1.0

    public get constraint(): Jolt.SliderConstraint {
        return this._constraint
    }

    private _prevPos: number = 0.0

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
            case DriverControlMode.VELOCITY:
                this._constraint.SetMotorState(JOLT.EMotorState_Velocity)
                break
            case DriverControlMode.POSITION:
                this._constraint.SetMotorState(JOLT.EMotorState_Position)
                break
            default:
                // idk
                break
        }
    }

    public constructor(id: DriverID, constraint: Jolt.SliderConstraint, maxVelocity: number, info?: mirabuf.IInfo) {
        super(id, info)

        this._constraint = constraint
        this.maxVelocity = maxVelocity

        const motorSettings = this._constraint.GetMotorSettings()
        const springSettings = motorSettings.mSpringSettings
        springSettings.mFrequency = 20 * (1.0 / getLastDeltaT())
        springSettings.mDamping = 0.999
        motorSettings.mSpringSettings = springSettings

        this._maxForceWithGrav = motorSettings.get_mMaxForceLimit()
        if (!PreferencesSystem.getGlobalPreference("SubsystemGravity")) {
            motorSettings.set_mMaxForceLimit(MAX_FORCE_WITHOUT_GRAV)
            motorSettings.set_mMinForceLimit(-MAX_FORCE_WITHOUT_GRAV)
        }

        this._constraint.SetMotorState(JOLT.EMotorState_Velocity)
        this.controlMode = DriverControlMode.VELOCITY

        PreferencesSystem.addPreferenceEventListener("SubsystemGravity", event => {
            const motorSettings = this._constraint.GetMotorSettings()
            if (event.prefValue) {
                motorSettings.set_mMaxForceLimit(this._maxForceWithGrav)
                motorSettings.set_mMinForceLimit(-this._maxForceWithGrav)
            } else {
                motorSettings.set_mMaxForceLimit(MAX_FORCE_WITHOUT_GRAV)
                motorSettings.set_mMinForceLimit(-MAX_FORCE_WITHOUT_GRAV)
            }
        })
    }

    public update(_: number): void {
        if (this._controlMode == DriverControlMode.VELOCITY) {
            this._constraint.SetTargetVelocity(this.accelerationDirection * this.maxVelocity)
        } else if (this._controlMode == DriverControlMode.POSITION) {
            let pos = this._targetPosition

            if (pos - this._prevPos < -this.maxVelocity) pos = this._prevPos - this.maxVelocity
            if (pos - this._prevPos > this.maxVelocity) pos = this._prevPos + this.maxVelocity

            this._constraint.SetTargetPosition(pos)
        }
    }

    public getReceiverType(): NoraTypes {
        return NoraTypes.NUMBER
    }
    public setReceiverValue(val: NoraNumber): void {
        this.accelerationDirection = val
    }
    public displayName(): string {
        return `${this.info?.name ?? "-"} [Slider]`
    }
}

export default SliderDriver
