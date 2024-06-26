import Jolt from "@barclah/jolt-physics"
import Driver from "./Driver"
import JOLT from "@/util/loading/JoltSyncLoader"

const LATERIAL_FRICTION = 0.6;
const LONGITUDINAL_FRICTION = 0.8;

class WheelDriver extends Driver {
    private _constraint: Jolt.VehicleConstraint
    private _wheel: Jolt.WheelWV

    private _targetWheelSpeed: number = 0.0

    public get targetWheelSpeed(): number {
        return this._targetWheelSpeed
    }
    public set targetWheelSpeed(radsPerSec: number) {
        this._targetWheelSpeed = radsPerSec
    }

    public get constraint(): Jolt.VehicleConstraint {
        return this._constraint
    }

    public constructor(constraint: Jolt.VehicleConstraint) {
        super()

        this._constraint = constraint;
        this._wheel = JOLT.castObject(this._constraint.GetWheel(0), JOLT.WheelWV);
        this._wheel.set_mCombinedLateralFriction(LATERIAL_FRICTION); 
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION);
    }

    public Update(_: number): void {
        this._wheel.SetAngularVelocity(this._targetWheelSpeed)
    }
}

export default WheelDriver
