import type Jolt from "@synthesis.adsk/jolt-physics"
import InputSystem from "@/systems/input/InputSystem.ts"
import { DriveBehavior } from "@/systems/simulation/behavior/synthesis/drive/DriveBehavior.ts"
import MecanumDriveDiagnostics from "@/systems/simulation/behavior/synthesis/drive/MecanumDriveDiagnostics.ts"
import type WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import type WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"

/** Angle between a mecanum wheel's plane and its roller axles. */
export const ROLLER_ANGLE = Math.PI / 4

/**
 * Turn command added per unit of normalized yaw-rate error. See {@link MecanumDriveBehavior}.
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
 * balances the disturbance, leaving a standing error. The integral term is what actually drives
 * that to zero, within a few tenths of a second of the stick moving.
 */
const YAW_HOLD_INTEGRAL_GAIN = 45.0
/** Ceiling on the correction, so holding heading can never consume the whole drivetrain. */
const YAW_HOLD_LIMIT = 1.0

/** Same three constants for the cross-track loop. See {@link MecanumDriveBehavior.holdCourse}. */
const COURSE_HOLD_GAIN = 2.0
const COURSE_HOLD_INTEGRAL_GAIN = 10.0
const COURSE_HOLD_LIMIT = 0.5

/**
 * Below these the robot counts as stopped and corrections switch off entirely, m/s and rad/s.
 *
 * Loose enough that the loops let go promptly instead of hunting around zero, tight enough that the
 * robot is genuinely at rest by the time they do.
 */
const AT_REST_SPEED = 0.05
const AT_REST_YAW_RATE = 0.05

/**
 * One driven mecanum wheel: the tire plus the geometry the kinematics needs.
 *
 * Everything here is in the chassis frame with +x forward and +y left, which is what
 * {@link MecanumDriveBehavior} mixes in. A wheel is fully described by where it sits and which way
 * its rollers push; nothing else about the chassis matters.
 */
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
     * direction. {@link WheelDriver.configureMecanumRoller} is what points the tire along it.
     */
    pushX: number
    pushY: number

    /** Roller axle direction in radians about the wheel's steering axis. */
    steerAngle: number

    /** Fastest contact-patch speed this tire can produce, m/s. Wheel radius times its max rad/s. */
    maxSurfaceSpeed: number

    /** Row this wheel sits in, front (0) to rear. Roller handedness alternates by row. */
    row: number
    /** +1 for a left-side wheel, -1 for a right-side wheel. */
    leftSign: number
}

/**
 * Robot-relative mecanum drive.
 *
 * Wheels are driven kinematically through {@link WheelDriver}, exactly like skid-steer and swerve,
 * and no wheel mesh spins. Conventions match the swerve inputs it reuses: +forward is the robot's
 * nose, +strafe is left, +turn is counter-clockwise.
 *
 * ## The mix
 *
 * Rather than the WPILib four-corner formula (`FL = x - y - z`, and so on), this solves the
 * kinematics directly. A mecanum tire can only push along its roller normal `n`, and it free-slides
 * perpendicular to it, so the one thing it constrains is the speed of its own contact patch along
 * `n`. For a chassis moving at `(vx, vy)` and yawing at `w`, the patch under a wheel at `(x, y)`
 * moves at `(vx - w*y, vy + w*x)`, so the speed that tire must produce is
 *
 * ```
 * s = (vx - w*y) * nx + (vy + w*x) * ny
 *   = vx*nx + vy*ny + w*(x*ny - y*nx)
 * ```
 *
 * Every wheel gets exactly the speed the requested chassis motion implies at its own contact patch,
 * which is what makes the commands mutually consistent: no tire is asked to scrub against another.
 * For four wheels at the corners of a rectangle with the usual alternating rollers this reduces to
 * the WPILib formula, but unlike that formula it also holds for six wheels, uneven wheelbases,
 * mixed wheel radii, and wheels that aren't at corners at all — none of which are unusual in an
 * imported CAD robot.
 *
 * That mix only produces the motion it describes if each tire can actually push along its roller
 * axle rather than straight ahead, which {@link WheelDriver.configureMecanumRoller} arranges at
 * configuration time. Without it the diagonal pairs cancel and strafing is impossible.
 *
 * ## Driving where you point it
 *
 * The mix above is exact only while no tire is slipping. Once a tire saturates, the force it
 * actually delivers is its friction times the load it happens to carry, not what it was told, so
 * the wheels stop agreeing on one chassis motion. Mecanum feels this far more than skid-steer:
 * every tire pushes diagonally, and the sideways halves of those pushes only cancel when opposite
 * wheels carry comparable load. In this sim they routinely do not — each wheel is a separate
 * single-wheel `VehicleConstraint` on the same chassis, and the solver visits them in sequence, so
 * how the robot's weight gets divided up is largely an artifact of that ordering. Measured on a
 * six-wheel robot with its mass dead-centre, one front wheel carried two and a half times its
 * mirror, which was worth eighty degrees of unasked-for rotation over a metre and a half of driving.
 *
 * So the mix is corrected against what the chassis is really doing, which is what a real mecanum
 * robot does with a gyro and for the same reason. Two loops, both described where they are
 * implemented: {@link MecanumDriveBehavior.holdHeading} takes out rotation the driver didn't ask
 * for, and {@link MecanumDriveBehavior.holdCourse} takes out the sideways creep that survives it.
 * Speed along the commanded direction stays open-loop, so the robot can still be shoved around.
 */
class MecanumDriveBehavior extends DriveBehavior {
    private readonly _modules: MecanumModule[]
    private readonly _brainIndex: number
    private readonly _diagnostics: MecanumDriveDiagnostics

    /** Chassis speed at which the first tire saturates on a pure translation command, m/s. */
    private readonly _maxTranslationSpeed: number
    /** Yaw rate at which the first tire saturates on a pure turn command, rad/s. */
    private readonly _maxTurnRate: number

    public get wheels(): WheelDriver[] {
        return this._modules.map(m => m.wheel)
    }

    /** Chassis body, read for its yaw rate. Without one the drive is open-loop. */
    private readonly _chassis?: Jolt.Body
    /** Accumulated normalized yaw-rate error driving the integral term. */
    private _yawErrorIntegral = 0
    /** Accumulated normalized cross-track error driving its integral term. */
    private _crossTrackIntegral = 0
    /**
     * Whether the robot is still coming to rest from a command of its own.
     *
     * Distinguishes "the driver let go and we're still rolling" from "we were parked and something
     * hit us". The first is the drivetrain's own business to finish cleanly; the second is not
     * something it should fight.
     */
    private _settling = false

    public constructor(
        modules: MecanumModule[],
        wheelStimuli: WheelRotationStimulus[],
        brainIndex: number,
        assemblyId: string,
        chassis?: Jolt.Body
    ) {
        super(
            modules.map(m => m.wheel),
            wheelStimuli
        )

        this._modules = modules
        this._brainIndex = brainIndex
        this._chassis = chassis

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
        this._diagnostics = new MecanumDriveDiagnostics(modules, assemblyId)
    }

    /** Contact-patch speed along this tire's push direction per rad/s of chassis yaw. */
    private static turnCoefficient(m: MecanumModule): number {
        return m.x * m.pushY - m.y * m.pushX
    }

    private static deadband(x: number, threshold = 0.1): number {
        return Math.abs(x) < threshold ? 0 : x
    }

    /**
     * How the chassis is actually moving, in its own frame.
     *
     * Resolved against the chassis' own axes rather than the world's so everything still reads
     * correctly on a ramp. Jolt hands back reused static temporaries here, so every component is
     * read immediately and nothing is destroyed.
     */
    private chassisMotion(): { yawRate: number; forwardSpeed: number; leftSpeed: number } | undefined {
        if (!this._chassis) return undefined

        const rotation = this._chassis.GetRotation()
        const qx = rotation.GetX()
        const qy = rotation.GetY()
        const qz = rotation.GetZ()
        const qw = rotation.GetW()

        // Columns of the rotation matrix: the chassis' local left (+X), up (+Y) and nose (+Z) axes
        // expressed in world space.
        const leftX = 1 - 2 * (qy * qy + qz * qz)
        const leftY = 2 * (qx * qy + qw * qz)
        const leftZ = 2 * (qx * qz - qw * qy)
        const upX = 2 * (qx * qy - qw * qz)
        const upY = 1 - 2 * (qx * qx + qz * qz)
        const upZ = 2 * (qy * qz + qw * qx)
        const noseX = 2 * (qx * qz + qw * qy)
        const noseY = 2 * (qy * qz - qw * qx)
        const noseZ = 1 - 2 * (qx * qx + qy * qy)

        const omega = this._chassis.GetAngularVelocity()
        const yawRate = omega.GetX() * upX + omega.GetY() * upY + omega.GetZ() * upZ

        const velocity = this._chassis.GetLinearVelocity()
        const vx = velocity.GetX()
        const vy = velocity.GetY()
        const vz = velocity.GetZ()

        return {
            yawRate,
            forwardSpeed: vx * noseX + vy * noseY + vz * noseZ,
            leftSpeed: vx * leftX + vy * leftY + vz * leftZ,
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
     * creeps sideways while pointing the right way. Reversing the six-wheel test robot slid fifteen
     * percent of its travel sideways with its heading already held.
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
     * @returns the target written to each wheel, index-aligned with the modules array.
     */
    private driveSpeeds(forward: number, strafe: number, turn: number): number[] {
        if (forward === 0 && strafe === 0 && turn === 0) {
            this._modules.forEach(m => {
                m.wheel.accelerationDirection = 0
            })
            return this._modules.map(() => 0)
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
        const scaled = targets.map(t => t * scale)
        this._modules.forEach((m, i) => {
            m.wheel.accelerationDirection = scaled[i]
        })

        return scaled
    }

    public update(dt: number): void {
        // Reuses the swerve inputs; mecanum has the same 3-DOF command shape. Deadband here rather
        // than inside driveSpeeds so the threshold is applied to the raw -1..1 stick value.
        const forward = MecanumDriveBehavior.deadband(InputSystem.getInput("swerveForward", this._brainIndex))
        const strafe = MecanumDriveBehavior.deadband(InputSystem.getInput("swerveStrafe", this._brainIndex))
        const turn = MecanumDriveBehavior.deadband(InputSystem.getInput("swerveTurn", this._brainIndex))

        const commanded = forward !== 0 || strafe !== 0 || turn !== 0
        const motion = this.chassisMotion()
        const atRest =
            !motion ||
            (Math.hypot(motion.forwardSpeed, motion.leftSpeed) < AT_REST_SPEED &&
                Math.abs(motion.yawRate) < AT_REST_YAW_RATE)

        // Letting go of the stick doesn't end the maneuver — the wheels are now being told to stop
        // a chassis that still has momentum, and they brake with the same lopsided grip they drove
        // with, only reversed. Left uncorrected that is worth tens of degrees of twist on the way
        // to a standstill, which reads as the robot slewing after the driver has already let go. So
        // the heading loop keeps running until the robot is actually stopped.
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

        const targets = this.driveSpeeds(
            course.forward,
            course.strafe,
            correcting ? this.holdHeading(turn, motion.yawRate, dt) : turn
        )

        this._diagnostics.sample(dt, { forward, strafe, turn }, targets)
    }
}

export default MecanumDriveBehavior
