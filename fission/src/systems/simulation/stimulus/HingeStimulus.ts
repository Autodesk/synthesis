import type Jolt from "@synthesis.adsk/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import { BaseUnit, DerivativeOrder, noraType, num, type NoraValueOf } from "../Nora"
import EncoderStimulus from "./EncoderStimulus"
import type { StimulusID } from "./Stimulus"

const HINGE_TYPE = noraType([num(BaseUnit.POSITION, DerivativeOrder.ZERO), num(BaseUnit.POSITION, DerivativeOrder.ONE)])

class HingeStimulus extends EncoderStimulus<typeof HINGE_TYPE> {
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

    public update(deltaT: number): void {
        if (this._accum) {
            this._hingeAngleAccum += this._hinge.GetTargetAngularVelocity() * deltaT
        }
    }

    public resetAccum() {
        this._hingeAngleAccum = 0.0
    }

    public get supplierType() {
        return HINGE_TYPE
    }

    public supplyValue(): NoraValueOf<typeof HINGE_TYPE> {
        return [this.positionValue, this.velocityValue]
    }

    public displayName(): string {
        return `${this.info?.name ?? "-"} [Encoder]`
    }
}

export default HingeStimulus
