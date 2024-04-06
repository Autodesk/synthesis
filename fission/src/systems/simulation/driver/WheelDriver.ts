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

    private _timeAccum = 0;
    public Update(deltaT: number): void {
        this._controller.SetForwardInput(50);
    }
}

export default WheelDriver;