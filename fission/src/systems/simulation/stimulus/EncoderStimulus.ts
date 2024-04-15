import Stimulus from "./Stimulus";

abstract class EncoderStimulus extends Stimulus {

    public abstract get positionValue(): number;
    public abstract get velocityValue(): number;

    protected constructor() {
        super();
    }

    public abstract Update(_: number): void;
}

export default EncoderStimulus;