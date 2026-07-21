import type Jolt from "@azaleacolburn/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import JOLT from "@/util/loading/JoltSyncLoader"
import { type NoraNumber, NoraTypes } from "../Nora"
import type { SimType } from "../wpilib_brain/WPILibTypes"
import Driver, { type DriverID } from "./Driver"

const LATERIAL_FRICTION = 1.0
const LONGITUDINAL_FRICTION = 1.0

class WheelDriver extends Driver {
    private _constraint: Jolt.VehicleConstraint
    private _wheel: Jolt.WheelWV
    public deviceType?: SimType
    public device?: string
    private _reversed: boolean

    public accelerationDirection: number = 0.0
    private _prevVel: number = 0.0
    public maxVelocity = 30.0
    public maxAcceleration = 1.5
    // Debug-only: throttled ground-contact diagnostic, logged only while actually commanded to move.
    private _debugTickCounter = 0

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
        reversed: boolean = false
    ) {
        super(id, info)

        this._constraint = constraint
        this.maxVelocity = maxVel
        const controller = JOLT.castObject(this._constraint.GetController(), JOLT.WheeledVehicleController)
        this.maxAcceleration = controller.GetEngine().mMaxTorque

        this._reversed = reversed
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

        if (this.accelerationDirection !== 0 && this._debugTickCounter++ % 60 === 0) {
            const hasContact = this._wheel.HasContact()
            const chassisBody = this._constraint.GetVehicleBody()
            const chassisPos = chassisBody.GetPosition()
            const chassisVel = chassisBody.GetLinearVelocity()
            const chassisAngVel = chassisBody.GetAngularVelocity()
            const contact = hasContact ? this._wheel.GetContactPosition() : undefined
            const contactNormal = hasContact ? this._wheel.GetContactNormal() : undefined
            const com = chassisBody.GetCenterOfMassPosition()
            const bounds = chassisBody.GetShape().GetLocalBounds()
            const chassisWorldLowestY = com.GetY() + bounds.mMin.GetY()
            const invMass = chassisBody.GetMotionProperties().GetInverseMass()
            const accForce = chassisBody.GetAccumulatedForce()
            console.log(
                `[WheelDriver] ${this.info?.name ?? "?"}: vel=${vel.toFixed(3)} hasContact=${hasContact} ` +
                    `chassisRealMass=${(invMass === 0 ? Infinity : 1 / invMass).toFixed(4)} ` +
                    `chassisAccForce=(${accForce.GetX().toFixed(4)},${accForce.GetY().toFixed(4)},${accForce.GetZ().toFixed(4)}) ` +
                    `suspensionLength=${this._wheel.GetSuspensionLength().toFixed(6)} ` +
                    `longLambda=${this._wheel.GetLongitudinalLambda().toFixed(4)} latLambda=${this._wheel.GetLateralLambda().toFixed(4)} ` +
                    `suspensionLambda=${this._wheel.GetSuspensionLambda().toFixed(4)} ` +
                    `combinedLongFriction=${this._wheel.get_mCombinedLongitudinalFriction().toFixed(4)} ` +
                    `chassisPos=(${chassisPos.GetX().toFixed(3)},${chassisPos.GetY().toFixed(3)},${chassisPos.GetZ().toFixed(3)}) ` +
                    `chassisWorldLowestY=${chassisWorldLowestY.toFixed(4)} ` +
                    `chassisVel=(${chassisVel.GetX().toFixed(4)},${chassisVel.GetY().toFixed(4)},${chassisVel.GetZ().toFixed(4)}) ` +
                    `chassisAngVel=(${chassisAngVel.GetX().toFixed(4)},${chassisAngVel.GetY().toFixed(4)},${chassisAngVel.GetZ().toFixed(4)}) ` +
                    `contactPos=${contact ? `(${contact.GetX().toFixed(3)},${contact.GetY().toFixed(3)},${contact.GetZ().toFixed(3)})` : "n/a"} ` +
                    `contactNormal=${contactNormal ? `(${contactNormal.GetX().toFixed(3)},${contactNormal.GetY().toFixed(3)},${contactNormal.GetZ().toFixed(3)})` : "n/a"}`
            )
            JOLT.destroy(com)
            JOLT.destroy(bounds)
            JOLT.destroy(accForce)
        }
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
