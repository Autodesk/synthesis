import Jolt from "@barclah/jolt-physics";
import Driver from "./Driver";
import { SIMULATION_PERIOD } from "@/systems/physics/PhysicsSystem";
import JOLT from "@/util/loading/JoltSyncLoader";

class WheelDriver extends Driver {

    private _constraint: Jolt.VehicleConstraint;
    private _wheel: Jolt.WheelWV;

    private _targetWheelSpeed: number = 0.0;

    public get actualWheelSpeed(): number {
        return this._wheel.GetAngularVelocity();
    }
    public get targetWheelSpeed(): number {
        return this._targetWheelSpeed;
    }
    public set targetWheelSpeed(radsPerSec: number) {
        this._wheel.SetAngularVelocity(radsPerSec);
    }

    public constructor(constraint: Jolt.VehicleConstraint) {
        super();

        this._constraint = constraint;
        this._wheel = JOLT.castObject(this._constraint.GetWheel(0), JOLT.WheelWV);
    }

    public Update(deltaT: number): void {
        this._wheel.SetAngularVelocity(this._targetWheelSpeed);

        const current = this._constraint.GetVehicleBody().GetLinearVelocity();
        console.log(`Speed: ${current.Length()}`);

        if (!this._constraint.GetVehicleBody().IsActive()) {
            console.log("Asleep");
        }
    }
}

export default WheelDriver;