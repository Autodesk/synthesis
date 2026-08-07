import type Jolt from "@synthesis.adsk/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import EncoderStimulus from "./EncoderStimulus"
import type { StimulusID } from "./Stimulus"
import { BaseUnit, DerivativeOrder, noraType, type NoraValueOf, num, } from "../Nora"

const SLIDER_TYPE = noraType([
    num(BaseUnit.POSITION, DerivativeOrder.ZERO),
    num(BaseUnit.POSITION, DerivativeOrder.ONE),
])

class SliderStimulus extends EncoderStimulus<typeof SLIDER_TYPE> {
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
    public update(deltaT: number): void {
        this._velocity = (this._slider.GetCurrentPosition() - this._lastPosition) / deltaT
        this._lastPosition = this._slider.GetCurrentPosition()
    }

    public get supplierType() {
        return SLIDER_TYPE
    }

    public supplyValue(): NoraValueOf<typeof SLIDER_TYPE> {
        return [this.positionValue, this.velocityValue]
    }

    public displayName(): string {
        return `${this.info?.name ?? "-"} [Encoder]`
    }
}

export default SliderStimulus
