import { mirabuf } from "@/proto/mirabuf"
import Stimulus from "./Stimulus"

abstract class EncoderStimulus extends Stimulus {
    public abstract get positionValue(): number
    public abstract get velocityValue(): number

    protected constructor(info?: mirabuf.IInfo) {
        super(info)
    }

    public abstract Update(_: number): void
}

export default EncoderStimulus
