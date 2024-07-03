import Mechanism from "../physics/Mechanism"

abstract class Brain {
    protected _mechanism: Mechanism

    constructor(mechansim: Mechanism) {
        this._mechanism = mechansim
    }

    public abstract Update(deltaT: number): void

    public abstract Enable(): void
    public abstract Disable(): void
}

export default Brain
