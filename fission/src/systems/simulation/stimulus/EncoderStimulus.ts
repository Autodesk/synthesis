import Stimulus from "./Stimulus";

abstract class EncoderStimulus extends Stimulus {

    public abstract get value(): number;

    protected constructor() {
        super();
    }

    public abstract Update(_: number): void;
}

export default EncoderStimulus;