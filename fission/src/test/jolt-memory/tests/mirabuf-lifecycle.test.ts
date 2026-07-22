// Integration exercise for the mirabuf robot load/unload path: createBodiesFromParser,
// createJointsFromParser (-> isWheel -> createWheelConstraint), and destroyMechanism, driven
// through the real `PhysicsSystem`/`MirabufParser` classes, with the same leak-detection
// instrumentation as real-lifecycle.test.ts. Every robot spawn/despawn goes through
// `createMechanismFromParser`/`destroyMechanism` via MirabufSceneObject.ts, and this path was
// previously untested by this suite, which only exercised the hand-rolled createBox/setShape path.
//
// No real `.mira` asset is fetched (no network, see setup.ts). The assembly is hand-built in
// mirabuf-fixtures.ts, seeded from tracing exactly which proto fields
// MirabufParser.ts/ConstraintSettingsUtilities.ts/PhysicsSystem.ts dereference for a single
// chassis + wheel (REVOLUTE, isWheel) robot.
import { describe, expect, test } from "vitest"
import MirabufParser from "@/mirabuf/MirabufParser"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import JOLT from "@/util/loading/JoltSyncLoader"
import { diffLiveCountsFiltered, patchDestroyGuard, snapshotAllLiveCounts } from "../lib/instrumentation"
import { createWheeledRobotAssembly } from "../lib/mirabuf-fixtures"

describe("real lifecycle: mirabuf robot load/simulate/unload", () => {
    test("createMechanismFromParser -> update -> destroyMechanism leaves no leak and no destroy-guard violation", () => {
        const { assembly } = createWheeledRobotAssembly()
        const parser = new MirabufParser(assembly)

        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        const system = new PhysicsSystem()
        const mechanism = system.createMechanismFromParser(parser)

        expect(mechanism.nodeToBody.size).toBe(2)

        for (let step = 0; step < 3; step++) {
            system.update(1 / 60)
        }

        system.destroyMechanism(mechanism)
        mechanism.layerReserve?.release()
        system.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })

    // Real usage isn't a single load/simulate/unload. A `PhysicsSystem` outlives many robot swaps:
    // load, delete, load a different one, repeat. A leak too small to see in one cycle (e.g. one
    // un-released ref per createMechanismFromParser call) compounds linearly here. Each cycle
    // loads a distinct variant (different GUIDs/dimensions, see mirabuf-fixtures.ts) to rule out
    // an identity-keyed cache masking the bug.
    //
    // Hazard: `VehicleConstraint.SetVehicleCollisionTester` AddRefs `tester` (see
    // PhysicsSystem.ts's `createVehicleListeners`), a claim held by the constraint, not by us.
    // `destroyMechanism`'s `mech.vehicleTesters.forEach(x => x.Release())` released that same
    // reference, but `RemoveConstraint` (run just before, in the same `destroyMechanism`) frees
    // the constraint first, which drops its own reference and frees `tester` immediately. The
    // later `Release()` was therefore a double-free on already-freed memory. Fixed by having
    // `createVehicleListeners` take its own explicit `tester.AddRef()`, so `destroyMechanism`'s
    // `Release()` always drops a reference we actually hold, independent of teardown order.
    test("N robot swaps (createMechanismFromParser -> update -> destroyMechanism) x10 through one PhysicsSystem leaves no leak", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        const system = new PhysicsSystem()

        for (let cycle = 0; cycle < 10; cycle++) {
            const { assembly } = createWheeledRobotAssembly(cycle)
            const parser = new MirabufParser(assembly)
            const mechanism = system.createMechanismFromParser(parser)

            expect(mechanism.nodeToBody.size).toBe(2)

            for (let step = 0; step < 3; step++) {
                system.update(1 / 60)
            }

            system.destroyMechanism(mechanism)
            mechanism.layerReserve?.release()
        }

        system.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })
})
