import Jolt from "@barclah/jolt-physics";
import Driver from "./Driver";
import { SIMULATION_PERIOD } from "@/systems/physics/PhysicsSystem";
import JOLT from "@/util/loading/JoltSyncLoader";

class HingeDriver extends Driver {

    private _constraint: Jolt.HingeConstraint;

    private _targetVelocity: number = 0.0;

    public get targetVelocity(): number {
        return this._targetVelocity;
    }
    public set targetVelocity(radsPerSec: number) {
        this._targetVelocity = radsPerSec;
    }

    public set minTorqueLimit(nm: number) {
        const motorSettings = this._constraint.GetMotorSettings();
        motorSettings.mMinTorqueLimit = nm;
    }
    public set maxTorqueLimit(nm: number) {
        const motorSettings = this._constraint.GetMotorSettings();
        motorSettings.mMaxTorqueLimit = nm;
    }

    public constructor(constraint: Jolt.HingeConstraint) {
        super();

        this._constraint = constraint;

        const motorSettings = this._constraint.GetMotorSettings();
        const springSettings = motorSettings.mSpringSettings;
        springSettings.mFrequency = 20 * (1.0 / SIMULATION_PERIOD);
        springSettings.mDamping = 0.995;

        motorSettings.mSpringSettings = springSettings;
        motorSettings.mMinTorqueLimit = -50.0;
        motorSettings.mMaxTorqueLimit = 50.0;

        this._constraint.SetMotorState(JOLT.EMotorState_Velocity);
    }

    public Update(_: number): void {
        this._constraint.SetTargetAngularVelocity(this._targetVelocity);

    }
}

export default HingeDriver;