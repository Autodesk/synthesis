// Integration exercise: SynthesisBrain.ts's configure() drivetrain-detection pass (constructor,
// every robot spawn) and applyUnstickForce() (per unstick button press), previously untested
// against real Jolt ownership.
//
// Caught two real bugs:
//   1. createSkidSteerDriveBehavior()'s first loop (building `constraintPositions` to pick the
//      left/right split axis) called `JOLT.destroy(m)` on `TwoBodyConstraint.
//      GetConstraintToBody1Matrix()`'s return, a STATIC_ALIAS (non-constructor `[Value]` return,
//      a reused scratch buffer, not a heap allocation, per ownership-table.generated.json). A
//      bad-free, not a leak: it frees a non-heap address the very next loop in the same function
//      calls the identical method on. Fixed by dropping the destroy() call.
//   2. applyUnstickForce()'s `unstickForce` (`new JOLT.Vec3(...)`, a COPY) was handed to
//      `Body.AddForce`, a CLONED arg per jolt/JoltJS.idl (Jolt copies the value internally, so
//      the caller keeps ownership), but it was never destroy()'d afterward. Fixed by destroying
//      it right after the AddForce call, same pattern as DragModeSystem.ts's AddForce fix.
//
// Bug 2 is covered end-to-end through a real `SynthesisBrain` construction. Bug 1 is covered
// directly against a real, properly-cast `HingeConstraint` instead: `createSkidSteerDriveBehavior`
// filters `mechanism.constraints` by `instanceof JOLT.TwoBodyConstraint`, but in this suite's
// plain-Node Jolt build, constraints in `mechanism.constraints` are always base-`Constraint`-typed
// (PhysicsSystem.ts's `*ConstraintSettings.Create()` never casts before storing), so `instanceof`
// never holds without an explicit `JOLT.castObject`. That filter is therefore always empty here,
// and a fixture with a wheel driver crashes before reaching either loop (see createArmAssembly's
// comment), likely a Node-vs-browser-WASM-build divergence in embind's downcasting, a separate
// pre-existing issue out of scope for this audit. So this file tests the exact fixed call pattern
// (`GetConstraintToBody1Matrix()` called twice without destroying, matching both loops in the real
// function) against a `Jolt.HingeConstraint` obtained the same way `SimulationLayer`'s constructor
// does (`JOLT.castObject`, driver-lifecycle.test.ts's audited path).
//
// `World.physicsSystem` is a real `PhysicsSystem`, and a real `SimulationLayer`
// (SimulationSystem.ts, see simulation-layer-lifecycle.test.ts) is built from a real `Mechanism`
// and returned by a stubbed `World.simulationSystem.getSimulationLayer`. The `MirabufSceneObject`
// SynthesisBrain configures against is a minimal duck-typed stub covering only the members
// configure()/applyUnstickForce()/update() read (`mechanism`, `robotPreferences`, `assemblyId`,
// `assemblyName`, `savePreferences`, `ejectorActive`/`intakeActive`).
import { describe, expect, test, vi } from "vitest"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import MirabufParser from "@/mirabuf/MirabufParser"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import { SimulationLayer } from "@/systems/simulation/SimulationSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import JOLT from "@/util/loading/JoltSyncLoader"
import {
    diffLiveCounts,
    diffLiveCountsFiltered,
    patchDestroyGuard,
    snapshotAllLiveCounts,
    snapshotLiveCounts,
} from "../lib/instrumentation"
import { createArmAssembly, createFullDrivetrainAssembly } from "../lib/mirabuf-fixtures"

let activePhysicsSystem: PhysicsSystem
let activeSimulationLayer: SimulationLayer

const mockSceneRenderer = {
    mirabufSceneObjects: {
        findWhere: vi.fn(() => undefined),
    },
}

const mockSimulationSystem = {
    getSimulationLayer: vi.fn(() => activeSimulationLayer),
}

vi.mock("@/systems/World", () => ({
    default: {
        get physicsSystem() {
            return activePhysicsSystem
        },
        get sceneRenderer() {
            return mockSceneRenderer
        },
        get simulationSystem() {
            return mockSimulationSystem
        },
    },
}))

describe("real lifecycle: SynthesisBrain through a real SimulationLayer/PhysicsSystem", () => {
    test("construct (wheel-less, skid-steer with 0 wheels) -> applyUnstickForce -> update -> disable leaves no leak", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        activePhysicsSystem = new PhysicsSystem()
        const { assembly } = createArmAssembly()
        const parser = new MirabufParser(assembly)
        const mechanism = activePhysicsSystem.createMechanismFromParser(parser)
        activeSimulationLayer = new SimulationLayer(mechanism)

        const fakeAssembly = {
            mechanism,
            assemblyName: "Synthetic Arm",
            assemblyId: "synthetic-arm-brain-test",
            robotPreferences: { unstickForce: 5, sequentialConfig: undefined },
            ejectorActive: false,
            intakeActive: false,
            savePreferences: vi.fn(),
        } as unknown as MirabufSceneObject

        const brain = new SynthesisBrain(fakeAssembly)

        // Bracket narrowly around applyUnstickForce() with the unfiltered per-class counter.
        // `Vec3` is already globally classified INTERNAL_REF_UNPROVABLE in class-classification.ts
        // for an unrelated reason (ContactListenerJS's transient baseOffset alias, see
        // ejectable-intake-lifecycle.test.ts's comment), so filtering it the way the rest of this
        // test does below would hide a real regression of the exact leak this call exists to
        // catch (`unstickForce`, a `new JOLT.Vec3(...)` handed to `Body.AddForce`, a CLONED arg
        // the caller must still destroy()).
        const vec3Before = snapshotLiveCounts(JOLT, ["Vec3"])
        // Private, but this is the exact per-unstick-press call path (update()'s
        // `applyUnstickForce()` on a rising edge of the "unstick" input), called directly here
        // since driving it through InputSystem's real input plumbing is unrelated to what this
        // test audits.
        ;(brain as unknown as { applyUnstickForce: () => void }).applyUnstickForce()
        const vec3After = snapshotLiveCounts(JOLT, ["Vec3"])
        expect(diffLiveCounts(vec3Before, vec3After)).toEqual([])

        for (let i = 0; i < 3; i++) {
            brain.update(1 / 60)
        }

        brain.disable()

        activePhysicsSystem.destroyMechanism(mechanism)
        mechanism.layerReserve?.release()
        activePhysicsSystem.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })

    test("GetConstraintToBody1Matrix's STATIC_ALIAS return survives being read twice without destroy()", () => {
        activePhysicsSystem = new PhysicsSystem()
        const { assembly } = createFullDrivetrainAssembly()
        const parser = new MirabufParser(assembly)
        const mechanism = activePhysicsSystem.createMechanismFromParser(parser)

        const hingeConstraint = mechanism.constraints.find(
            c => c.primaryConstraint.GetSubType() === JOLT.EConstraintSubType_Hinge
        )!.primaryConstraint
        // Same cast SimulationLayer's constructor performs (driver-lifecycle.test.ts's audited
        // path). This is what makes `instanceof Jolt.TwoBodyConstraint` hold.
        const hinge = JOLT.castObject(hingeConstraint, JOLT.HingeConstraint)
        expect(hinge instanceof JOLT.TwoBodyConstraint).toBe(true)

        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        // Mirrors createSkidSteerDriveBehavior's two loops: read the same STATIC_ALIAS Mat44
        // return twice in a row, never destroy()ing it either time.
        const m1 = hinge.GetConstraintToBody1Matrix()
        const t1 = m1.GetTranslation()
        const x1 = t1.GetX()

        const m2 = hinge.GetConstraintToBody1Matrix()
        const t2 = m2.GetTranslation()
        const x2 = t2.GetX()

        // Same underlying constraint, same call, read twice with nothing else mutating it in
        // between, so this must agree. A prior destroy() of the shared scratch buffer between
        // the two reads would make this either throw or return garbage.
        expect(x2).toBe(x1)

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])

        activePhysicsSystem.destroyMechanism(mechanism)
        mechanism.layerReserve?.release()
        activePhysicsSystem.destroy()
    })
})
