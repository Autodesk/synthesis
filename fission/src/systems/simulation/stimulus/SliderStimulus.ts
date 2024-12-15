import Jolt from "@barclah/jolt-physics"
import EncoderStimulus from "./EncoderStimulus"
import { mirabuf } from "@/proto/mirabuf"
import { StimulusID } from "./Stimulus"
import { NoraTypes, NoraNumber2 } from "../Nora"

class SliderStimulus extends EncoderStimulus {
    private _slider: Jolt.SliderConstraint
    private _velocity: number = 0.0

    public get positionValue(): number {
        return this._slider.GetCurrentPosition()
    }
    public get velocityValue(): number {
        return this._velocity
    }

    public constructor(id: StimulusID, slider: Jolt.SliderConstraint, info?: mirabuf.IInfo) {
        super(id, info)

        this._slider = slider
    }

    private _lastPosition: number = 0.0
    public Update(deltaT: number): void {
        this._velocity = (this._slider.GetCurrentPosition() - this._lastPosition) / deltaT
        this._lastPosition = this._slider.GetCurrentPosition()
    }

    public getSupplierType(): NoraTypes {
        return NoraTypes.Number2
    }
    public getSupplierValue(): NoraNumber2 {
        return [this.positionValue, this.velocityValue]
    }
    public DisplayName(): string {
        return `${this.info?.name ?? "-"} [Encoder]`
    }
}

export default SliderStimulus
