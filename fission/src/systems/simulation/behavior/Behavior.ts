import Driver from "../driver/Driver"
import Stimulus from "../stimulus/Stimulus"

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

export interface SequenceableBehavior {
    get jointIndex(): number
}

export default Behavior
