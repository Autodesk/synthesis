import type Driver from "@/systems/simulation/driver/Driver"
import type Stimulus from "@/systems/simulation/stimulus/Stimulus"

export enum DriveType {
    ARCADE = "Arcade",
    TANK = "Tank",
    SWERVE = "Swerve",
}

abstract class Behavior {
    protected readonly _drivers: Driver[]
    protected readonly _stimuli: Stimulus[]

    constructor(drivers: Driver[], stimuli: Stimulus[]) {
        this._drivers = drivers
        this._stimuli = stimuli
    }

    public abstract update(deltaT: number): void
}

export default Behavior
