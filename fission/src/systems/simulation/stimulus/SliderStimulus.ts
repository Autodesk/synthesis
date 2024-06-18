import Jolt from "@barclah/jolt-physics"
import EncoderStimulus from "./EncoderStimulus"

class SliderStimulus extends EncoderStimulus {
    private _slider: Jolt.SliderConstraint
    private _velocity: number = 0.0

    public get positionValue(): number {
        return this._slider.GetCurrentPosition()
    }
    public get velocityValue(): number {
        return this._velocity
    }

    public constructor(slider: Jolt.SliderConstraint) {
        super()

        this._slider = slider
    }

    private _lastPosition: number = 0.0
    public Update(deltaT: number): void {
        this._velocity =
            (this._slider.GetCurrentPosition() - this._lastPosition) / deltaT
        this._lastPosition = this._slider.GetCurrentPosition()
    }
}

export default SliderStimulus
