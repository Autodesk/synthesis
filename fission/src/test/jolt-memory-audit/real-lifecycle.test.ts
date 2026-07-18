// Integration exercise (Phase 6, take 2): drives fission's actual `PhysicsSystem` app class
// through several create/simulate/teardown cycles — not the minimal hand-rolled world in
// factories.ts — with the same leak-detection instrumentation attached, plus the generic
// destroy-refcount guard from instrumentation.ts. This is a real correctness assertion (no
// use-after-free, no leak across the full IDL surface), not a meta-test — it currently FAILS,
// because `PhysicsSystem.setShape(..., destroy: true)` is a genuine production bug: it frees the
// new shape's C++ object immediately after `BodyInterface.SetShape` has already taken its own
// reference to it, a raw delete while another owner still holds a pointer (see
// docs/JOLT_REFCOUNTED_DESTROY_SEMANTICS.md, [[jolt_refcounted_destroy_danger]]). The point is
// that `patchDestroyGuard` catches this generically, by duck-typing `GetRefCount`, with no
// hand-written recipe naming `PhysicsSystem.setShape` specifically. Confirmed empirically
// (non-ASan build): the corrupted shape's vtable gets called through on the very next
// `PhysicsSystem.update()` and traps as a WASM `RuntimeError` — this test is expected to fail with
// that crash, not just a clean assertion, the first time it runs.
//
// This only works now that the jolt-memory-audit project runs under jsdom with `fetch`,
// `localStorage`, and `Worker` stubbed in setup.ts — see that file for why each stub exists.
import * as THREE from "three"
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import { createStandaloneBoxShape } from "./factories"
import { diffLiveCountsFiltered, patchDestroyGuard, snapshotAllLiveCounts } from "./instrumentation"

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

            // The known trigger, exercised every cycle (one case among many the guard covers, not
            // the point of the test) — see BodyInterface.SetShape's handoff recipe in factories.ts
            // for the same hazard in isolation.
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
