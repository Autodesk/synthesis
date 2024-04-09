import Jolt from "@barclah/jolt-physics";
import Driver from "./Driver";
import { SIMULATION_PERIOD } from "@/systems/physics/PhysicsSystem";
import JOLT from "@/util/loading/JoltSyncLoader";

class SliderDriver extends Driver {

    private _constraint: Jolt.SliderConstraint;

    public constructor(constraint: Jolt.SliderConstraint) {
        super();

        this._constraint = constraint;

        const motorSettings = this._constraint.GetMotorSettings();
        const springSettings = motorSettings.mSpringSettings;
        springSettings.mFrequency = 20 * (1.0 / SIMULATION_PERIOD);
        springSettings.mDamping = 0.995;

        motorSettings.mSpringSettings = springSettings;
        motorSettings.mMinTorqueLimit = -125.0;
        motorSettings.mMaxTorqueLimit = 125.0;

        this._constraint.SetMotorState(JOLT.EMotorState_Position);
        this._constraint.SetTargetPosition(this._constraint.GetCurrentPosition());
    }

    private _flip = 1;
    public Update(deltaT: number): void {

        let targetPosition = this._constraint.GetTargetPosition() + (this._flip * deltaT * 0.05);
        if (targetPosition < this._constraint.GetLimitsMin()) {
            targetPosition = this._constraint.GetLimitsMin();
            this._flip *= -1;
        } else if (targetPosition > this._constraint.GetLimitsMax()) {
            targetPosition = this._constraint.GetLimitsMax();
            this._flip *= -1;
        }

        this._constraint.SetTargetPosition(targetPosition);
    }
}

export default SliderDriver;