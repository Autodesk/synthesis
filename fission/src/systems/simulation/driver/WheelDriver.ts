import Jolt from "@barclah/jolt-physics";
import Driver from "./Driver";
import JOLT from "@/util/loading/JoltSyncLoader";

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
        
        console.log(`Wheel X: ${constraint.GetVehicleBody().GetCenterOfMassPosition().GetX().toFixed(5)}`);
        if (constraint.GetVehicleBody().GetCenterOfMassPosition().GetX() < 0) {
            this._targetWheelSpeed = 10.0;
        } else {
            this._targetWheelSpeed = 10.0;
        }
    }

    public Update(_: number): void {
        this._wheel.SetAngularVelocity(this._targetWheelSpeed);
    }
}

export default WheelDriver;