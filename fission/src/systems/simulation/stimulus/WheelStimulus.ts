import Jolt from "@barclah/jolt-physics";
import EncoderStimulus from "./EncoderStimulus";

/**
 * 
 */
class WheelRotationStimulus extends EncoderStimulus {
    private _accum: boolean = true;
    private _wheelRotationAccum = 0.0;
    private _wheel: Jolt.Wheel;
    
    public get positionValue(): number {
        if (this._accum) {
            return this._wheelRotationAccum;
        } else {
            return this._wheel.GetRotationAngle();
        }
    }

    public get velocityValue(): number {
        return this._wheel.GetAngularVelocity();
    }

    public set accum(shouldAccum: boolean) {
        if (!this._accum && shouldAccum) {
            this.resetAccum();
        }
        this._accum = shouldAccum;
    }

    public constructor(wheel: Jolt.Wheel) {
        super();

        this._wheel = wheel;
    }

    public Update(_: number): void {
        if (this._accum) {
            this._wheelRotationAccum += 0;
        }
    }

    public resetAccum() { this._wheelRotationAccum = 0.0; }
}

export default WheelRotationStimulus;