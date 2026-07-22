// One integration exercise: a full create/simulate/teardown cycle through a real physics world
// (not a single isolated call), with the same leak-detection instrumentation attached. Catches
// cross-component bugs pure API-surface reasoning can't see.
//
// This doesn't drive through fission's own `PhysicsSystem` app class directly because that
// doesn't work in this suite's plain-Node environment: `PhysicsSystem.ts` transitively imports
// `MirabufLoader.ts`, which touches `window.localStorage` in a static initializer, and
// `DefaultMatchModeConfigs.ts`, which calls `fetch()` with a relative URL. Both assume a browser
// context and throw/reject immediately under Node (`ReferenceError: window is not defined` /
// `TypeError: Failed to parse URL from /api/match_configs/manifest.json`). That coupling is
// pre-existing and unrelated to this suite, and isn't fixed here. So this test builds the same
// minimal physics world shape fission's real `PhysicsSystem.createBox`/`createBody` build (see
// factories.ts's `createMinimalPhysicsSystem`/`createBoxBody`, copied from the same construction
// order), rather than importing the app class.
//
// Scope note: `BodyInterface.SetShape` and `VehicleConstraint.SetVehicleCollisionTester` (the
// other two call sites from docs/JOLT_REFCOUNTED_DESTROY_SEMANTICS.md) have no real call site in
// fission/src today, since `PhysicsSystem.ts` never calls either. They're already covered, as far
// as they can be, by the harness's own recipes in handoff-timing.test.ts. There is no real app
// code path to integration-test them against yet.
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import { createBoxBody, createMinimalPhysicsSystem, createStandaloneBoxShape, type MinimalPhysicsContext } from "../lib/factories"
import { expectNoLeaks } from "../lib/instrumentation"

describe("integration: create/simulate/teardown through a real physics world", () => {
    test("create body, step simulation, teardown leaves no leak in the value types it owns directly", async () => {
        let ctx: MinimalPhysicsContext | undefined
        const diffs = await expectNoLeaks(JOLT, ["RVec3", "Quat", "BodyCreationSettings"], () => {
            ctx = createMinimalPhysicsSystem(JOLT)
            const shape = createStandaloneBoxShape(JOLT)
            const body = createBoxBody(JOLT, ctx, shape)
            shape.Release() // ownership transferred to BodyCreationSettings -> Body, drop our own AddRef

            ctx.bodyInterface.AddBody(body.GetID(), JOLT.EActivation_Activate)
            ctx.joltInterface.Step(1.0 / 60.0, 1)

            ctx.bodyInterface.RemoveBody(body.GetID())
            ctx.bodyInterface.DestroyBody(body.GetID())
        })
        ctx!.destroy()
        expect(diffs).toEqual([])
    })

    test("shape handed to BodyCreationSettings is correctly retained, not a leak", () => {
        // `Shape` is RefTarget (see the plan's Context section on RefTarget/handoff-timing).
        // `createBoxBody` hands the shape to `BodyCreationSettings`, and this test never destroys
        // its own handle afterward (only `Release()`s the AddRef it took). Per
        // docs/JOLT_REFCOUNTED_DESTROY_SEMANTICS.md's rule, that's the *correct* pattern (let the
        // Body's own teardown eventually release it), not a leak.
        //
        // Deliberately NOT using `expectNoLeaks`'s cache-size diffing for this claim (unlike the
        // test above). A `RefTarget` freed correctly via C++ refcounting (never passed to
        // `JOLT.destroy()`) never has its JS wrapper cache entry removed, since the binder only
        // does `delete cache[pointer]` inside `JOLT.destroy()` itself. If a later, unrelated
        // allocation reuses that freed pointer, `k()`'s cache lookup hits the stale entry and
        // hands back the old wrapper instead of allocating a new one, so the cache's key count
        // doesn't grow: a false negative for this shape (running this file's first test before
        // this one reproduces it, since the second shape's construction shows zero net change in
        // `Shape`'s cache size by reusing the first test's already-freed pointer/cache entry).
        // Cache-size diffing is the wrong tool for "did this specific RefTarget survive its
        // handoff": reading the instance's own `GetRefCount()` directly is unambiguous and avoids
        // the cross-test aliasing hazard.
        const ctx = createMinimalPhysicsSystem(JOLT)
        const shape = createStandaloneBoxShape(JOLT)
        const body = createBoxBody(JOLT, ctx, shape)
        shape.Release()
        ctx.bodyInterface.AddBody(body.GetID(), JOLT.EActivation_Activate)

        // Still alive and holding exactly the Body/BodyCreationSettings chain's reference. If it
        // had been wrongly freed, this read would throw or return garbage (see
        // handoff-timing.test.ts's variant (a)).
        expect(shape.GetRefCount()).toBe(1)

        ctx.destroy()
    })
})
