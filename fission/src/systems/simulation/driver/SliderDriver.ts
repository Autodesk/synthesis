import Jolt from "@barclah/jolt-physics"
import Driver from "./Driver"
import { GetLastDeltaT } from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import InputSystem from "@/systems/input/InputSystem"

class SliderDriver extends Driver {
    private _constraint: Jolt.SliderConstraint
    private _targetPosition: number = 0.0

    public get targetPosition(): number {
        return this._targetPosition
    }
    public set targetPosition(position: number) {
        this._targetPosition = Math.max(
            this._constraint.GetLimitsMin(),
            Math.min(this._constraint.GetLimitsMax(), position)
        )
    }

    public set minForceLimit(newtons: number) {
        const motorSettings = this._constraint.GetMotorSettings()
        motorSettings.mMinForceLimit = newtons
    }
    public set maxForceLimit(newtons: number) {
        const motorSettings = this._constraint.GetMotorSettings()
        motorSettings.mMaxForceLimit = newtons
    }

    public constructor(constraint: Jolt.SliderConstraint) {
        super()

        this._constraint = constraint

        const motorSettings = this._constraint.GetMotorSettings()
        const springSettings = motorSettings.mSpringSettings
        springSettings.mFrequency = 20 * (1.0 / GetLastDeltaT())
        springSettings.mDamping = 0.995

        motorSettings.mSpringSettings = springSettings
        motorSettings.mMinForceLimit = -125.0
        motorSettings.mMaxForceLimit = 125.0

        this._constraint.SetMotorState(JOLT.EMotorState_Position)

        this.targetPosition = this._constraint.GetCurrentPosition()
    }

    public Update(_: number): void {
        this._targetPosition +=
            ((InputSystem.getInput("sliderUp") ? 1 : 0) - (InputSystem.getInput("sliderDown") ? 1 : 0)) * 3
        this._constraint.SetTargetPosition(this._targetPosition)
    }
}

export default SliderDriver
