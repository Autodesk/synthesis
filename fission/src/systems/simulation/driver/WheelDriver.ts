import type Jolt from "@synthesis.adsk/jolt-physics"
import * as THREE from "three"
import type { mirabuf } from "@/proto/mirabuf"
import JOLT from "@/util/loading/JoltSyncLoader"
import { readJoltVec3 } from "@/util/TypeConversions"
import { type NoraNumber, NoraTypes } from "../Nora"
import type { SimType } from "../wpilib_brain/WPILibTypes"
import Driver, { type DriverID } from "./Driver"

const LATERIAL_FRICTION = 1.0
const LONGITUDINAL_FRICTION = 1.0

/**
 * Jolt's stock `WheelSettingsWV` slip-vs-friction curves, as `[slip, friction]` points.
 *
 * Restoring a tire means putting these back, and there is no way to read a `LinearCurve`'s points
 * back out through the bindings, so they are mirrored here. Nothing in Synthesis overrides the
 * curves at constraint-creation time, so a freshly imported wheel always carries exactly these.
 * Longitudinal slip is a ratio; lateral slip is in degrees.
 */
const DEFAULT_LONGITUDINAL_FRICTION_CURVE: readonly [number, number][] = [
    [0.0, 0.0],
    [0.06, 1.2],
    [0.2, 1.0],
]
const DEFAULT_LATERAL_FRICTION_CURVE: readonly [number, number][] = [
    [0.0, 0.0],
    [3.0, 1.2],
    [20.0, 1.0],
]

/**
 * Baseline suspension travel a mecanum tire gets in metres.
 *
 * Small enough to stay well inside a wheel's radius on any plausible robot, so the chassis never
 * visibly squats, and large enough that the load a tire carries varies smoothly with chassis pose
 * rather than snapping between wheels. A chassis whose wheels aren't all the same size gets more
 * than this; see {@link mecanumSuspensionTravel}.
 */
const MECANUM_SUSPENSION_TRAVEL = 0.02
/** Near-critical, so the chassis settles without bouncing between wheels. */
const MECANUM_SUSPENSION_DAMPING = 1.0
/** Matches the world gravity PhysicsSystem installs; used to size the suspension spring. */
const GRAVITY = 9.8 // m/s^2

/**
 * Suspension travel that lets every wheel of a drivetrain reach the ground.
 *
 * Mecanum needs all its wheels to be weight bearing. A wheel in the air contributes no force at all,
 * and the remaining wheels' diagonal pushes then don't cancel.
 *
 * Suspension travel is what a real robot uses to close that gap, so the travel is sized to span it:
 * twice the spread in wheel radius, on top of the baseline, which leaves every wheel somewhere
 * inside its stroke rather than pinned at an end. A drivetrain with uniform wheels just gets the baseline.
 *
 * @param radii Every mecanum wheel's radius on this robot, metres.
 */
export function mecanumSuspensionTravel(radii: number[]): number {
    if (radii.length === 0) return MECANUM_SUSPENSION_TRAVEL
    const spread = Math.max(...radii) - Math.min(...radii)
    return MECANUM_SUSPENSION_TRAVEL + 2 * spread
}

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
    /** Suspension geometry as imported, so {@link WheelDriver.resetTire} can put it back. */
    private readonly _restSuspension: {
        position: THREE.Vector3
        direction: THREE.Vector3
        minLength: number
        maxLength: number
        frequency: number
        damping: number
    }

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
        this._restForward = readJoltVec3(settings.get_mWheelForward())
        this._steeringAxis = readJoltVec3(settings.get_mSteeringAxis())

        const spring = settings.get_mSuspensionSpring()
        this._restSuspension = {
            position: readJoltVec3(settings.get_mPosition()),
            direction: readJoltVec3(settings.get_mSuspensionDirection()),
            minLength: settings.get_mSuspensionMinLength(),
            maxLength: settings.get_mSuspensionMaxLength(),
            frequency: spring.get_mFrequency(),
            damping: spring.get_mDamping(),
        }
    }

    /**
     * Rewrites this tire's slip-vs-friction curves.
     *
     * The curves, not the `mCombined*Friction` scalars, are the only durable way to change a
     * tire's grip. `WheelWV::Update` recomputes both scalars from these curves at the top of every
     * physics step, so a scalar written from here is gone before the solver ever reads it.
     * An empty curve evaluates to 0, i.e. no grip at all.
     *
     * @param longitudinal Points for the rolling axis, or undefined for no grip.
     * @param lateral Points for the sideways axis, or undefined for no grip.
     */
    private setFrictionCurves(
        longitudinal: readonly [number, number][] | undefined,
        lateral: readonly [number, number][] | undefined
    ): void {
        const settings = this._wheel.GetSettings()
        const curve = new JOLT.LinearCurve()

        const write = (points: readonly [number, number][] | undefined, apply: (c: Jolt.LinearCurve) => void) => {
            curve.Clear()
            points?.forEach(([slip, friction]) => curve.AddPoint(slip, friction))
            apply(curve)
        }

        write(longitudinal, c => settings.set_mLongitudinalFriction(c))
        write(lateral, c => settings.set_mLateralFriction(c))

        JOLT.destroy(curve)
    }

    /** Points this wheel's rolling direction along `forward`, expressed in chassis-local space. */
    private setWheelForward(forward: THREE.Vector3): void {
        const vec = new JOLT.Vec3(forward.x, forward.y, forward.z)
        this._wheel.GetSettings().set_mWheelForward(vec)
        JOLT.destroy(vec)
    }

    /**
     * Restores a plain gripping tire: full friction on both axes, pointed straight ahead.
     */
    public resetTire(): void {
        this.setFrictionCurves(DEFAULT_LONGITUDINAL_FRICTION_CURVE, DEFAULT_LATERAL_FRICTION_CURVE)
        this._wheel.set_mCombinedLateralFriction(LATERIAL_FRICTION)
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION)
        this.setWheelForward(this._restForward)
        this.resetSuspension()
    }

    /**
     * Approximates a mecanum wheel's rollers with a steered, laterally frictionless tire.
     *
     * Jolt has no roller model: a `WheelWV` pushes only along its own heading, and its lateral
     * friction resists sideways motion. A real mecanum wheel is the reverse — it pushes along the
     * roller axle, 45 degrees off the wheel plane, and free-slides perpendicular to it. Turning
     * the tire onto the roller axle and removing its lateral grip reproduces that force basis, so
     * the wheels can sum to a lateral force instead of only forward plus yaw.
     *
     * This rewrites `mWheelForward` rather than calling `SetSteerAngle`. Steer angle reads back
     * correctly from `GetSteerAngle` but never reaches the tire: with a 45 degree steer applied
     * that way, a robot driving straight logs zero lateral slip and a top speed of exactly
     * `omega * radius` instead of the `sqrt(2)` times that a rotated contact basis would give.
     *
     * Nothing here rotates the wheel's rigid body, so the mesh still visually points straight ahead.
     *
     * @param angle Roller axle direction in radians, about this wheel's steering axis.
     * @param suspensionTravel Travel to give the suspension, from {@link mecanumSuspensionTravel}.
     */
    public configureMecanumRoller(angle: number, suspensionTravel: number): void {
        this.setFrictionCurves(DEFAULT_LONGITUDINAL_FRICTION_CURVE, undefined)
        this._wheel.set_mCombinedLateralFriction(0)
        this._wheel.set_mCombinedLongitudinalFriction(LONGITUDINAL_FRICTION)
        this.setWheelForward(this._restForward.clone().applyAxisAngle(this._steeringAxis, angle))
        this.setSuspensionTravel(suspensionTravel)
    }

    /**
     * Gives this wheel a springy suspension of `travel` metres instead of the near-rigid strut
     * drivetrains import with.
     *
     * Imported wheels get essentially zero travel (see `SUSPENSION_MIN_FACTOR` in PhysicsSystem) to
     * stop robots visibly levitating. The side effect is that a chassis with four or more wheels
     * rests on a statically indeterminate set of rigid struts: the solver is free to put the load
     * almost anywhere, and it picks a badly twisted distribution that also flickers in and out of
     * contact. Skid-steer shrugs that off because every tire pushes the same direction, but mecanum
     * cannot. Its wheels only cancel each other's sideways push when they carry comparable load,
     * so a twisted load distribution turns "drive forward" into "drive diagonally".
     *
     * Real travel makes the load distribution determinate: each tire's share follows its
     * compression, which follows the chassis pose. The spring runs in frequency mode so Jolt sizes
     * the stiffness from the robot's own mass, and the frequency is picked so that a wheel settles
     * at half travel under gravity (static deflection is `g / (2*pi*f)^2`). The attachment point
     * moves up by that same half-travel, so the robot's resting ride height is unchanged and the
     * anti-levitation tuning is preserved.
     *
     * It may be reasonable to use this more generally in the future, not just for mecanum drive.
     * See SYNTH-301 for more info.
     *
     * @param travel Total suspension travel in metres.
     */
    private setSuspensionTravel(travel: number): void {
        const settings = this._wheel.GetSettings()
        const staticDeflection = travel / 2

        settings.set_mSuspensionMinLength(0)
        settings.set_mSuspensionMaxLength(travel)

        const spring = new JOLT.SpringSettings()
        spring.set_mMode(JOLT.ESpringMode_FrequencyAndDamping)
        spring.set_mFrequency(Math.sqrt(GRAVITY / staticDeflection) / (2 * Math.PI))
        spring.set_mDamping(MECANUM_SUSPENSION_DAMPING)
        settings.set_mSuspensionSpring(spring)
        JOLT.destroy(spring)

        // The wheel now hangs `staticDeflection` lower at rest, so lift its mount by the same
        // amount to leave the contact patch, and the chassis, where they already were.
        this.setSuspensionPosition(
            this._restSuspension.position.clone().addScaledVector(this._restSuspension.direction, -staticDeflection)
        )
    }

    /** Restores the near-rigid suspension this wheel was imported with. */
    private resetSuspension(): void {
        const settings = this._wheel.GetSettings()
        settings.set_mSuspensionMinLength(this._restSuspension.minLength)
        settings.set_mSuspensionMaxLength(this._restSuspension.maxLength)

        const spring = new JOLT.SpringSettings()
        spring.set_mMode(JOLT.ESpringMode_FrequencyAndDamping)
        spring.set_mFrequency(this._restSuspension.frequency)
        spring.set_mDamping(this._restSuspension.damping)
        settings.set_mSuspensionSpring(spring)
        JOLT.destroy(spring)

        this.setSuspensionPosition(this._restSuspension.position)
    }

    /** Moves this wheel's suspension attachment point, chassis-local. */
    private setSuspensionPosition(position: THREE.Vector3): void {
        const vec = new JOLT.Vec3(position.x, position.y, position.z)
        this._wheel.GetSettings().set_mPosition(vec)
        JOLT.destroy(vec)
    }

    public get radius(): number {
        return this._wheel.GetSettings().get_mRadius()
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
