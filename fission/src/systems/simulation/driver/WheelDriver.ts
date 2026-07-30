import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
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
    /** This wheel's unrotated rolling direction, chassis-local. Mecanum swings it onto the rollers. */
    private readonly _restForward: THREE.Vector3
    /** Axis `_restForward` is swung about, chassis-local. Vertical for every current import path. */
    private readonly _steeringAxis: THREE.Vector3

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

        // Captured before anything can rotate it; the URDF import path overrides these defaults.
        const settings = this._wheel.GetSettings()
        this._restForward = WheelDriver.readVec3(settings.get_mWheelForward())
        this._steeringAxis = WheelDriver.readVec3(settings.get_mSteeringAxis())
    }

    /** Copies a Jolt getter's reused static temporary, which must not be destroyed. */
    private static readVec3(v: Jolt.Vec3): THREE.Vector3 {
        return new THREE.Vector3(v.GetX(), v.GetY(), v.GetZ())
    }

    /** Points this wheel's rolling direction along `forward`, expressed in chassis-local space. */
    private setWheelForward(forward: THREE.Vector3): void {
        const vec = new JOLT.Vec3(forward.x, forward.y, forward.z)
        // set_ copies into the settings struct, so the argument stays ours to free.
        this._wheel.GetSettings().set_mWheelForward(vec)
        JOLT.destroy(vec)
    }

    /**
     * Restores a plain gripping tire: full friction on both axes, pointed straight ahead.
     *
     * Undoes {@link WheelDriver.disableGroundFriction} and {@link WheelDriver.configureMecanumRoller}.
     * Mecanum is the only drivetrain that applies either, and nothing else clears them.
     */
    public resetTire(): void {
        this._wheel.set_mCombinedLateralFriction(LATERIAL_FRICTION)
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION)
        this.setWheelForward(this._restForward)
    }

    /**
     * Makes this wheel free-roll by removing its grip on both axes.
     *
     * Mecanum uses it on the wheels it excludes from its four driven corners. Unlike
     * `Constraint.SetEnabled(false)` this leaves the suspension intact, so the wheel doesn't droop.
     */
    public disableGroundFriction(): void {
        this._wheel.set_mCombinedLateralFriction(0)
        this._wheel.set_mCombinedLongitudinalFriction(0)
    }

    /**
     * Approximates a mecanum wheel's rollers with a steered, laterally frictionless tire.
     *
     * Jolt has no roller model: a `WheelWV` pushes only along its own heading, and its lateral
     * friction resists sideways motion. A real mecanum wheel is the reverse — it pushes along the
     * roller axle, 45 degrees off the wheel plane, and free-slides perpendicular to it. Turning
     * the tire onto the roller axle and removing its lateral grip reproduces that force basis, so
     * four corners can sum to a lateral force instead of only forward plus yaw.
     *
     * This rewrites `mWheelForward` rather than calling `SetSteerAngle`. Steer angle reads back
     * correctly from `GetSteerAngle` but never reaches the tire: with a 45 degree steer applied
     * that way, a robot driving straight logs zero lateral slip and a top speed of exactly
     * `omega * radius` instead of the `sqrt(2)` times that a rotated contact basis would give.
     *
     * Nothing here rotates the wheel's rigid body, so the mesh still visually points straight ahead.
     *
     * @param angle Roller axle direction in radians, about this wheel's steering axis.
     */
    public configureMecanumRoller(angle: number): void {
        this._wheel.set_mCombinedLateralFriction(0)
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION)
        this.setWheelForward(this._restForward.clone().applyAxisAngle(this._steeringAxis, angle))
    }

    /**
     * Snapshot of this wheel's tire state for drivetrain diagnostics.
     *
     * `contactLongitudinal` is the world-space direction Jolt will actually push this tire along.
     * It is the ground truth for whether a roller angle reached the physics, as opposed to merely
     * being stored; everything else here only reports what was asked for. Zero-length when the
     * tire is airborne, since Jolt only recomputes the basis on contact.
     *
     * Jolt hands back a reused static temporary for the vector, so it is copied and not destroyed.
     * Every other read is a scalar.
     */
    public debugState() {
        return {
            reversed: this._reversed,
            targetVelocity: this._targetVelocity(),
            angularVelocity: this._wheel.GetAngularVelocity(),
            steerAngle: this._wheel.GetSteerAngle(),
            contactLongitudinal: WheelDriver.readVec3(this._wheel.GetContactLongitudinal()),
            hasContact: this._wheel.HasContact(),
            suspensionLength: this._wheel.GetSuspensionLength(),
            longitudinalSlip: this._wheel.get_mLongitudinalSlip(),
            lateralSlip: this._wheel.get_mLateralSlip(),
            longitudinalLambda: this._wheel.GetLongitudinalLambda(),
            lateralLambda: this._wheel.GetLateralLambda(),
            lateralFriction: this._wheel.get_mCombinedLateralFriction(),
            longitudinalFriction: this._wheel.get_mCombinedLongitudinalFriction(),
        }
    }

    public update(_: number): void {
        const vel = this._targetVelocity()
        this._wheel.SetAngularVelocity(vel)
        this._prevVel = vel
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
