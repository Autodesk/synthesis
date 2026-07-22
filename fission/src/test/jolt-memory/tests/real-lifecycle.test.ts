// Integration exercise: drives fission's actual `PhysicsSystem` app class through
// create/simulate/teardown cycles, using the same leak-detection instrumentation as
// factories.ts's minimal world, plus the generic destroy-refcount guard from instrumentation.ts.
//
// Expected to FAIL: `PhysicsSystem.setShape(..., destroy: true)` is a real bug. It frees the
// new shape's C++ object right after `BodyInterface.SetShape` has already taken its own
// reference to it, a raw delete while another owner still holds a pointer (see
// docs/JOLT_REFCOUNTED_DESTROY_SEMANTICS.md, [[jolt_refcounted_destroy_danger]]).
// `patchDestroyGuard` catches this by duck-typing `GetRefCount`, with no recipe naming
// `PhysicsSystem.setShape` specifically. The corrupted shape's vtable gets called through on
// the next `PhysicsSystem.update()` and traps as a WASM `RuntimeError`, so this test is
// expected to crash on first run, not just fail a clean assertion.
//
// Runs under jsdom with `fetch`, `localStorage`, and `Worker` stubbed in setup.ts.
import * as THREE from "three"
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import { createStandaloneBoxShape } from "../lib/factories"
import { diffLiveCountsFiltered, patchDestroyGuard, snapshotAllLiveCounts } from "../lib/instrumentation"

describe("real lifecycle: PhysicsSystem create/simulate/setShape/teardown", () => {
    test("N cycles through the real app class leave no leak and no destroy-guard violation", () => {
        const guard = patchDestroyGuard(JOLT)
        const before = snapshotAllLiveCounts(JOLT)

        const system = new PhysicsSystem()
        const CYCLES = 5
        const BODIES_PER_CYCLE = 3

        for (let cycle = 0; cycle < CYCLES; cycle++) {
            const bodies = Array.from({ length: BODIES_PER_CYCLE }, (_, i) =>
                system.createBox(new THREE.Vector3(0.5, 0.5, 0.5), 1, new THREE.Vector3(i, 5, 0), undefined)
            )
            bodies.forEach(body => system.addBodyToSystem(body.GetID(), true))

            // Known trigger, exercised every cycle (see BodyInterface.SetShape's handoff recipe
            // in factories.ts for the same hazard in isolation).
            const newShape = createStandaloneBoxShape(JOLT)
            system.setShape(bodies[0].GetID(), newShape, true, JOLT.EActivation_Activate, true)

            for (let step = 0; step < 3; step++) {
                system.update(1 / 60)
            }

            system.destroyBodies(...bodies)
        }

        system.destroy()

        const after = snapshotAllLiveCounts(JOLT)
        guard.unpatch()

        expect(guard.violations).toEqual([])
        expect(diffLiveCountsFiltered(before, after)).toEqual([])
    })
})
