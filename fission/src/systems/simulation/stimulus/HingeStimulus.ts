import Jolt from "@barclah/jolt-physics";
import EncoderStimulus from "./EncoderStimulus";

class HingeStimulus extends EncoderStimulus {
    private _accum: boolean = false;
    private _hingeAngleAccum = 0.0;
    private _hinge: Jolt.HingeConstraint;

    public get value(): number {
        if (this._accum) {
            return this._hingeAngleAccum;
        } else {
            return this._hinge.GetCurrentAngle();
        }
    }

    public set accum(shouldAccum: boolean) {
        if (!this._accum && shouldAccum) {
            this.resetAccum();
        }
        this._accum = shouldAccum;
    }

    public constructor(hinge: Jolt.HingeConstraint) {
        super();

        this._hinge = hinge;
    }    
    
    public Update(deltaT: number): void {
        if (this._accum) {
            this._hingeAngleAccum += this._hinge.GetTargetAngularVelocity() * deltaT;
        }
    }

    public resetAccum() { this._hingeAngleAccum = 0.0; }
}

export default HingeStimulus;