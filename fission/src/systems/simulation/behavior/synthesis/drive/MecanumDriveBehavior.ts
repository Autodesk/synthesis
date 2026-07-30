import InputSystem from "@/systems/input/InputSystem.ts"
import { DriveBehavior } from "@/systems/simulation/behavior/synthesis/drive/DriveBehavior.ts"
import MecanumDriveDiagnostics from "@/systems/simulation/behavior/synthesis/drive/MecanumDriveDiagnostics.ts"
import type WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import type WheelRotationStimulus from "@/systems/simulation/stimulus/WheelStimulus.ts"

/** Angle between a mecanum wheel's plane and its roller axles. */
const ROLLER_ANGLE = Math.PI / 4

/**
 * Roller axle direction for a corner, in radians about the robot's up axis.
 *
 * Roller handedness alternates along the chassis diagonals, so FL/RR share one angle and FR/RL
 * the other — the same `forwardSign * leftSign` product the strafe term uses. Positive rotation
 * about +Y swings the +Z nose toward +X (left), and an FL wheel's rollers have to push
 * forward-and-*right*, hence the negation. A wheel on the lateral centerline (`forwardSign` 0)
 * belongs to no diagonal and stays a plain forward-pointing tire.
 *
 * {@link WheelDriver.configureMecanumRoller} is what applies this to the tire.
 */
export function mecanumRollerSteerAngle(forwardSign: number, leftSign: number): number {
    return -forwardSign * leftSign * ROLLER_ANGLE
}

/**
 * One driven mecanum corner: a wheel plus the geometry the mixing formula needs.
 *
 * Roller handedness on a real mecanum drivetrain alternates along the chassis diagonals, so
 * the strafe term needs both which side and which end of the chassis a wheel sits on. The
 * turn term only needs the side; the rest of it is the wheel's own continuous offset.
 */
export interface MecanumModule {
    wheel: WheelDriver
    /** +1 for a front corner, -1 for a rear corner, 0 for a wheel on the lateral centerline. */
    forwardSign: number
    /** +1 for a left corner, -1 for a right corner. */
    leftSign: number
    /**
     * |forward offset| + |lateral offset| from the driven-wheel centroid, normalized so the
     * driven set averages 1. Normalizing keeps turn authority independent of robot scale while
     * still letting wheels further from the centroid contribute more to a turn.
     */
    momentArm: number
}

/**
 * Robot-relative mecanum drive.
 *
 * Wheels are driven kinematically through {@link WheelDriver}, exactly like skid-steer and
 * swerve, and no wheel mesh spins. Each driven corner gets the standard mecanum
 * inverse-kinematics mix
 *
 * ```
 * target = forward - forwardSign * leftSign * strafe - leftSign * momentArm * turn
 * ```
 *
 * which is the WPILib `driveCartesianIK` formula (`FL = x - y - z`, `FR = x + y + z`,
 * `RL = x + y - z`, `RR = x - y + z`) generalized to per-wheel geometry. Conventions match
 * the swerve inputs it reuses: +forward is the robot's nose, +strafe is left, +turn is
 * counter-clockwise.
 *
 * That mix only produces the motion it describes if each corner can push along its roller axle
 * rather than straight ahead, which {@link WheelDriver.configureMecanumRoller} arranges at
 * configuration time. Without it the diagonal pairs cancel and strafing is impossible.
 */
class MecanumDriveBehavior extends DriveBehavior {
    private readonly _modules: MecanumModule[]
    private readonly _brainIndex: number
    private readonly _diagnostics: MecanumDriveDiagnostics

    /**
     * Wheel surface speed per unit of chassis speed, per module.
     *
     * A steered tire rolls only along its own heading, so moving the chassis at `v` needs a
     * surface speed of just `v * cos(steerAngle)`. Without this a mecanum robot would top out
     * sqrt(2) faster than the same robot in skid-steer.
     */
    private readonly _speedScales: number[]

    public get wheels(): WheelDriver[] {
        return this._modules.map(m => m.wheel)
    }

    public constructor(
        modules: MecanumModule[],
        wheelStimuli: WheelRotationStimulus[],
        brainIndex: number,
        assemblyId: string
    ) {
        super(
            modules.map(m => m.wheel),
            wheelStimuli
        )

        this._modules = modules
        this._brainIndex = brainIndex
        this._speedScales = modules.map(m => Math.cos(mecanumRollerSteerAngle(m.forwardSign, m.leftSign)))
        this._diagnostics = new MecanumDriveDiagnostics(modules, assemblyId)
    }

    private static deadband(x: number, threshold = 0.1): number {
        return Math.abs(x) < threshold ? 0 : x
    }

    /**
     * Mixes the 3-DOF chassis command onto each driven corner.
     *
     * @param forward Nose-ward chassis command, -1..1, already deadbanded.
     * @param strafe Left-ward chassis command, -1..1, already deadbanded.
     * @param turn Counter-clockwise chassis command, -1..1, already deadbanded.
     * @returns the target written to each corner, index-aligned with the modules array.
     */
    private driveSpeeds(forward: number, strafe: number, turn: number): number[] {
        if (forward === 0 && strafe === 0 && turn === 0) {
            this._modules.forEach(m => {
                m.wheel.accelerationDirection = 0
            })
            return this._modules.map(() => 0)
        }

        // Adjusts how much turning versus translation is favored, matching swerve.
        turn *= 1.5

        let maxSpeed = 0
        const targets = this._modules.map(m => {
            const target = forward - m.forwardSign * m.leftSign * strafe - m.leftSign * m.momentArm * turn
            maxSpeed = Math.max(maxSpeed, Math.abs(target))
            return target
        })

        // Normalize all corners together if any exceeds 1, preserving their ratios. Converting the
        // normalized mix to wheel surface speed comes after, so it can't disturb those ratios.
        const scale = maxSpeed > 1 ? 1 / maxSpeed : 1
        const scaled = targets.map((t, i) => t * scale * this._speedScales[i])
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

        const targets = this.driveSpeeds(forward, strafe, turn)

        this._diagnostics.sample(dt, { forward, strafe, turn }, targets)
    }
}

export default MecanumDriveBehavior
