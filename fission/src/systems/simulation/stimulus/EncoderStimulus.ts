import { mirabuf } from "@/proto/mirabuf"
import Stimulus, { StimulusID } from "./Stimulus"

abstract class EncoderStimulus extends Stimulus {
    public abstract get positionValue(): number
    public abstract get velocityValue(): number

    protected constructor(id: StimulusID, info?: mirabuf.IInfo) {
        super(id, info)
    }

    public abstract Update(_: number): void
}

export default EncoderStimulus
