import Jolt from "@barclah/jolt-physics"
import EncoderStimulus from "./EncoderStimulus"
import { mirabuf } from "@/proto/mirabuf"
import { StimulusID } from "./Stimulus"
import { NoraTypes, NoraNumber2 } from "../Nora"

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

    public Update(deltaT: number): void {
        if (this._accum) {
            this._wheelRotationAccum += this._wheel.GetAngularVelocity() * deltaT
        }
    }

    public resetAccum() {
        this._wheelRotationAccum = 0.0
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

export default WheelRotationStimulus
