import Driver from "@/systems/simulation/driver/Driver"
import Stimulus from "@/systems/simulation/stimulus/Stimulus"

abstract class Behavior {
    private _drivers: Driver[]
    private _stimuli: Stimulus[]

    protected get drivers() {
        return this._drivers
    }
    protected get stimuli() {
        return this._stimuli
    }

    constructor(drivers: Driver[], stimuli: Stimulus[]) {
        this._drivers = drivers
        this._stimuli = stimuli
    }

    public abstract Update(deltaT: number): void
}

export default Behavior
