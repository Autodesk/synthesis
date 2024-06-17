import Jolt from "@barclah/jolt-physics";
import Driver from "./Driver";
import JOLT from "@/util/loading/JoltSyncLoader";
import InputSystem from "@/systems/input/InputSystem";

class WheelDriver extends Driver {

    private _constraint: Jolt.VehicleConstraint;
    private _wheel: Jolt.WheelWV;

    private _targetWheelSpeed: number = 0.0;

    public get targetWheelSpeed(): number {
        return this._targetWheelSpeed;
    }
    public set targetWheelSpeed(radsPerSec: number) {
        this._targetWheelSpeed = radsPerSec;
    }

    public constructor(constraint: Jolt.VehicleConstraint) {
        super();

        this._constraint = constraint;
        this._wheel = JOLT.castObject(this._constraint.GetWheel(0), JOLT.WheelWV);

        if (constraint.GetVehicleBody().GetCenterOfMassPosition().GetX() < 0) {
            this._targetWheelSpeed = 0;
        } else {
            this._targetWheelSpeed = 0;
        }
    }

    public Update(_: number): void {
        this._targetWheelSpeed = InputSystem.getInput('arcadeForward') ? 50 : InputSystem.getInput('arcadeBackward') ? -50 : 0;
        
        this._wheel.SetAngularVelocity(this._targetWheelSpeed);
    }
}

export default WheelDriver;