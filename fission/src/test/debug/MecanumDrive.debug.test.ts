/**
 * DEBUG-ONLY driving loop for the mecanum drivetrain. Delete with `MecanumHarness.ts` once the
 * drivetrain is dialed in.
 *
 * Opt in explicitly — this measures behavior rather than asserting on it, needs the local asset
 * pack, and takes a few seconds, so it stays out of the normal suite:
 *   VITE_MECANUM_DEBUG=1 npx vitest run --project chromium src/test/debug/MecanumDrive.debug.test.ts
 *
 * Pick a different robot with:
 *   VITE_MECANUM_ROBOT="KitBot (2024).mira" VITE_MECANUM_DEBUG=1 npx vitest run ...
 */
import { describe, expect, test, vi } from "vitest"
import PhysicsSystem from "@/systems/physics/PhysicsSystem.ts"
import type { Command, MoveResult } from "./MecanumHarness.ts"
import {
    buildRig,
    chassisPose,
    describeBalance,
    drive,
    formatMove,
    loadRobot,
    resetPose,
    summarize,
    wheelLabels,
} from "./MecanumHarness.ts"

// MecanumDriveDiagnostics reaches into World for the chassis body. The harness measures the
// chassis itself, so a stub that yields no scene objects is enough (diagnostics then no-op).
vi.mock("@/systems/World", () => ({
    default: {
        get sceneRenderer() {
            return { sceneObjects: new Map() }
        },
        get multiplayerSystem() {
            return null
        },
    },
}))

vi.mock("@/systems/input/InputSystem", async () => {
    const { harnessCommand } = await import("./HarnessCommand.ts")
    return {
        default: {
            getInput: (name: string) => {
                if (name === "swerveForward") return harnessCommand.forward
                if (name === "swerveStrafe") return harnessCommand.strafe
                if (name === "swerveTurn") return harnessCommand.turn
                return 0
            },
        },
    }
})

const env = (import.meta as { env?: Record<string, string> }).env ?? {}
// One Jolt heap per page: a second live JoltInterface runs the WASM allocator out of memory, so
// only one robot per run. See the header for how to pick it.
const ROBOTS = [env.VITE_MECANUM_ROBOT || "Dozer v11.mira"]

describe.runIf(env.VITE_MECANUM_DEBUG)("Mecanum drive debug", () => {
    for (const robot of ROBOTS) {
        test(robot, async () => {
            const parser = await loadRobot(robot)
            const rig = buildRig(new PhysicsSystem(), parser)
            const labels = wheelLabels(rig)

            const geometry = rig.layout.modules
                .map(
                    (m, i) =>
                        `  wheel[${i}] ${labels[i].padEnd(5)} ${(m.wheel.info?.name ?? "-").padEnd(10)} ` +
                        `fwd=${m.x.toFixed(3)} left=${m.y.toFixed(3)} row=${m.row} ` +
                        `steer=${((m.steerAngle * 180) / Math.PI).toFixed(0)}deg ` +
                        `radius=${m.wheel.debugState().radius.toFixed(4)} ` +
                        `maxSurfaceSpeed=${m.maxSurfaceSpeed.toFixed(2)}m/s`
                )
                .join("\n")

            const balance = describeBalance(rig)

            // Sweeping throttle separates a wrong mix (error flat in throttle) from saturated
            // tires (error grows with throttle, because a slipping tire's force follows its
            // load rather than its command).
            const throttles = [0.2, 0.4, 0.6, 0.8, 1.0]
            const axes: [string, (t: number) => Command][] = [
                ["fwd", t => ({ forward: t, strafe: 0, turn: 0 })],
                ["rev", t => ({ forward: -t, strafe: 0, turn: 0 })],
                ["left", t => ({ forward: 0, strafe: t, turn: 0 })],
                ["right", t => ({ forward: 0, strafe: -t, turn: 0 })],
                ["ccw", t => ({ forward: 0, strafe: 0, turn: t })],
                ["cw", t => ({ forward: 0, strafe: 0, turn: -t })],
            ]

            const moves: MoveResult[] = []
            for (const [name, make] of axes) {
                for (const t of throttles) {
                    resetPose(rig)
                    moves.push(drive(rig, make(t), 1.5, `${name}@${t.toFixed(1)}`))
                    // Releasing the stick is its own maneuver: the wheels are commanded to a stop
                    // while the chassis still has momentum, and any uncorrected imbalance in how
                    // they brake shows up as the robot curving or twisting on its way to rest.
                    moves.push(drive(rig, { forward: 0, strafe: 0, turn: 0 }, 1.25, `  ↳coast(${name})`))
                }
            }

            // Field-oriented check: spin the robot most of a quarter turn, then ask for field
            // forward. A field-oriented drive keeps going the way it originally faced, so the travel
            // stays along fieldForward no matter what the nose is doing; a robot-relative one follows
            // the nose and the travel lands on fieldLeft instead.
            resetPose(rig)
            const field = chassisPose(rig.chassis, rig.layout.frame)
            moves.push(drive(rig, { forward: 0, strafe: 0, turn: 0.8 }, 1.0, "spin"))
            moves.push(drive(rig, { forward: 0, strafe: 0, turn: 0 }, 0.75, "  ↳coast(spin)"))

            const spun = chassisPose(rig.chassis, rig.layout.frame)
            moves.push(drive(rig, { forward: 1, strafe: 0, turn: 0 }, 1.5, "fieldFwd"))
            const after = chassisPose(rig.chassis, rig.layout.frame)

            const legTravel = after.position.clone().sub(spun.position)
            const spunHeadingDeg =
                (Math.atan2(field.left.dot(spun.forward), field.forward.dot(spun.forward)) * 180) / Math.PI
            const fieldCheck =
                `fieldOriented: after spinning ${spunHeadingDeg.toFixed(1)}deg off field forward, ` +
                `cmd(f=+1.0) travelled field(fwd=${legTravel.dot(field.forward).toFixed(2)}m ` +
                `left=${legTravel.dot(field.left).toFixed(2)}m) ` +
                `robot(fwd=${legTravel.dot(spun.forward).toFixed(2)}m left=${legTravel.dot(spun.left).toFixed(2)}m)`

            console.log(
                `\n===== ${robot} =====\n` +
                    `wheels=${rig.wheels.length} driven=${rig.layout.modules.length} ` +
                    `lateralAxis=${rig.layout.useLateralZ ? "-Z" : "+X"}\n` +
                    `${geometry}\n${balance}\n\n${fieldCheck}\n\n${summarize(moves)}\n\n` +
                    moves
                        .filter(m => m.label.endsWith("@1.0"))
                        .map(m => formatMove(rig, m, labels))
                        .join("\n")
            )

            expect(rig.wheels.length).toBeGreaterThan(0)
        }, 120_000)
    }
})
