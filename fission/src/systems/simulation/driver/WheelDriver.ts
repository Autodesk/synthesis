import Jolt from "@barclah/jolt-physics";
import Driver from "./Driver";
import { SIMULATION_PERIOD } from "@/systems/physics/PhysicsSystem";
import JOLT from "@/util/loading/JoltSyncLoader";

class WheelDriver extends Driver {

    private _constraint: Jolt.VehicleConstraint;
    private _controller: Jolt.WheeledVehicleController;

    public constructor(constraint: Jolt.VehicleConstraint) {
        super();

        this._constraint = constraint;

        this._controller = JOLT.castObject(this._constraint.GetController(), JOLT.WheeledVehicleController);
    }

    public Update(deltaT: number): void {
        // this._controller.GetEngine().SetCurrentRPM(1000);
        // this._controller.SetDriverInput(1.0, 0.0, 0.0, 0.0);

        // this._constraint.GetWheel(0).SetAngularVelocity(10);

        const current = this._constraint.GetVehicleBody().GetLinearVelocity();
        console.log(`Speed: ${current.Length()}`);

        if (!this._constraint.GetVehicleBody().IsActive()) {
            console.log("Asleep");
        }
    }
}

export default WheelDriver;