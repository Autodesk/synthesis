import JOLT from "@/util/loading/JoltSyncLoader";
import Mechanism from "../physics/Mechanism";
import WorldSystem from "../WorldSystem";
import Brain from "./Brain";
import Driver from "./driver/Driver";
import Stimulus from "./stimulus/Stimulus";
import HingeDriver from "./driver/HingeDriver";
import WheelDriver from "./driver/WheelDriver";

class SimulationSystem extends WorldSystem {

    private _simMechanisms: Map<Mechanism, SimulationLayer>;

    constructor() {
        super();

        this._simMechanisms = new Map();
    }

    public RegisterMechanism(mechanism: Mechanism) {
        if (this._simMechanisms.has(mechanism))
            return;

        this._simMechanisms.set(mechanism, new SimulationLayer(mechanism));
    }

    public GetSimulationLayer(mechanism: Mechanism): SimulationLayer | undefined {
        return this._simMechanisms.get(mechanism);
    }

    public Update(deltaT: number): void {
        this._simMechanisms.forEach(simLayer => simLayer.Update(deltaT));
    }

    public Destroy(): void {
        this._simMechanisms.forEach(simLayer => simLayer.SetBrain(undefined));
        this._simMechanisms.clear();
    }

}

class SimulationLayer {
    private _mechanism: Mechanism;
    private _brain?: Brain;

    private _drivers: Driver[];
    private _stimuli: Stimulus[];

    public get brain() { return this._brain; }

    constructor(mechanism: Mechanism) {
        this._mechanism = mechanism;

        // Generate standard drivers and stimuli
        this._drivers = [];
        this._mechanism.constraints.forEach(x => {
            if (x.constraint.GetSubType() == JOLT.EConstraintSubType_Hinge) {
                const hinge = JOLT.castObject(x.constraint, JOLT.HingeConstraint);
                const driver = new HingeDriver(hinge);
                this._drivers.push(driver);
            } else if (x.constraint.GetSubType() == JOLT.EConstraintSubType_Vehicle) {
                const vehicle = JOLT.castObject(x.constraint, JOLT.VehicleConstraint);
                const driver = new WheelDriver(vehicle);
                this._drivers.push(driver);
            }
        });

        this._stimuli = [];
    }

    public Update(deltaT: number) {
        this._brain?.Update(deltaT);
        this._drivers.forEach(x => x.Update(deltaT));
        this._stimuli.forEach(x => x.Update(deltaT));
    }

    public SetBrain<T extends Brain>(brain: T | undefined) {
        if (this._brain)
            this._brain.Disable();

        this._brain = brain;
        
        if (this._brain)
            this._brain.Enable();
    }
}

export default SimulationSystem;