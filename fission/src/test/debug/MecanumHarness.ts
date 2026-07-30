/**
 * DEBUG-ONLY harness for iterating on mecanum drive behavior. Not part of the shipped test suite;
 * delete along with `MecanumDrive.debug.test.ts` once the drivetrain is dialed in.
 *
 * Boots a real Jolt world with a real robot from a `.mira`, wires the same
 * {@link resolveMecanumLayout} / {@link MecanumDriveBehavior} the game uses, then runs scripted
 * driving maneuvers and reports what the chassis actually did.
 */
import * as THREE from "three"
import type Jolt from "@synthesis.adsk/jolt-physics"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader.ts"
import MirabufParser from "@/mirabuf/MirabufParser.ts"
import type Mechanism from "@/systems/physics/Mechanism.ts"
import type PhysicsSystem from "@/systems/physics/PhysicsSystem.ts"
import MecanumDriveBehavior from "@/systems/simulation/behavior/synthesis/drive/MecanumDriveBehavior.ts"
import {
    applyMecanumTires,
    logMecanumLayout,
    type MecanumFrame,
    type MecanumLayout,
    resolveMecanumLayout,
} from "@/systems/simulation/behavior/synthesis/drive/MecanumLayout.ts"
import WheelDriver from "@/systems/simulation/driver/WheelDriver.ts"
import { makeDriverID } from "@/systems/simulation/driver/Driver.ts"
import JOLT from "@/util/loading/JoltSyncLoader"
import { convertJoltQuatToThreeQuaternion, convertJoltVec3ToThreeVector3 } from "@/util/TypeConversions.ts"
import { type Command, setHarnessCommand } from "./HarnessCommand.ts"

export type { Command }

/** Physics step used by the harness. Matches the sim's standard 120 Hz period. */
export const STEP = 1 / 120

export interface Rig {
    physicsSystem: PhysicsSystem
    mechanism: Mechanism
    wheels: WheelDriver[]
    layout: MecanumLayout
    behavior: MecanumDriveBehavior
    chassis: Jolt.Body
    /** Settled pose of every body, so each maneuver can start from the same place. */
    restPose: BodyPose[]
}

interface BodyPose {
    body: Jolt.Body
    position: [number, number, number]
    rotation: [number, number, number, number]
}

export interface Pose {
    position: THREE.Vector3
    forward: THREE.Vector3
    left: THREE.Vector3
    up: THREE.Vector3
}

export interface MoveResult {
    label: string
    command: Command
    /** Displacement resolved in the robot frame at the start of the move, metres. */
    travelForward: number
    travelLeft: number
    travelUp: number
    /** Integrated yaw over the move, degrees. */
    rotationDeg: number
    /** Peak absolute roll/pitch seen during the move, degrees. */
    maxTiltDeg: number
    /** Fraction of steps each wheel had ground contact, index-aligned with `Rig.wheels`. */
    contact: number[]
    /** Mean normal (suspension) impulse per wheel — the load each tire carried. */
    meanNormalImpulse: number[]
    /** Mean magnitude of the applied longitudinal impulse per wheel. */
    meanLongImpulse: number[]
    /**
     * Mean *signed* longitudinal impulse per wheel, and the yaw moment it produced.
     *
     * A translation command wants the moments to sum to zero. Whichever wheel's moment fails to be
     * cancelled by its mirror is the one turning the robot.
     */
    meanSignedLongImpulse: number[]
    meanYawMoment: number[]
    /** Peak combined lateral friction Jolt actually used, per wheel. Should be 0 on mecanum tires. */
    maxLateralFriction: number[]
    /** Peak combined longitudinal friction Jolt actually used, per wheel. */
    maxLongitudinalFriction: number[]
    /** Mean absolute longitudinal slip per wheel. Large values mean the tire is just spinning. */
    meanSlip: number[]
    /** Mean chassis speed over the move, m/s. */
    meanSpeed: number
    /**
     * Mean yaw rate over the last quarter second, deg/s.
     *
     * Separates a correction that never converges from one that does but had to work through a
     * transient: total rotation counts the transient, this doesn't.
     */
    settledYawRateDeg: number
    /**
     * Mean sideways speed as a fraction of mean speed, measured against the *commanded* direction.
     *
     * Resolved in the frame at the start of the move, which is the field frame the command is given
     * in. Unlike the heading error of the whole path this is an instantaneous measure: it is purely
     * how much the robot slides sideways relative to where it was being told to go.
     */
    crossTrackFraction: number
}

export async function loadRobot(fileName: string) {
    const assembly = await MirabufCachingService.cacheRemoteAndReturn(`/api/mira/robots/${fileName}`, MiraType.ROBOT)
    if (!assembly) throw new Error(`could not load ${fileName}`)
    return new MirabufParser(assembly)
}

/** Lifts every body in the mechanism so the lowest point of the robot sits `clearance` above y=0. */
function dropOntoGround(physicsSystem: PhysicsSystem, mechanism: Mechanism, clearance: number) {
    let minY = Number.POSITIVE_INFINITY
    for (const bodyId of mechanism.nodeToBody.values()) {
        const body = physicsSystem.getBody(bodyId)
        if (!body) continue
        const bounds = body.GetWorldSpaceBounds()
        minY = Math.min(minY, bounds.mMin.GetY())
    }
    if (!Number.isFinite(minY)) return

    const lift = clearance - minY
    if (Math.abs(lift) < 1e-6) return

    for (const bodyId of mechanism.nodeToBody.values()) {
        const body = physicsSystem.getBody(bodyId)
        if (!body) continue
        const p = body.GetPosition()
        // setBodyPosition frees the vector for us.
        physicsSystem.setBodyPosition(bodyId, new JOLT.RVec3(p.GetX(), p.GetY() + lift, p.GetZ()))
    }
}

/** Builds every WheelDriver on a mechanism, mirroring what SimulationLayer does. */
export function makeWheelDrivers(mechanism: Mechanism): WheelDriver[] {
    return mechanism.constraints
        .filter(c => c.primaryConstraint.GetSubType() === JOLT.EConstraintSubType_Vehicle)
        .map(
            c =>
                new WheelDriver(
                    makeDriverID(c),
                    JOLT.castObject(c.primaryConstraint, JOLT.VehicleConstraint),
                    c.maxVelocity,
                    c.info
                )
        )
}

/** Resolves the chassis' world axes against the same local frame the layout mixed in. */
export function chassisPose(chassis: Jolt.Body, frame: MecanumFrame): Pose {
    const rotation = convertJoltQuatToThreeQuaternion(chassis.GetRotation())
    return {
        position: convertJoltVec3ToThreeVector3(chassis.GetCenterOfMassPosition(), false),
        forward: frame.localNose.clone().applyQuaternion(rotation),
        left: frame.localLeft.clone().applyQuaternion(rotation),
        up: new THREE.Vector3(0, 1, 0).applyQuaternion(rotation),
    }
}

/**
 * Spawns a robot, configures it for mecanum, and settles it on the ground.
 *
 * @param physicsSystem A fresh PhysicsSystem (its constructor supplies the ground plane).
 * @param parser Parsed `.mira`.
 * @param settleSeconds How long to let the robot fall and stop bouncing before returning.
 */
export function buildRig(physicsSystem: PhysicsSystem, parser: MirabufParser, settleSeconds = 2.0): Rig {
    const mechanism = physicsSystem.createMechanismFromParser(parser)
    dropOntoGround(physicsSystem, mechanism, 0.02)

    const wheels = makeWheelDrivers(mechanism)
    const chassisId = mechanism.getBodyByNodeId(mechanism.rootBody)!
    const chassis = physicsSystem.getBody(chassisId)!

    const layout = resolveMecanumLayout(wheels, convertJoltQuatToThreeQuaternion(chassis.GetRotation()))
    applyMecanumTires(layout)
    logMecanumLayout(layout)

    const behavior = new MecanumDriveBehavior(layout.modules, [], 0, "debug-harness", layout.frame, chassis)

    const rig: Rig = { physicsSystem, mechanism, wheels, layout, behavior, chassis, restPose: [] }
    // Settle with everything commanded to zero so the drivers hold the wheels still.
    drive(rig, { forward: 0, strafe: 0, turn: 0 }, settleSeconds, "settle")
    rig.restPose = snapshotPose(rig)
    return rig
}

function snapshotPose(rig: Rig): BodyPose[] {
    const poses: BodyPose[] = []
    for (const bodyId of rig.mechanism.nodeToBody.values()) {
        const body = rig.physicsSystem.getBody(bodyId)
        if (!body) continue
        const p = body.GetPosition()
        const q = body.GetRotation()
        poses.push({
            body,
            position: [p.GetX(), p.GetY(), p.GetZ()],
            rotation: [q.GetX(), q.GetY(), q.GetZ(), q.GetW()],
        })
    }
    return poses
}

/**
 * Teleports the robot back to its settled pose with everything at rest.
 *
 * Each maneuver has to start from the same state or the numbers aren't comparable — and without
 * this the robot walks off the 15 m ground plate partway through a sweep and free-falls.
 */
export function resetPose(rig: Rig): void {
    for (const pose of rig.restPose) {
        // setBodyPositionRotationAndVelocity frees all four arguments for us.
        rig.physicsSystem.setBodyPositionRotationAndVelocity(
            pose.body.GetID(),
            new JOLT.RVec3(...pose.position),
            new JOLT.Quat(...pose.rotation),
            new JOLT.Vec3(0, 0, 0),
            new JOLT.Vec3(0, 0, 0)
        )
    }

    rig.wheels.forEach(w => {
        w.accelerationDirection = 0
        w.update(STEP)
    })
    drive(rig, { forward: 0, strafe: 0, turn: 0 }, 0.5, "settle")
}

/**
 * Runs one maneuver.
 *
 * Steps the behavior, the wheel drivers, and physics in the same order `SimulationLayer` +
 * `World.updateWorld` do, so the harness sees the same ordering the game does.
 */
export function drive(rig: Rig, command: Command, seconds: number, label: string): MoveResult {
    const steps = Math.max(1, Math.round(seconds / STEP))
    const start = chassisPose(rig.chassis, rig.layout.frame)

    const contact = rig.wheels.map(() => 0)
    const normalImpulse = rig.wheels.map(() => 0)
    const longImpulse = rig.wheels.map(() => 0)
    const maxLatFriction = rig.wheels.map(() => 0)
    const maxLongFriction = rig.wheels.map(() => 0)
    const slip = rig.wheels.map(() => 0)
    const signedLongImpulse = rig.wheels.map(() => 0)
    let rotation = 0
    let maxTilt = 0
    let speedSum = 0
    const settleWindow = Math.min(steps, Math.round(0.25 / STEP))
    let settledYaw = 0

    // Unit vector of the commanded direction and its left-hand perpendicular. The command is
    // field-frame, and each maneuver starts from the rest pose facing field forward, so this is also
    // the robot frame at the start of the move; it drifts from the robot's current frame by however
    // much yaw the heading loop failed to hold.
    const commandMagnitude = Math.hypot(command.forward, command.strafe)
    const wantForward = commandMagnitude > 0 ? command.forward / commandMagnitude : 0
    const wantLeft = commandMagnitude > 0 ? command.strafe / commandMagnitude : 0
    let crossTrackSum = 0

    for (let i = 0; i < steps; i++) {
        setHarnessCommand(command)
        rig.behavior.update(STEP)
        rig.wheels.forEach(w => w.update(STEP))
        rig.physicsSystem.update(STEP)

        const pose = chassisPose(rig.chassis, rig.layout.frame)
        const omega = convertJoltVec3ToThreeVector3(rig.chassis.GetAngularVelocity(), false)
        const yawRate = omega.dot(pose.up)
        rotation += yawRate * STEP
        if (i >= steps - settleWindow) settledYaw += yawRate
        maxTilt = Math.max(maxTilt, Math.acos(Math.min(1, Math.max(-1, pose.up.y))))
        const velocity = convertJoltVec3ToThreeVector3(rig.chassis.GetLinearVelocity(), false)
        speedSum += velocity.length()
        crossTrackSum += Math.abs(velocity.dot(start.forward) * -wantLeft + velocity.dot(start.left) * wantForward)

        rig.wheels.forEach((w, k) => {
            const state = w.debugState()
            if (state.hasContact) contact[k]++
            normalImpulse[k] += state.suspensionLambda
            longImpulse[k] += Math.abs(state.longitudinalLambda)
            signedLongImpulse[k] += state.longitudinalLambda
            maxLatFriction[k] = Math.max(maxLatFriction[k], state.lateralFriction)
            maxLongFriction[k] = Math.max(maxLongFriction[k], state.longitudinalFriction)
            slip[k] += Math.abs(state.longitudinalSlip)
        })
    }

    const end = chassisPose(rig.chassis, rig.layout.frame)
    const displacement = end.position.clone().sub(start.position)

    return {
        label,
        command,
        travelForward: displacement.dot(start.forward),
        travelLeft: displacement.dot(start.left),
        travelUp: displacement.dot(start.up),
        rotationDeg: (rotation * 180) / Math.PI,
        maxTiltDeg: (maxTilt * 180) / Math.PI,
        contact: contact.map(c => c / steps),
        meanNormalImpulse: normalImpulse.map(v => v / steps),
        meanLongImpulse: longImpulse.map(v => v / steps),
        maxLateralFriction: maxLatFriction,
        maxLongitudinalFriction: maxLongFriction,
        meanSlip: slip.map(v => v / steps),
        meanSpeed: speedSum / steps,
        settledYawRateDeg: ((settledYaw / settleWindow) * 180) / Math.PI,
        crossTrackFraction: speedSum > 0 ? crossTrackSum / speedSum : 0,
        meanSignedLongImpulse: signedLongImpulse.map(v => v / steps),
        meanYawMoment: signedLongImpulse.map((v, k) => {
            const m = rig.layout.modules[k]
            return (v / steps) * (m.x * m.pushY - m.y * m.pushX)
        }),
    }
}

const f2 = (x: number, d = 2) => (x < 0 ? "" : "+") + x.toFixed(d)

export function formatMove(rig: Rig, result: MoveResult, labels: string[]): string {
    return (
        `${result.label.padEnd(10)} cmd(f=${f2(result.command.forward, 1)} s=${f2(result.command.strafe, 1)} ` +
        `t=${f2(result.command.turn, 1)})  travel(fwd=${f2(result.travelForward)}m left=${f2(result.travelLeft)}m ` +
        `up=${f2(result.travelUp)}m) yaw=${f2(result.rotationDeg, 1)}deg tilt<=${result.maxTiltDeg.toFixed(1)}deg ` +
        `speed=${result.meanSpeed.toFixed(2)}m/s ` +
        `netYawMoment=${f2(
            result.meanYawMoment.reduce((a, b) => a + b, 0),
            3
        )}\n` +
        rig.wheels
            .map(
                (_, i) =>
                    `   ${labels[i].padEnd(5)} contact=${(result.contact[i] * 100).toFixed(0).padStart(3)}% ` +
                    `normal=${result.meanNormalImpulse[i].toFixed(3).padStart(6)} ` +
                    `long=${f2(result.meanSignedLongImpulse[i], 3).padStart(7)} ` +
                    `yawMoment=${f2(result.meanYawMoment[i], 4).padStart(8)} ` +
                    `slip=${result.meanSlip[i].toFixed(1).padStart(7)}`
            )
            .join("\n")
    )
}

/**
 * One line per maneuver: what was asked for versus what happened.
 *
 * `err` is the angle between the commanded travel direction and the actual one, so it is directly
 * "how far off does the robot go"; `yaw` is how much it turned while doing it. A translation
 * maneuver wants both near zero. A turn maneuver has no travel direction, so its `err` is blank and
 * the number to read is `drift`, how far the robot wandered while spinning in place.
 */
export function summarize(moves: MoveResult[]): string {
    const rows = moves
        .filter(m => m.label !== "coast" && m.label !== "probe" && m.label !== "settle")
        .map(m => {
            const wantForward = m.command.forward
            const wantLeft = m.command.strafe
            const translating = Math.hypot(wantForward, wantLeft) > 1e-6
            const distance = Math.hypot(m.travelForward, m.travelLeft)

            let error = "    -"
            if (translating && distance > 1e-3) {
                const wanted = Math.atan2(wantLeft, wantForward)
                const actual = Math.atan2(m.travelLeft, m.travelForward)
                let delta = ((actual - wanted) * 180) / Math.PI
                delta = ((((delta + 180) % 360) + 360) % 360) - 180
                error = `${f2(delta, 1)}`.padStart(5)
            }

            return (
                `  ${m.label.padEnd(13)} travel(f=${f2(m.travelForward).padStart(6)} ` +
                `l=${f2(m.travelLeft).padStart(6)})m dist=${distance.toFixed(2).padStart(5)}m ` +
                `err=${error}deg yaw=${f2(m.rotationDeg, 1).padStart(7)}deg ` +
                `settledYaw=${f2(m.settledYawRateDeg, 1).padStart(7)}deg/s ` +
                `speed=${m.meanSpeed.toFixed(2)}m/s`
            )
        })

    return `summary (err = heading error of actual travel vs commanded):\n${rows.join("\n")}`
}

/**
 * Where the robot's mass sits relative to its wheels, and how the standing load is shared.
 *
 * Mecanum's diagonal force pairs only cancel when opposite corners carry similar load, so a
 * front- or side-biased chassis translates directly into drift. This is the number to read before
 * blaming the mixing formula.
 */
export function describeBalance(rig: Rig): string {
    const settle = drive(rig, { forward: 0, strafe: 0, turn: 0 }, 0.5, "probe")
    const labels = wheelLabels(rig)
    const total = settle.meanNormalImpulse.reduce((a, b) => a + b, 0)

    const pose = chassisPose(rig.chassis, rig.layout.frame)

    // Mass-weighted over every body, not just the chassis: the wheels and any arm are a large share
    // of an imported robot's mass, and it is the whole robot's centre that sets the load split.
    let mass = 0
    const weighted = new THREE.Vector3()
    for (const bodyId of rig.mechanism.nodeToBody.values()) {
        const body = rig.physicsSystem.getBody(bodyId)
        if (!body || !body.IsDynamic()) continue
        const inverseMass = body.GetMotionProperties().GetInverseMass()
        if (inverseMass <= 0) continue
        const m = 1 / inverseMass
        mass += m
        weighted.addScaledVector(convertJoltVec3ToThreeVector3(body.GetCenterOfMassPosition(), false), m)
    }
    const com = mass > 0 ? weighted.divideScalar(mass) : pose.position.clone()

    const wheelCentroid = new THREE.Vector3()
    rig.wheels.forEach(w => {
        const forward = new JOLT.Vec3(1, 0, 0)
        const up = new JOLT.Vec3(0, 1, 0)
        const t = w.constraint.GetWheelWorldTransform(0, forward, up).GetTranslation()
        wheelCentroid.add(new THREE.Vector3(t.GetX(), t.GetY(), t.GetZ()))
        JOLT.destroy(forward)
        JOLT.destroy(up)
    })
    wheelCentroid.divideScalar(rig.wheels.length)
    const comOffset = com.clone().sub(wheelCentroid)

    const rollDeg = (Math.asin(Math.min(1, Math.max(-1, -pose.left.y))) * 180) / Math.PI
    const pitchDeg = (Math.asin(Math.min(1, Math.max(-1, pose.forward.y))) * 180) / Math.PI

    return (
        `balance: mass=${mass.toFixed(1)}kg comOffset(fwd=${f2(comOffset.dot(pose.forward), 3)} ` +
        `left=${f2(comOffset.dot(pose.left), 3)} up=${f2(comOffset.dot(pose.up), 3)}) ` +
        `rest(roll=${f2(rollDeg, 2)}deg pitch=${f2(pitchDeg, 2)}deg)\n` +
        `  staticLoad: ` +
        rig.wheels
            .map(
                (_, i) =>
                    `${labels[i]}=${((settle.meanNormalImpulse[i] / (total || 1)) * 100).toFixed(0)}%` +
                    `/susp${rig.wheels[i].debugState().suspensionLength.toFixed(4)}`
            )
            .join(" ")
    )
}

/** Human-readable role names, index-aligned with `Rig.wheels`. */
export function wheelLabels(rig: Rig): string[] {
    const lastRow = Math.max(...rig.layout.modules.map(m => m.row))
    const counts = new Map<string, number>()
    const raw = rig.layout.modules.map(m => {
        const end = m.row === 0 ? "F" : m.row === lastRow ? "R" : "M"
        return `${end}${m.leftSign > 0 ? "L" : "R"}`
    })
    raw.forEach(l => counts.set(l, (counts.get(l) ?? 0) + 1))
    const used = new Map<string, number>()
    return raw.map(l => {
        if ((counts.get(l) ?? 0) === 1) return l
        const n = (used.get(l) ?? 0) + 1
        used.set(l, n)
        return `${l}${n}`
    })
}
