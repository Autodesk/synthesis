import type Jolt from "@synthesis.adsk/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import { BaseUnit, DerivativeOrder, noraType, num, type NoraValueOf } from "../Nora"
import EncoderStimulus from "./EncoderStimulus"
import type { StimulusID } from "./Stimulus"

export const WHEEL_STIMULUS_TYPE = noraType([
    num(BaseUnit.ANGLE, DerivativeOrder.ZERO, "Angle"),
    num(BaseUnit.ANGLE, DerivativeOrder.ONE, "Velocity"),
])

class WheelRotationStimulus extends EncoderStimulus<typeof WHEEL_STIMULUS_TYPE> {
    private _accum: boolean = true
    private _wheelRotationAccum = 0.0
    private _wheel: Jolt.Wheel

    public get positionValue() {
        let value = this._wheelRotationAccum
        if (!this._accum) {
            value = this._wheel.GetRotationAngle()
        }

        return { value, baseType: num(BaseUnit.ANGLE, DerivativeOrder.ZERO) }
    }

    public get velocityValue() {
        return { value: this._wheel.GetAngularVelocity(), baseType: num(BaseUnit.ANGLE, DerivativeOrder.ONE) }
    }

    public set accum(shouldAccum: boolean) {
        if (!this._accum && shouldAccum) {
            this.resetAccum()
        }
        this._accum = shouldAccum
    }

    public constructor(id: StimulusID, wheel: Jolt.Wheel, info?: mirabuf.IInfo) {
        super(id, info)

        this._wheel = wheel
    }

    public update(deltaT: number): void {
        if (this._accum) {
            this._wheelRotationAccum += this._wheel.GetAngularVelocity() * deltaT
        }
    }

    public resetAccum() {
        this._wheelRotationAccum = 0.0
    }

    public get supplierType() {
        return WHEEL_STIMULUS_TYPE
    }

    public supplyValue(): NoraValueOf<typeof WHEEL_STIMULUS_TYPE> {
        return [this.positionValue, this.velocityValue]
    }

    public displayName(): string {
        return `${this.info?.name ?? "-"} [Encoder]`
    }
}

export default WheelRotationStimulus
