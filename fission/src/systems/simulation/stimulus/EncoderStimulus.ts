import type { mirabuf } from "@/proto/mirabuf"
import Stimulus, { type StimulusID } from "./Stimulus"
import type { NoraType } from "../Nora"

abstract class EncoderStimulus<T extends NoraType = NoraType> extends Stimulus<T> {
    public abstract get positionValue(): number
    public abstract get velocityValue(): number

    protected constructor(id: StimulusID, info?: mirabuf.IInfo) {
        super(id, info)
    }

    public abstract override update(deltaT: number): void
}

export default EncoderStimulus
