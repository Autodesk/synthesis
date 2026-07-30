import * as THREE from "three"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import type { MecanumModule } from "@/systems/simulation/behavior/synthesis/drive/MecanumDriveBehavior.ts"
import World from "@/systems/World.ts"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions.ts"

/**
 * Diagnostics for {@link MecanumDriveBehavior}.
 *
 * Mecanum is the only Synthesis drivetrain whose commanded motion can silently fail: the mixing
 * formula assumes each wheel produces a diagonal ground force from its rollers, which Jolt has no
 * model for and {@link WheelDriver.configureMecanumRoller} only approximates with a steered,
 * laterally frictionless tire. So the interesting question is never "what did we command" but
 * "what did the chassis actually do with it", which needs the command, the chassis response, and
 * the per-tire contact state side by side.
 *
 * Two things to read first in any capture: the `push` column, the bearing Jolt is actually pushing
 * each tire along, which must be -45deg on FL/RR and +45deg on FR/RL for strafing to be possible
 * at all; and a strafe episode's `travel(left=)`, the only direct evidence a lateral force existed.
 *
 * Two outputs:
 * - Live samples at {@link SAMPLE_PERIOD}: command, robot-frame chassis velocity, per-wheel targets
 *   and tire state. Shows instantaneous behavior while driving.
 * - Episode summaries, flushed when the command shape changes: total displacement resolved in the
 *   robot frame *at episode start*, plus total rotation. This is what quantifies "won't drive
 *   straight" (forward-only episode with nonzero lateral travel or rotation) and "won't translate"
 *   (strafe-only episode with near-zero lateral travel).
 *
 * Toggle at runtime from the browser console: `window.mecanumDebug = false`.
 */

/** Seconds between live samples. */
const SAMPLE_PERIOD = 0.25
/** Episodes shorter than this are input noise, not a maneuver worth summarizing. */
const MIN_EPISODE_TIME = 0.3
/** Matches the behavior's own input deadband so logged commands agree with commanded targets. */
const DEADBAND = 0.1

const DEBUG_MECANUM_DEFAULT = true

interface WheelAccumulator {
    target: number
    omega: number
    longitudinalSlip: number
    lateralSlip: number
    lateralLambda: number
    contactSamples: number
}

interface Episode {
    mode: string
    time: number
    samples: number
    command: { forward: number; strafe: number; turn: number }
    yawRate: number
    /** Integrated yaw, radians. Unlike comparing start and end headings this doesn't wrap. */
    rotation: number
    startPosition: THREE.Vector3
    startForward: THREE.Vector3
    startLeft: THREE.Vector3
    startUp: THREE.Vector3
    wheels: WheelAccumulator[]
}

interface ChassisState {
    position: THREE.Vector3
    forward: THREE.Vector3
    left: THREE.Vector3
    up: THREE.Vector3
    velocity: THREE.Vector3
    angularVelocity: THREE.Vector3
}

const num = (x: number, digits = 2) => (x < 0 ? "" : "+") + x.toFixed(digits)

/** Whether mecanum diagnostics are on. Configuration-time logging shares the same switch. */
export function mecanumDebugEnabled(): boolean {
    return (globalThis as { mecanumDebug?: boolean }).mecanumDebug ?? DEBUG_MECANUM_DEFAULT
}

class MecanumDriveDiagnostics {
    private readonly _modules: MecanumModule[]
    private readonly _labels: string[]
    private readonly _assemblyId: string

    private _sampleTimer = 0
    private _episode?: Episode
    private _loggedConfiguration = false

    public constructor(modules: MecanumModule[], assemblyId: string) {
        this._modules = modules
        this._assemblyId = assemblyId
        this._labels = MecanumDriveDiagnostics.labelModules(modules)
    }

    private static get _enabled(): boolean {
        return mecanumDebugEnabled()
    }

    /**
     * Names each corner by its role, e.g. `FL`, `RR`, `CL` for a wheel on the lateral centerline.
     * Duplicate roles get an index suffix so a 6-wheel robot's rows stay distinguishable.
     */
    private static labelModules(modules: MecanumModule[]): string[] {
        const lastRow = Math.max(...modules.map(m => m.row))
        const raw = modules.map(m => {
            const end = m.row === 0 ? "F" : m.row === lastRow ? "R" : "M"
            return `${end}${m.leftSign > 0 ? "L" : "R"}`
        })
        const counts = new Map<string, number>()
        raw.forEach(label => counts.set(label, (counts.get(label) ?? 0) + 1))
        const used = new Map<string, number>()
        return raw.map(label => {
            if ((counts.get(label) ?? 0) === 1) return label
            const n = (used.get(label) ?? 0) + 1
            used.set(label, n)
            return `${label}${n}`
        })
    }

    /** @returns the chassis root body id, or undefined before the scene object is ready. */
    private resolveRootNodeId() {
        return [...World.sceneRenderer.sceneObjects.values()]
            .filter(o => o instanceof MirabufSceneObject)
            .find(o => (o as MirabufSceneObject).assemblyId === this._assemblyId)
            ?.getRootNodeId()
    }

    /**
     * Reads the chassis pose and velocity.
     *
     * Jolt getters hand back reused static temporaries, so every value is copied into THREE types
     * immediately and none of them is destroyed.
     */
    private chassisState(): ChassisState | undefined {
        const rootNodeId = this.resolveRootNodeId()
        if (!rootNodeId) return undefined
        const body = World.physicsSystem.getBody(rootNodeId)
        if (!body) return undefined

        const rotation = convertJoltQuatToThreeQuaternion(body.GetRotation())
        return {
            position: convertJoltVec3ToThreeVector3(body.GetCenterOfMassPosition(), false),
            // Same robot-local convention the behavior mixes against: +Z nose, +X left, +Y up.
            forward: new THREE.Vector3(0, 0, 1).applyQuaternion(rotation),
            left: new THREE.Vector3(1, 0, 0).applyQuaternion(rotation),
            up: new THREE.Vector3(0, 1, 0).applyQuaternion(rotation),
            velocity: convertJoltVec3ToThreeVector3(body.GetLinearVelocity(), false),
            angularVelocity: convertJoltVec3ToThreeVector3(body.GetAngularVelocity(), false),
        }
    }

    /** Names the command shape, so an episode ends when the driver changes what they're asking for. */
    private static classify(forward: number, strafe: number, turn: number): string {
        const active = [
            Math.abs(forward) >= DEADBAND ? (forward > 0 ? "fwd" : "rev") : undefined,
            Math.abs(strafe) >= DEADBAND ? (strafe > 0 ? "left" : "right") : undefined,
            Math.abs(turn) >= DEADBAND ? (turn > 0 ? "ccw" : "cw") : undefined,
        ].filter(Boolean)
        return active.length === 0 ? "idle" : active.join("+")
    }

    /** Logs the corner roles and tire friction once the wheels have physics state. */
    private logConfiguration(): void {
        if (this._loggedConfiguration) return
        this._loggedConfiguration = true

        console.log(
            `[Mecanum] configured ${this._modules.length} driven wheels\n` +
                this._modules
                    .map((m, i) => {
                        const state = m.wheel.debugState()
                        return (
                            `  ${this._labels[i].padEnd(4)} at(fwd=${num(m.x, 3)} left=${num(m.y, 3)}) ` +
                            `steer=${num((m.steerAngle * 180) / Math.PI, 0)}deg ` +
                            `maxSurfaceSpeed=${m.maxSurfaceSpeed.toFixed(2)}m/s ` +
                            `reversed=${state.reversed} ` +
                            `friction(long=${state.longitudinalFriction.toFixed(2)} ` +
                            `lat=${state.lateralFriction.toFixed(2)})`
                        )
                    })
                    .join("\n")
        )
    }

    /**
     * Records one behavior tick. Call after the targets have been written to the wheels.
     *
     * @param dt Seconds since the previous tick.
     * @param command The deadbanded, normalized chassis command (-1..1 per axis).
     * @param targets Per-module wheel target, index-aligned with the modules array.
     */
    public sample(dt: number, command: { forward: number; strafe: number; turn: number }, targets: number[]): void {
        if (!MecanumDriveDiagnostics._enabled || dt <= 0) return

        const chassis = this.chassisState()
        if (!chassis) return

        this.logConfiguration()

        const mode = MecanumDriveDiagnostics.classify(command.forward, command.strafe, command.turn)
        if (this._episode?.mode !== mode) {
            this.flushEpisode(chassis)
            this._episode =
                mode === "idle"
                    ? undefined
                    : {
                          mode,
                          time: 0,
                          samples: 0,
                          command: { forward: 0, strafe: 0, turn: 0 },
                          yawRate: 0,
                          rotation: 0,
                          startPosition: chassis.position.clone(),
                          startForward: chassis.forward.clone(),
                          startLeft: chassis.left.clone(),
                          startUp: chassis.up.clone(),
                          wheels: this._modules.map(() => ({
                              target: 0,
                              omega: 0,
                              longitudinalSlip: 0,
                              lateralSlip: 0,
                              lateralLambda: 0,
                              contactSamples: 0,
                          })),
                      }
        }

        const states = this._modules.map(m => m.wheel.debugState())

        const episode = this._episode
        if (episode) {
            episode.time += dt
            episode.samples++
            episode.command.forward += command.forward
            episode.command.strafe += command.strafe
            episode.command.turn += command.turn
            const yawRate = chassis.angularVelocity.dot(chassis.up)
            episode.yawRate += yawRate
            episode.rotation += yawRate * dt
            states.forEach((state, i) => {
                const acc = episode.wheels[i]
                acc.target += targets[i] ?? 0
                acc.omega += state.angularVelocity
                acc.longitudinalSlip += state.longitudinalSlip
                acc.lateralSlip += state.lateralSlip
                acc.lateralLambda += state.lateralLambda
                if (state.hasContact) acc.contactSamples++
            })
        }

        this._sampleTimer += dt
        if (this._sampleTimer < SAMPLE_PERIOD || mode === "idle") {
            if (mode === "idle") this._sampleTimer = SAMPLE_PERIOD
            return
        }
        this._sampleTimer = 0

        const forwardVel = chassis.velocity.dot(chassis.forward)
        const leftVel = chassis.velocity.dot(chassis.left)
        const yawRate = (chassis.angularVelocity.dot(chassis.up) * 180) / Math.PI

        /**
         * Bearing of the direction Jolt is actually pushing a tire, relative to the robot's nose,
         * positive toward the robot's left. Must read -45 on FL/RR and +45 on FR/RL; 0 everywhere
         * means the roller angle never reached the physics and strafing cannot work.
         */
        const pushBearing = (state: { contactLongitudinal: THREE.Vector3; hasContact: boolean }) => {
            if (!state.hasContact) return "---- "
            const deg =
                (Math.atan2(
                    state.contactLongitudinal.dot(chassis.left),
                    state.contactLongitudinal.dot(chassis.forward)
                ) *
                    180) /
                Math.PI
            return `${num(deg, 1)}`.padEnd(5)
        }

        console.log(
            `[Mecanum] cmd(f=${num(command.forward)} s=${num(command.strafe)} t=${num(command.turn)}) ` +
                `vel(fwd=${num(forwardVel)} left=${num(leftVel)} |v|=${chassis.velocity.length().toFixed(2)}) ` +
                `yaw=${num(yawRate, 1)}deg/s\n` +
                states
                    .map((state, i) => {
                        const target = targets[i] ?? 0
                        return (
                            `  ${this._labels[i].padEnd(4)} target=${num(target)} push=${pushBearing(state)}deg ` +
                            `cmdOmega=${num(state.targetVelocity, 1)} omega=${num(state.angularVelocity, 1)} ` +
                            `slip(long=${num(state.longitudinalSlip)} lat=${num(state.lateralSlip)}) ` +
                            `lambda(long=${num(state.longitudinalLambda, 1)} lat=${num(state.lateralLambda, 1)}) ` +
                            `contact=${state.hasContact ? "yes" : "NO "} ` +
                            `susp=${state.suspensionLength.toFixed(3)}`
                        )
                    })
                    .join("\n")
        )
    }

    /** Prints what the robot actually did over the episode that just ended. */
    private flushEpisode(chassis: ChassisState): void {
        const episode = this._episode
        this._episode = undefined
        if (!episode || episode.samples === 0 || episode.time < MIN_EPISODE_TIME) return

        const n = episode.samples
        const displacement = chassis.position.clone().sub(episode.startPosition)
        const travelForward = displacement.dot(episode.startForward)
        const travelLeft = displacement.dot(episode.startLeft)
        const travelUp = displacement.dot(episode.startUp)

        // Integrated rather than measured start-to-end, which would wrap past half a turn and
        // report a fast spin as a small drift in the opposite direction.
        const rotation = (episode.rotation * 180) / Math.PI

        const lateralPurity =
            Math.abs(travelForward) + Math.abs(travelLeft) > 1e-4
                ? Math.abs(travelLeft) / (Math.abs(travelForward) + Math.abs(travelLeft))
                : 0

        console.log(
            `[Mecanum] episode "${episode.mode}" ${episode.time.toFixed(2)}s ` +
                `cmd(f=${num(episode.command.forward / n)} s=${num(episode.command.strafe / n)} ` +
                `t=${num(episode.command.turn / n)})\n` +
                `  travel(fwd=${num(travelForward)}m left=${num(travelLeft)}m up=${num(travelUp)}m) ` +
                `lateralShare=${(lateralPurity * 100).toFixed(0)}% ` +
                `rotation=${num(rotation, 1)}deg ` +
                `meanYaw=${num(((episode.yawRate / n) * 180) / Math.PI, 1)}deg/s\n` +
                episode.wheels
                    .map((acc, i) => {
                        return (
                            `  ${this._labels[i].padEnd(4)} meanTarget=${num(acc.target / n)} ` +
                            `meanOmega=${num(acc.omega / n, 1)} ` +
                            `meanSlip(long=${num(acc.longitudinalSlip / n)} lat=${num(acc.lateralSlip / n)}) ` +
                            `meanLatLambda=${num(acc.lateralLambda / n, 1)} ` +
                            `contact=${((acc.contactSamples / n) * 100).toFixed(0)}%`
                        )
                    })
                    .join("\n")
        )
    }
}

export default MecanumDriveDiagnostics
