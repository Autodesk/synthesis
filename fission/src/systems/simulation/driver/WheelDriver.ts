import type Jolt from "@azaleacolburn/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import JOLT from "@/util/loading/JoltSyncLoader"
import { type NoraNumber, NoraTypes } from "../Nora"
import type { SimType } from "../wpilib_brain/WPILibTypes"
import Driver, { type DriverID } from "./Driver"

const LATERIAL_FRICTION = 1.0
const LONGITUDINAL_FRICTION = 1.0
const WHEEL_DIAGNOSTIC_TICKS = 240
const WHEEL_DIAGNOSTIC_INTERVAL = 15

type WheelDiagnosticSource = "urdf-auto" | "regular"

const fmtVec = (v: Jolt.Vec3 | Jolt.RVec3) => `(${v.GetX().toFixed(4)}, ${v.GetY().toFixed(4)}, ${v.GetZ().toFixed(4)})`

class WheelDriver extends Driver {
    private _constraint: Jolt.VehicleConstraint
    private _wheel: Jolt.WheelWV
    public deviceType?: SimType
    public device?: string
    private _reversed: boolean
    private _diagnosticSource: WheelDiagnosticSource
    private _diagnosticTick = 0

    public accelerationDirection: number = 0.0
    private _prevVel: number = 0.0
    public maxVelocity = 30.0
    public maxAcceleration = 1.5

    public _targetVelocity = () => {
        let vel = this.accelerationDirection * (this._reversed ? -1 : 1) * this.maxVelocity

        if (vel - this._prevVel < -this.maxAcceleration) vel = this._prevVel - this.maxAcceleration
        if (vel - this._prevVel > this.maxAcceleration) vel = this._prevVel + this.maxAcceleration

        return vel
    }

    public get constraint(): Jolt.VehicleConstraint {
        return this._constraint
    }

    public constructor(
        id: DriverID,
        constraint: Jolt.VehicleConstraint,
        maxVel: number,
        info?: mirabuf.IInfo,
        deviceType?: SimType,
        device?: string,
        reversed: boolean = false,
        diagnosticSource: WheelDiagnosticSource = "regular"
    ) {
        super(id, info)

        this._constraint = constraint
        this.maxVelocity = maxVel
        const controller = JOLT.castObject(this._constraint.GetController(), JOLT.WheeledVehicleController)
        this.maxAcceleration = controller.GetEngine().mMaxTorque

        this._reversed = reversed
        this._diagnosticSource = diagnosticSource
        this.deviceType = deviceType
        this.device = device
        this._wheel = JOLT.castObject(this._constraint.GetWheel(0), JOLT.WheelWV)
        this._wheel.set_mCombinedLateralFriction(LATERIAL_FRICTION)
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION)
    }

    public update(_: number): void {
        const vel = this._targetVelocity()
        this._wheel.SetAngularVelocity(vel)
        this._prevVel = vel
        this.logWheelDiagnostics(vel)
    }

    private logWheelDiagnostics(targetVelocity: number): void {
        this._diagnosticTick++
        if (this._diagnosticTick > WHEEL_DIAGNOSTIC_TICKS || this._diagnosticTick % WHEEL_DIAGNOSTIC_INTERVAL !== 0) {
            return
        }

        const settings = this._wheel.GetSettings()
        const wheelUp = settings.mWheelUp
        const wheelForward = settings.mWheelForward
        const wheelRightRaw = wheelForward.Cross(wheelUp)
        const wheelRight = wheelRightRaw.Normalized()
        const worldTransform = this._constraint.GetWheelWorldTransform(0, wheelRight, wheelUp)
        const wheelCenter = worldTransform.GetTranslation()
        const worldUp = this._constraint.GetWorldUp()
        const radius = settings.mRadius
        const predictedBottom = {
            x: wheelCenter.GetX() - worldUp.GetX() * radius,
            y: wheelCenter.GetY() - worldUp.GetY() * radius,
            z: wheelCenter.GetZ() - worldUp.GetZ() * radius,
        }
        const hasContact = this._wheel.HasContact()
        const contactPosition = hasContact ? this._wheel.GetContactPosition() : undefined
        const contactNormal = hasContact ? this._wheel.GetContactNormal() : undefined
        const contactLongitudinal = hasContact ? this._wheel.GetContactLongitudinal() : undefined
        const contactLateral = hasContact ? this._wheel.GetContactLateral() : undefined
        const contactDeltaY = contactPosition ? contactPosition.GetY() - predictedBottom.y : undefined

        console.log(
            `[WheelRuntimeCompare] source=${this._diagnosticSource}` +
                ` tick=${this._diagnosticTick}` +
                ` joint="${this.info?.name ?? this.info?.GUID ?? "unknown"}"` +
                ` command=${this.accelerationDirection.toFixed(3)} targetVel=${targetVelocity.toFixed(3)}` +
                ` mPosition=${fmtVec(settings.mPosition)} radius=${radius.toFixed(4)} width=${settings.mWidth.toFixed(4)}` +
                ` wheelCenter=${fmtVec(wheelCenter)}` +
                ` predictedBottom=(${predictedBottom.x.toFixed(4)}, ${predictedBottom.y.toFixed(4)}, ${predictedBottom.z.toFixed(4)})` +
                ` worldUp=${fmtVec(worldUp)} wheelUp=${fmtVec(wheelUp)} wheelForward=${fmtVec(wheelForward)}` +
                ` suspensionDir=${fmtVec(settings.mSuspensionDirection)}` +
                ` suspensionLength=${this._wheel.GetSuspensionLength().toFixed(4)}` +
                ` minSuspension=${settings.mSuspensionMinLength.toFixed(4)}` +
                ` maxSuspension=${settings.mSuspensionMaxLength.toFixed(4)}` +
                ` hitHardPoint=${this._wheel.HasHitHardPoint()}` +
                ` hasContact=${hasContact}` +
                (contactPosition
                    ? ` contactPos=${fmtVec(contactPosition)}` +
                      ` contactDeltaYFromPredictedBottom=${contactDeltaY!.toFixed(4)}` +
                      ` contactNormal=${fmtVec(contactNormal!)}` +
                      ` contactLongitudinal=${fmtVec(contactLongitudinal!)}` +
                      ` contactLateral=${fmtVec(contactLateral!)}`
                    : " contactPos=(none)")
        )

        JOLT.destroy(wheelRightRaw)
        JOLT.destroy(wheelRight)
        JOLT.destroy(worldTransform)
    }

    public getReceiverType(): NoraTypes {
        return NoraTypes.NUMBER
    }
    public setReceiverValue(val: NoraNumber): void {
        this.accelerationDirection = val
    }
    public displayName(): string {
        return `${this.info?.name ?? "-"} [Wheel]`
    }
}

export default WheelDriver
