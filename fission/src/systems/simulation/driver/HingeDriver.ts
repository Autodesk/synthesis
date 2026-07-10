import type Jolt from "@azaleacolburn/jolt-physics"
import type { mirabuf } from "@/proto/mirabuf"
import { getLastDeltaT } from "@/systems/physics/PhysicsSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { type NoraNumber, NoraTypes } from "../Nora"
import Driver, { DriverControlMode, type DriverID } from "./Driver"

const MAX_TORQUE_WITHOUT_GRAV = 100

// Proportional gain (rad/s per rad of error) for continuous-rotation position tracking.
const CONTINUOUS_POSITION_GAIN = 5.0

/**
 * Smallest signed rotation (in radians, within [-π, π]) that takes angle `from` to angle `to`.
 */
function shortestAngleDelta(from: number, to: number): number {
    const twoPi = 2 * Math.PI
    let d = (to - from) % twoPi
    if (d > Math.PI) d -= twoPi
    if (d < -Math.PI) d += twoPi
    return d
}

class HingeDriver extends Driver {
    private _constraint: Jolt.HingeConstraint

    private _controlMode: DriverControlMode = DriverControlMode.VELOCITY
    private _targetAngle: number
    private _maxTorqueWithGrav: number = 0.0
    private _continuous: boolean = false
    public accelerationDirection: number = 0.0
    public maxVelocity: number

    // Used by `SwerveDriveBehaviour`
    public feedforwardVelocity: number = 0

    public get constraint(): Jolt.HingeConstraint {
        return this._constraint
    }

    /** World-space position of this hinge's anchor point (on body 1). */
    public get worldAnchor(): Jolt.RVec3 {
        return this._constraint.GetBody1().GetCenterOfMassTransform().MulVec3(this._constraint.GetLocalSpacePoint1())
    }

    /**
     * World-space hinge axis (on body 1).
     *
     * Uses Multiply3x3 (rotation only) because the axis is a direction, not a point.
     * MulVec3 would add the body's world position and corrupt the direction.
     */
    public get worldAxis(): Jolt.Vec3 {
        return this._constraint
            .GetBody1()
            .GetCenterOfMassTransform()
            .Multiply3x3(this._constraint.GetLocalSpaceHingeAxis1())
    }

    public get targetAngle(): number {
        return this._targetAngle
    }
    public set targetAngle(rads: number) {
        // A continuously-rotating hinge has no meaningful limits to clamp to; the target is a
        // heading the controller will reach via the shortest path. A limited hinge clamps.
        this._targetAngle = this._continuous
            ? rads
            : Math.max(this._constraint.GetLimitsMin(), Math.min(this._constraint.GetLimitsMax(), rads))
    }

    /**
     * Removes this hinge's rotation limit so it can spin continuously (used for swerve azimuth /
     * steering modules). Without this a Jolt hinge angle is confined to [-π, π] and a module can
     * get stuck taking the long way around when the shortest path crosses the ±π seam.
     */
    public setContinuousRotation(): void {
        this._continuous = true
        this._constraint.SetLimits(-Math.PI, Math.PI)
    }

    /** True once {@link setContinuousRotation} has been applied, marking this as a swerve azimuth hinge. */
    public get continuous(): boolean {
        return this._continuous
    }

    public get maxAcceleration() {
        return this._constraint.GetMotorSettings().mMaxTorqueLimit
    }

    public set maxAcceleration(nm: number) {
        const motorSettings = this._constraint.GetMotorSettings()
        motorSettings.set_mMaxTorqueLimit(nm)
        motorSettings.set_mMinTorqueLimit(-nm)
    }

    public get controlMode(): DriverControlMode {
        return this._controlMode
    }

    public set controlMode(mode: DriverControlMode) {
        this._controlMode = mode
        switch (mode) {
            case DriverControlMode.VELOCITY:
                this._constraint.SetMotorState(JOLT.EMotorState_Velocity)
                break
            case DriverControlMode.POSITION:
                // Position tracking runs through the velocity motor via a shortest-path P-controller
                // in update(), which lets a continuous hinge cross the ±π seam.
                this._constraint.SetMotorState(JOLT.EMotorState_Velocity)
                break
            default:
                // idk
                break
        }
    }

    public constructor(id: DriverID, constraint: Jolt.HingeConstraint, maxVelocity: number, info?: mirabuf.IInfo) {
        super(id, info)

        this._constraint = constraint
        this.maxVelocity = maxVelocity
        this._targetAngle = this._constraint.GetCurrentAngle()

        const motorSettings = this._constraint.GetMotorSettings()
        const springSettings = motorSettings.mSpringSettings

        // These values were selected based on the suggestions of the documentation for stiff control.
        springSettings.mFrequency = 20 * (1.0 / getLastDeltaT())
        springSettings.mDamping = 0.995
        motorSettings.mSpringSettings = springSettings

        this._maxTorqueWithGrav = motorSettings.get_mMaxTorqueLimit()
        if (!PreferencesSystem.getUserPreference("SubsystemGravity")) {
            motorSettings.set_mMaxTorqueLimit(MAX_TORQUE_WITHOUT_GRAV)
            motorSettings.set_mMinTorqueLimit(-MAX_TORQUE_WITHOUT_GRAV)
        }

        this.controlMode = DriverControlMode.VELOCITY

        PreferencesSystem.addPreferenceEventListener("SubsystemGravity", event => {
            const motorSettings = this._constraint.GetMotorSettings()
            if (event.prefValue) {
                motorSettings.set_mMaxTorqueLimit(this._maxTorqueWithGrav)
                motorSettings.set_mMinTorqueLimit(-this._maxTorqueWithGrav)
            } else {
                motorSettings.set_mMaxTorqueLimit(MAX_TORQUE_WITHOUT_GRAV)
                motorSettings.set_mMinTorqueLimit(-MAX_TORQUE_WITHOUT_GRAV)
            }
        })
    }

    public update(_: number): void {
        if (this._controlMode == DriverControlMode.VELOCITY) {
            this._constraint.SetTargetAngularVelocity(this.accelerationDirection * this.maxVelocity)
        } else if (this._controlMode == DriverControlMode.POSITION) {
            // Shortest-path velocity P-control: the wrapped error lets a continuous hinge cross the
            // ±π seam the short way, and capping at maxVelocity bounds the reaction torque.
            // feedforwardVelocity cancels steady-state lag when the target angle migrates (e.g.
            // during chassis rotation the robot-frame target moves at the chassis spin rate).
            const error = shortestAngleDelta(this._constraint.GetCurrentAngle(), this._targetAngle)
            const rawVelocity = error * CONTINUOUS_POSITION_GAIN + this.feedforwardVelocity
            const velocity = Math.max(-this.maxVelocity, Math.min(this.maxVelocity, rawVelocity))
            this._constraint.SetTargetAngularVelocity(velocity)
        }
    }

    public getReceiverType(): NoraTypes {
        return NoraTypes.NUMBER
    }

    public setReceiverValue(val: NoraNumber): void {
        this.accelerationDirection = val
    }

    public displayName(): string {
        return `${this.info?.name ?? "-"} [Hinge]`
    }
}

export default HingeDriver
