import Jolt from "@barclah/jolt-physics";
import Driver from "./Driver";
import { SIMULATION_PERIOD } from "@/systems/physics/PhysicsSystem";
import JOLT from "@/util/loading/JoltSyncLoader";

class HingeDriver extends Driver {

    private _constraint: Jolt.HingeConstraint;

    public constructor(constraint: Jolt.HingeConstraint) {
        super();

        this._constraint = constraint;

        const motorSettings = this._constraint.GetMotorSettings();
        const springSettings = motorSettings.mSpringSettings;
        springSettings.mFrequency = 20 * (1.0 / SIMULATION_PERIOD);
        springSettings.mDamping = 0.995;

        motorSettings.mSpringSettings = springSettings;
        motorSettings.mMinTorqueLimit = -125.0;
        motorSettings.mMaxTorqueLimit = 125.0;

        this._constraint.SetMotorState(JOLT.EMotorState_Velocity);
    }

    private _timeAccum = 0;
    public Update(deltaT: number): void {
        this._timeAccum += deltaT;
        const vel = Math.sin(this._timeAccum * 0.8) * 0.5;
        console.log(`Ang Vel: ${vel}`);
        this._constraint.SetTargetAngularVelocity(vel);

        if (!this._constraint.GetBody2().IsActive()) {
            console.log("Asleep");
        }
    }
}

export default HingeDriver;