import Jolt from "@barclah/jolt-physics"
import EncoderStimulus from "./EncoderStimulus"
import { mirabuf } from "@/proto/mirabuf"
import { StimulusID } from "./Stimulus"
import { NoraTypes, NoraNumber } from "../Nora"

class HingeStimulus extends EncoderStimulus {
    
    private _accum: boolean = false
    private _hingeAngleAccum: number = 0.0
    private _hinge: Jolt.HingeConstraint

    public get positionValue(): number {
        if (this._accum) {
            return this._hingeAngleAccum
        } else {
            return this._hinge.GetCurrentAngle()
        }
    }

    public get velocityValue(): number {
        return 0.0
    }

    public set accum(shouldAccum: boolean) {
        if (!this._accum && shouldAccum) {
            this.resetAccum()
        }
        this._accum = shouldAccum
    }

    public constructor(id: StimulusID, hinge: Jolt.HingeConstraint, info?: mirabuf.IInfo) {
        super(id, info)

        this._hinge = hinge
    }

    public Update(deltaT: number): void {
        if (this._accum) {
            this._hingeAngleAccum += this._hinge.GetTargetAngularVelocity() * deltaT
        }
    }

    public resetAccum() {
        this._hingeAngleAccum = 0.0
    }

    public getSupplierType(): NoraTypes {
        return NoraTypes.Number
    }
    public getSupplierValue(): NoraNumber {
        throw new Error("Method not implemented.")
    }
    public DisplayName(): string {
        return `${this.info?.name ?? "-"} [Encoder]`
    }
}

export default HingeStimulus
