import Driver from "../driver/Driver";
import Stimulus from "../stimulus/Stimulus";

abstract class Behavior {

    private _drivers: Driver[];
    private _stimuli: Stimulus[];

    constructor(drivers: Driver[], stimuli: Stimulus[]) {
        this._drivers = drivers;
        this._stimuli = stimuli;
    }
}

export default Behavior;