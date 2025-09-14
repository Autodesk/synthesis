import type Mechanism from "../physics/Mechanism"

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

    public abstract update(deltaT: number): void

    public abstract enable(): void
    public abstract disable(): void
}

export default Brain
