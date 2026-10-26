import type { mirabuf } from "@/proto/mirabuf"
import Stimulus, { type StimulusID } from "./Stimulus"
import type { NoraBaseType, NoraBaseValueOf } from "../Nora"

abstract class EncoderStimulus<T extends readonly [NoraBaseType, NoraBaseType]> extends Stimulus<T> {
    public abstract get positionValue(): NoraBaseValueOf<T[0]>
    public abstract get velocityValue(): NoraBaseValueOf<T[1]>

    protected constructor(id: StimulusID, info?: mirabuf.IInfo) {
        super(id, info)
    }

    public abstract override update(deltaT: number): void
}

export default EncoderStimulus
