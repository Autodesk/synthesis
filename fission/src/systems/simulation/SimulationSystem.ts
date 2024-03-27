import Mechanism from "../physics/Mechanism";
import WorldSystem from "../WorldSystem";
import Brain from "./Brain";
import Driver from "./Driver";
import Stimulus from "./Stimulus";

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
        this._simMechanisms.forEach(simLayer => simLayer.brain?.Update(deltaT));
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
        this._stimuli = [];
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