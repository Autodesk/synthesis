import Jolt from "@barclah/jolt-physics";
import EncoderStimulus from "./EncoderStimulus";

class SliderStimulus extends EncoderStimulus {
    
    private _slider: Jolt.SliderConstraint;

    public get value(): number {
        return this._slider.GetCurrentPosition();
    }
    
    public constructor(slider: Jolt.SliderConstraint) {
        super();

        this._slider = slider;
    }
    
    public Update(_: number): void { }
}

export default SliderStimulus;