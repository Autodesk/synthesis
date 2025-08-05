import type { mirabuf } from "@/proto/mirabuf"
import Stimulus, { type StimulusID } from "./Stimulus"

abstract class EncoderStimulus extends Stimulus {
    public abstract get positionValue(): number
    public abstract get velocityValue(): number

    protected constructor(id: StimulusID, info?: mirabuf.IInfo) {
        super(id, info)
    }

    public abstract update(deltaT: number): void
}

export default EncoderStimulus
