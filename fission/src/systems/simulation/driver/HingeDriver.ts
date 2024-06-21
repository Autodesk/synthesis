import Jolt from "@barclah/jolt-physics";
import Driver from "./Driver";
import { SIMULATION_PERIOD } from "@/systems/physics/PhysicsSystem";
import JOLT from "@/util/loading/JoltSyncLoader";

class HingeDriver extends Driver {

    private _constraint: Jolt.HingeConstraint;

    private _controlMode: DriverControlMode = DriverControlMode.Velocity;
    private _targetVelocity: number = 0.0;
    private _targetAngle: number;

    public get targetVelocity(): number {
        return this._targetVelocity;
    }
    public set targetVelocity(radsPerSec: number) {
        this._targetVelocity = radsPerSec;
    }

    public get targetAngle(): number {
        return this._targetAngle;
    }
    public set targetAngle(rads: number) {
        this._targetAngle = rads;
    }

    public set minTorqueLimit(nm: number) {
        const motorSettings = this._constraint.GetMotorSettings();
        motorSettings.mMinTorqueLimit = nm;
    }
    public set maxTorqueLimit(nm: number) {
        const motorSettings = this._constraint.GetMotorSettings();
        motorSettings.mMaxTorqueLimit = nm;
    }

    public get controlMode(): DriverControlMode {
        return this._controlMode;
    }
    public set controlMode(mode: DriverControlMode) {
        this._controlMode = mode;
        switch (mode) {
            case DriverControlMode.Velocity:
                this._constraint.SetMotorState(JOLT.EMotorState_Velocity);
                break;
            case DriverControlMode.Position:
                this._constraint.SetMotorState(JOLT.EMotorState_Position);
                break;
            default:
                // idk
                break;
        }
    }

    public constructor(constraint: Jolt.HingeConstraint) {
        super();

        this._constraint = constraint;

        const motorSettings = this._constraint.GetMotorSettings();
        const springSettings = motorSettings.mSpringSettings;

        // These values were selected based on the suggestions of the documentation for stiff control.
        springSettings.mFrequency = 20 * (1.0 / SIMULATION_PERIOD);
        springSettings.mDamping = 0.995;

        motorSettings.mSpringSettings = springSettings;
        motorSettings.mMinTorqueLimit = -50.0;
        motorSettings.mMaxTorqueLimit = 50.0;

        this._targetAngle = this._constraint.GetCurrentAngle();

        this.controlMode = DriverControlMode.Velocity;
    }

    public Update(_: number): void {
        if (this._controlMode == DriverControlMode.Velocity) {
            this._constraint.SetTargetAngularVelocity(this._targetVelocity);
        } else if (this._controlMode == DriverControlMode.Position) {
            this._constraint.SetTargetAngle(this._targetAngle);
        }

    }
}

export enum DriverControlMode {
    Velocity = 0,
    Position = 1
}

export default HingeDriver;