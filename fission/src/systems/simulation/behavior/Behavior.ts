import type Driver from "@/systems/simulation/driver/Driver"
import type Stimulus from "@/systems/simulation/stimulus/Stimulus"

export enum DriveType {
    ARCADE = "Arcade",
    TANK = "Tank",
    SWERVE = "Swerve",
}

export enum SwerveReferenceType {
    ROBOTCENTRIC = "Robot",
    FIELDCENTRIC = "Field"
}

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

    public abstract update(deltaT: number): void
}

export default Behavior
