import Mechanism from "../physics/Mechanism"

export type BrainType = "synthesis" | "wpilib" | "unknown"

abstract class Brain {
    protected _mechanism: Mechanism

    private _brainType: BrainType
    public get brainType() {
        return this._brainType
    }

    constructor(mechanism: Mechanism, brainType: BrainType) {
        this._mechanism = mechanism
        this._brainType = brainType
    }

    public abstract Update(deltaT: number): void

    public abstract Enable(): void
    public abstract Disable(): void
}

export default Brain
