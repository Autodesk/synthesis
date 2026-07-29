import type Mechanism from "../physics/Mechanism"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain.ts"
import type WPILibBrain from "@/systems/simulation/wpilib_brain/WPILibBrain.ts"

export type BrainType = "synthesis" | "wpilib" | "unknown"

abstract class Brain {
    protected _mechanism: Mechanism

    public abstract get brainType(): BrainType

    constructor(mechanism: Mechanism) {
        this._mechanism = mechanism
    }

    public abstract update(deltaT: number): void

    public abstract enable(): void
    public abstract disable(): void

    public isSynthesis(): this is SynthesisBrain {
        return this.brainType == "synthesis"
    }
    public isWPILib(): this is WPILibBrain {
        return this.brainType == "wpilib"
    }
}

export default Brain
