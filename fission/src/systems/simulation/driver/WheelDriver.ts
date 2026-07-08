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
    }

    /**
     * Ground-contact/suspension snapshot for diagnosing why a driven wheel isn't producing motion.
     * Includes the wheel's actual world-space raycast origin (not just local joint numbers) so a
     * never-contacting wheel can be checked against the ground plane/other bodies at runtime, and the
     * contact point/body when contact does register so a bad contact (wrong body, off to the side) isn't
     * mistaken for no contact at all. Also includes the chassis body's live world position: a reconstructed
     * wheel was observed sitting at ~2x its construction-time height every frame, and telling apart "the
     * joint math double-applies a world-space origin" from "the chassis itself got shoved upward out of a
     * ground penetration at spawn" requires seeing where the chassis body actually is at runtime, not just
     * where it was queried (unreliably, as (0,0,0)) at construction time before being added to the world.
     */
    public getDebugContactInfo(): {
        hasContact: boolean
        suspensionLength: number
        angularVelocity: number
        wheelWorldPos: { x: number; y: number; z: number }
        chassisWorldPos: { x: number; y: number; z: number }
        contactPos?: { x: number; y: number; z: number }
        contactBodyId?: number
    } {
        const forwardIn = new JOLT.Vec3(1, 0, 0)
        const upIn = new JOLT.Vec3(0, 1, 0)
        const wheelWorldTranslation = this._constraint.GetWheelWorldTransform(0, forwardIn, upIn).GetTranslation()
        JOLT.destroy(forwardIn)
        JOLT.destroy(upIn)

        const hasContact = this._wheel.HasContact()
        const contactPos = hasContact ? this._wheel.GetContactPosition() : undefined
        const chassisPos = this._constraint.GetVehicleBody().GetPosition()

        return {
            hasContact,
            suspensionLength: this._wheel.GetSuspensionLength(),
            angularVelocity: this._wheel.GetAngularVelocity(),
            wheelWorldPos: {
                x: wheelWorldTranslation.GetX(),
                y: wheelWorldTranslation.GetY(),
                z: wheelWorldTranslation.GetZ(),
            },
            chassisWorldPos: { x: chassisPos.GetX(), y: chassisPos.GetY(), z: chassisPos.GetZ() },
            contactPos: contactPos
                ? { x: contactPos.GetX(), y: contactPos.GetY(), z: contactPos.GetZ() }
                : undefined,
            contactBodyId: hasContact ? this._wheel.GetContactBodyID().GetIndex() : undefined,
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
