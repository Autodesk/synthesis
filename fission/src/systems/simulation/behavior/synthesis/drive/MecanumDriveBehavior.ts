import * as THREE from "three"
import type Jolt from "@synthesis.adsk/jolt-physics"
import InputSystem from "@/systems/input/InputSystem.ts"
import { DriveBehavior } from "@/systems/simulation/behavior/synthesis/drive/DriveBehavior.ts"
import type { MecanumFrame } from "@/systems/simulation/behavior/synthesis/drive/MecanumLayout.ts"
import type WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import type WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions.ts"

/** Angle between a mecanum wheel's plane and its roller axles. */
export const ROLLER_ANGLE = Math.PI / 4

/**
 * Turn command added per unit of normalized yaw-rate error. "How hard should we try and hold
 * the desired turn rate/target?"
 *
 * High enough to cancel most of the parasitic yaw a saturated tire set produces immediately, low
 * enough that the correction itself doesn't ring at the rate the wheels can change speed.
 */
const YAW_HOLD_GAIN = 4.0

/**
 * Turn command added per unit-second of accumulated error.
 *
 * The parasitic yaw is close to a constant disturbance for as long as a maneuver lasts, which is
 * exactly what proportional control alone cannot remove: it settles wherever the correction
 * balances the disturbance, leaving a standing error. Use this value to drive that to zero.
 */
const YAW_HOLD_INTEGRAL_GAIN = 45.0

/** Ceiling on the correction, so holding heading can never consume the whole drivetrain. */
const YAW_HOLD_LIMIT = 1.0

const COURSE_HOLD_GAIN = 2.0
const COURSE_HOLD_INTEGRAL_GAIN = 10.0
const COURSE_HOLD_LIMIT = 0.5

const AT_REST_SPEED = 0.05 // m/s
const AT_REST_YAW_RATE = 0.05 // rad/s

/** Field-oriented drive is defined about the world's vertical, not the chassis'. */
const WORLD_UP = new THREE.Vector3(0, 1, 0)

export interface MecanumModule {
    wheel: WheelDriver

    /** Forward offset of the contact patch from the chassis' turn centre, metres. */
    x: number
    /** Leftward offset of the contact patch from the chassis' turn centre, metres. */
    y: number

    /**
     * Unit direction this tire pushes the chassis along, chassis frame.
     *
     * This is the roller-normal, 45 degrees off the wheel plane, *not* the wheel's rolling
     * direction.
     */
    pushX: number
    pushY: number

    /** Roller axle direction in radians about the wheel's steering axis. */
    steerAngle: number

    /** Fastest contact-patch speed this tire can produce, m/s. Wheel radius times its max rad/s. */
    maxSurfaceSpeed: number

    /** Row this wheel sits in, front (0) to rear. Roller handedness alternates by row. */
    row: number

    /** +1 for left-side, -1 for right-side. */
    leftSign: number
}

/**
 * Field-oriented mecanum drive.
 *
 * Rather than use a four-corner formula (`FL = x - y - z`, and so on), we solve the
 * kinematics directly. A mecanum tire can only push along its roller normal `n`, and it free-slides
 * perpendicular to it, so the one thing it constrains is the speed of its own contact patch along
 * `n`. For a chassis moving at `(vx, vy)` and yawing at `w`, the patch under a wheel at `(x, y)`
 * moves at `(vx - w*y, vy + w*x)`, so the speed that tire must produce is
 *
 * Every wheel gets exactly the speed the requested chassis motion implies at its own contact patch,
 * which is what makes the commands mutually consistent: no tire is asked to scrub against another.
 * This is prefered as it will hold for six wheels, uneven wheelbases, mixed wheel radii, and wheels
 * that aren't at corners at all.
 *
 * This only works if each tire can actually slide along its roller axle instead of just pushing
 * straight ahead. {@link WheelDriver.configureMecanumRoller} sets that up. Skip it and the
 * diagonal pushes cancel out, so strafing doesn't happen.
 *
 * It's also only exact while no tire is slipping. A slipping tire delivers friction times
 * whatever load it's carrying, not the speed it was told, so the wheels stop agreeing on one
 * motion. Mecanum feels this worse than skid-steer because every push is diagonal, and the
 * sideways halves only cancel if opposite wheels carry similar load; which they often don't
 * here, since each wheel is its own `VehicleConstraint` and the solver settles them one at a
 * time, so load ends up split by solve order more than physics.
 *
 * So we correct the mix against the chassis's actual measured motion, same as a real mecanum
 * robot does with a gyro. {@link MecanumDriveBehavior.holdHeading} removes rotation the driver
 * didn't ask for; {@link MecanumDriveBehavior.holdCourse} removes the sideways drift left over
 * after that. Forward speed stays open-loop, so the robot can still be pushed around.
 *
 * If Jolt ever solves multiple VehicleConstraints on one body simultaneously instead of sequentially,
 * this imbalance, and the tuning below, should be re-measured; the hold-loop gains are tuned against
 * today's solver behavior.
 */
class MecanumDriveBehavior extends DriveBehavior {
    private readonly _modules: MecanumModule[]
    private readonly _brainIndex: number

    private readonly _maxTranslationSpeed: number // m/s
    private readonly _maxTurnRate: number // rad/s

    public get wheels(): WheelDriver[] {
        return this._modules.map(m => m.wheel)
    }

    /** Chassis body, read for its heading and yaw rate. Without one the drive is open-loop. */
    private readonly _chassis?: Jolt.Body
    /** Robot-local axes the modules were laid out against. */
    private readonly _frame: MecanumFrame

    private _fieldForward: THREE.Vector3

    /** When true, stick input drives relative to the chassis nose instead of a fixed field heading. */
    public robotCentric: boolean

    private _yawErrorIntegral = 0
    private _crossTrackIntegral = 0

    private _settling = false

    public constructor(
        modules: MecanumModule[],
        wheelStimuli: WheelRotationStimulus[],
        brainIndex: number,
        frame: MecanumFrame,
        chassis?: Jolt.Body,
        robotCentric = false
    ) {
        super(
            modules.map(m => m.wheel),
            wheelStimuli
        )

        this._modules = modules
        this._brainIndex = brainIndex
        this._chassis = chassis
        this._frame = frame
        this.robotCentric = robotCentric

        // Zero field-oriented drive to the robot's spawn heading so "forward" starts out as the
        // robot's nose. Falls back to world +Z if there is no chassis body to read.
        this._fieldForward = new THREE.Vector3(0, 0, 1)
        this.resetFieldForward()

        // Full stick should saturate exactly one tire and no more, so both limits are set by
        // whichever wheel runs out of speed first. Translation is the same limit on both axes
        // because every roller sits at the same 45 degrees; only its sign differs.
        const speedLimit = (demandPerUnit: (m: MecanumModule) => number) => {
            let limit = Number.POSITIVE_INFINITY
            for (const m of modules) {
                const demand = Math.abs(demandPerUnit(m))
                if (demand > 1e-6) limit = Math.min(limit, m.maxSurfaceSpeed / demand)
            }
            return Number.isFinite(limit) ? limit : 0
        }

        this._maxTranslationSpeed = speedLimit(m => Math.max(Math.abs(m.pushX), Math.abs(m.pushY)))
        this._maxTurnRate = speedLimit(m => MecanumDriveBehavior.turnCoefficient(m))
    }

    /** Contact-patch speed along this tire's push direction per rad/s of chassis yaw. */
    private static turnCoefficient(m: MecanumModule): number {
        return m.x * m.pushY - m.y * m.pushX
    }

    private static deadband(x: number, threshold = 0.1): number {
        return Math.abs(x) < threshold ? 0 : x
    }

    /**
     * Where the chassis is pointing and how it is actually moving, in its own frame.
     *
     * Velocity is resolved against the chassis' own axes rather than the world's so the correction
     * loops still read correctly on a ramp; the heading the field-oriented rotation needs comes from
     * `nose`. Both come from {@link MecanumFrame}, not a fixed +Z, because which local axis is the
     * nose depends on where the robot was imported from.
     */
    private chassisState():
        | { nose: THREE.Vector3; yawRate: number; forwardSpeed: number; leftSpeed: number }
        | undefined {
        if (!this._chassis) return undefined

        const rotation = convertJoltQuatToThreeQuaternion(this._chassis.GetRotation())
        const nose = this._frame.localNose.clone().applyQuaternion(rotation)
        const left = this._frame.localLeft.clone().applyQuaternion(rotation)
        // The chassis' own up, which yaw rate is measured about: local +Y, up in every robot frame.
        const up = new THREE.Vector3(0, 1, 0).applyQuaternion(rotation)

        const omega = convertJoltVec3ToThreeVector3(this._chassis.GetAngularVelocity(), false)
        const velocity = convertJoltVec3ToThreeVector3(this._chassis.GetLinearVelocity(), false)

        return {
            nose,
            yawRate: omega.dot(up),
            forwardSpeed: velocity.dot(nose),
            leftSpeed: velocity.dot(left),
        }
    }

    /**
     * Re-zeroes field-oriented drive to the direction the chassis is facing right now.
     */
    public resetFieldForward(): void {
        const nose = this.chassisState()?.nose
        if (!nose) return

        const heading = nose.projectOnPlane(WORLD_UP)
        if (heading.lengthSq() < 1e-6) return
        this._fieldForward = heading.normalize()
    }

    /**
     * Rotates a field-frame translation command into the chassis frame.
     *
     * @param forward Command along field forward, -1..1.
     * @param strafe Command along field left, -1..1.
     * @param nose The chassis' nose direction in world space.
     * @returns the same command expressed nose-ward and left-ward, for the mix to consume.
     */
    private toChassisFrame(forward: number, strafe: number, nose: THREE.Vector3): { forward: number; strafe: number } {
        const heading = nose.clone().projectOnPlane(WORLD_UP)
        // Pointing straight up or down: no heading to rotate by, so the command stays robot-relative
        // for the tick rather than snapping to an arbitrary direction.
        if (heading.lengthSq() < 1e-6) return { forward, strafe }

        // Angle from field forward to the nose, counter-clockwise about world up. Field left is
        // up x forward, which is +X for the +Z-nose frame, matching the modules' left-positive y.
        const fieldLeft = new THREE.Vector3().crossVectors(WORLD_UP, this._fieldForward)
        const angle = Math.atan2(fieldLeft.dot(heading), this._fieldForward.dot(heading))

        // Rotate the command by -angle: the chassis frame is the field frame turned by +angle.
        const cos = Math.cos(angle)
        const sin = Math.sin(angle)
        return {
            forward: forward * cos + strafe * sin,
            strafe: -forward * sin + strafe * cos,
        }
    }

    /**
     * Extra turn command needed to make the chassis rotate at the rate that was asked for.
     *
     * @param turn The driver's turn command, -1..1.
     * @param yawRate The chassis' measured yaw rate, rad/s.
     * @param dt Seconds since the previous tick.
     * @returns The turn command to actually mix, clamped to the usable range.
     */
    private holdHeading(turn: number, yawRate: number, dt: number): number {
        if (this._maxTurnRate <= 0) return turn

        const error = (turn * this._maxTurnRate - yawRate) / this._maxTurnRate
        const proportional = error * YAW_HOLD_GAIN
        const candidate = proportional + this._yawErrorIntegral * YAW_HOLD_INTEGRAL_GAIN

        // Stop integrating once the correction is pinned at the limit and the error would only
        // push it further out; otherwise the integral winds up and overshoots when the tires
        // regain grip. Errors pointing back toward the limit still integrate, so it unwinds.
        const saturated = Math.abs(candidate) >= YAW_HOLD_LIMIT && Math.sign(error) === Math.sign(candidate)
        if (!saturated) this._yawErrorIntegral += error * dt

        const correction = Math.max(
            -YAW_HOLD_LIMIT,
            Math.min(YAW_HOLD_LIMIT, proportional + this._yawErrorIntegral * YAW_HOLD_INTEGRAL_GAIN)
        )
        return Math.max(-1, Math.min(1, turn + correction))
    }

    /**
     * Steers the translation command back onto the line the driver asked for.
     *
     * Nulling yaw removes most of the damage an uneven load split does, but not all of it: the
     * sideways halves of the wheels' diagonal pushes still fail to cancel exactly, so the robot
     * creeps sideways while pointing the right way.
     *
     * Only the component *across* the commanded direction is corrected. Speed along the command
     * stays open-loop, so this makes the robot go where it is pointed without turning the
     * drivetrain into a speed servo that would refuse to be pushed around by other robots.
     *
     * @param forward Nose-ward command, -1..1.
     * @param strafe Left-ward command, -1..1.
     * @param motion The chassis' measured motion in its own frame.
     * @param dt Seconds since the previous tick.
     */
    private holdCourse(
        forward: number,
        strafe: number,
        motion: { forwardSpeed: number; leftSpeed: number },
        dt: number
    ): { forward: number; strafe: number } {
        const magnitude = Math.hypot(forward, strafe)
        if (magnitude < 1e-6 || this._maxTranslationSpeed <= 0) {
            this._crossTrackIntegral = 0
            return { forward, strafe }
        }

        // Left-hand perpendicular of the commanded direction, in the chassis frame.
        const acrossForward = -strafe / magnitude
        const acrossLeft = forward / magnitude

        const drift = motion.forwardSpeed * acrossForward + motion.leftSpeed * acrossLeft
        const error = -drift / this._maxTranslationSpeed
        const proportional = error * COURSE_HOLD_GAIN
        const candidate = proportional + this._crossTrackIntegral * COURSE_HOLD_INTEGRAL_GAIN

        const saturated = Math.abs(candidate) >= COURSE_HOLD_LIMIT && Math.sign(error) === Math.sign(candidate)
        if (!saturated) this._crossTrackIntegral += error * dt

        const correction = Math.max(
            -COURSE_HOLD_LIMIT,
            Math.min(COURSE_HOLD_LIMIT, proportional + this._crossTrackIntegral * COURSE_HOLD_INTEGRAL_GAIN)
        )

        return {
            forward: forward + correction * acrossForward,
            strafe: strafe + correction * acrossLeft,
        }
    }

    /**
     * Mixes the 3-DOF chassis command onto each wheel.
     *
     * @param forward Nose-ward chassis command, -1..1, already deadbanded.
     * @param strafe Left-ward chassis command, -1..1, already deadbanded.
     * @param turn Counter-clockwise chassis command, -1..1, already deadbanded.
     */
    private driveSpeeds(forward: number, strafe: number, turn: number): void {
        if (forward === 0 && strafe === 0 && turn === 0) {
            this._modules.forEach(m => {
                m.wheel.accelerationDirection = 0
            })
            return
        }

        const vx = forward * this._maxTranslationSpeed
        const vy = strafe * this._maxTranslationSpeed
        const w = turn * this._maxTurnRate

        // Each target is the fraction of its own tire's top speed, so wheels of different sizes or
        // gearings stay consistent with each other rather than with a shared nominal speed.
        let peak = 0
        const targets = this._modules.map(m => {
            const surfaceSpeed = vx * m.pushX + vy * m.pushY + w * MecanumDriveBehavior.turnCoefficient(m)
            const target = m.maxSurfaceSpeed > 0 ? surfaceSpeed / m.maxSurfaceSpeed : 0
            peak = Math.max(peak, Math.abs(target))
            return target
        })

        // A combined command can outrun a tire that either axis alone would not. Scaling the whole
        // set keeps the ratios, and the ratios are what make the wheels agree on one chassis motion.
        const scale = peak > 1 ? 1 / peak : 1
        this._modules.forEach((m, i) => {
            m.wheel.accelerationDirection = targets[i] * scale
        })
    }

    public update(dt: number): void {
        // Deadband here rather than inside driveSpeeds so the threshold is applied to the raw -1..1
        // stick value, before the field-oriented rotation mixes the two translation axes together.
        const fieldForward = MecanumDriveBehavior.deadband(InputSystem.getInput("swerveForward", this._brainIndex))
        const fieldStrafe = MecanumDriveBehavior.deadband(InputSystem.getInput("swerveStrafe", this._brainIndex))
        const turn = MecanumDriveBehavior.deadband(InputSystem.getInput("swerveTurn", this._brainIndex))

        if (InputSystem.getInput("swerveResetFieldForward", this._brainIndex)) this.resetFieldForward()

        const commanded = fieldForward !== 0 || fieldStrafe !== 0 || turn !== 0
        const motion = this.chassisState()

        // Rotation preserves the command's magnitude, so every check and limit below is unaffected
        // by which way the robot happens to be facing. Robot-centric drive skips the rotation
        // entirely: the stick's forward/strafe axes are already the chassis' nose/left axes.
        const { forward, strafe } =
            motion && !this.robotCentric
                ? this.toChassisFrame(fieldForward, fieldStrafe, motion.nose)
                : { forward: fieldForward, strafe: fieldStrafe }

        const atRest =
            !motion ||
            (Math.hypot(motion.forwardSpeed, motion.leftSpeed) < AT_REST_SPEED &&
                Math.abs(motion.yawRate) < AT_REST_YAW_RATE)

        if (commanded) this._settling = true
        else if (atRest) this._settling = false

        const correcting = motion && (commanded || this._settling)
        if (!correcting) {
            this._yawErrorIntegral = 0
            this._crossTrackIntegral = 0
        }

        // While settling there is no commanded direction to hold a course against, so only the
        // rotation is corrected; the wheels are still commanded to a stop.
        const course = correcting && commanded ? this.holdCourse(forward, strafe, motion, dt) : { forward, strafe }
        if (!commanded) this._crossTrackIntegral = 0

        this.driveSpeeds(course.forward, course.strafe, correcting ? this.holdHeading(turn, motion.yawRate, dt) : turn)
    }
}

export default MecanumDriveBehavior
