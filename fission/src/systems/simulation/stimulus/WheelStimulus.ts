import type Jolt from "@azaleacolburn/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import { type NoraNumber2, NoraTypes } from "../Nora"
import EncoderStimulus from "./EncoderStimulus"
import type { StimulusID } from "./Stimulus"

/**
 *
 */
class WheelRotationStimulus extends EncoderStimulus {
    private _accum: boolean = true
    private _wheelRotationAccum = 0.0
    private _wheel: Jolt.Wheel

    public get positionValue(): number {
        if (this._accum) {
            return this._wheelRotationAccum
        } else {
            return this._wheel.GetRotationAngle()
        }
    }

    public get velocityValue(): number {
        return this._wheel.GetAngularVelocity()
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

    public getSupplierType(): NoraTypes {
        return NoraTypes.NUMBER2
    }
    public getSupplierValue(): NoraNumber2 {
        return [this.positionValue, this.velocityValue]
    }
    public displayName(): string {
        return `${this.info?.name ?? "-"} [Encoder]`
    }
}

export default WheelRotationStimulus
