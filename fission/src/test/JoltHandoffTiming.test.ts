// Executable proof for the RefTarget/CONSUMED manual entries in JoltClassClassification.ts that
// the jolt-ownership codegen pipeline (fission/scripts/jolt-ownership/) can't prove from source:
// a RefTarget's `return self->Method();` in glue.cpp is textually identical whether that method is
// a harmless accessor or a delegate into a genuine allocation (confirmed for
// `*ConstraintSettings.Create()` -- see the `Constraint` entry). The only way to actually know is
// to ask the object itself, via `GetRefCount()`, checked in the same tick as the handoff call so
// there's no window for the object to have already been freed elsewhere (see JoltHandoffFixtures.ts's
// header for why that timing matters -- getting this wrong while building these fixtures produced
// a real, reproducible heap corruption bug, not just a wrong assertion).
//
// Deliberately does NOT test "destroy immediately after handoff" (a real use-after-free hazard on
// a plain build) -- that's a corruption check, branch 282 (branp/282/asan-memory-test)'s job, not
// this leak-only branch's. Every test here only exercises the side JoltClassClassification.ts's
// manual entries actually claim is safe: hand off, never destroy the child directly, only tear
// down the longer-lived parent.
//
// Not gated behind LEAK_CHECK_ENABLED: these are direct GetRefCount() assertions, independent of
// the whole-suite live-count-diffing mechanism, so they run under both `bun test` and
// `bun run test:leak` for free. Safe to run under fission-leak's global before/afterEach hook too
// -- every class touched here is already classified INTERNAL_REF_UNPROVABLE (not
// MUST_RETURN_TO_BASELINE), so a nonzero delta from the intentional non-destroys below is
// console.warn'd, never thrown.
import { describe, expect, test } from "vitest"
import {
    constraintHandoffProof,
    controllerSettingsConsumptionIsClean,
    createMinimalPhysicsSystem,
    refTargetHandoffRecipes,
} from "@/test/JoltHandoffFixtures"
import JOLT from "@/util/loading/JoltSyncLoader"

describe("JoltHandoffTiming: RefTarget handoff calls actually take a reference", () => {
    for (const [name, recipe] of Object.entries(refTargetHandoffRecipes)) {
        test(`${name}: refcount increases on handoff, child is never destroy()'d directly`, () => {
            const { refCountBeforeHandoff, refCountAfterHandoff } = recipe(JOLT)
            expect(refCountAfterHandoff).toBeGreaterThan(refCountBeforeHandoff)
        })
    }

    test("PhysicsSystem.AddConstraint/RemoveConstraint: AddConstraint takes a reference, RemoveConstraint is the correct release (never destroy() the handle)", () => {
        const { refCountBeforeHandoff, refCountAfterHandoff } = constraintHandoffProof(JOLT)
        expect(refCountAfterHandoff).toBeGreaterThan(refCountBeforeHandoff)
    })

    test("VehicleConstraintSettings.mController: CONSUMES the assigned WheeledVehicleControllerSettings, parent destroy is clean", () => {
        expect(() => controllerSettingsConsumptionIsClean(JOLT)).not.toThrow()
    })

    test("JoltSettings: CONSUMES ObjectLayerPairFilterTable/BroadPhaseLayerInterfaceTable/ObjectVsBroadPhaseLayerFilterTable, parent destroy is clean", () => {
        const ctx = createMinimalPhysicsSystem(JOLT)
        expect(() => ctx.destroy()).not.toThrow()
    })
})

// --- Not covered here, on purpose, not silently ---
// - VehicleCollisionTesterCastCylinder (SetVehicleCollisionTester): needs a full vehicle
//   constraint (wheels array, controller settings, live VehicleConstraint) to exercise for real --
//   meaningfully heavier setup than the recipes above for one additional class. Left for a future
//   extension; still hand-attested in JoltClassClassification.ts in the meantime.
// - BodyID, ContactListenerJS, BroadPhaseLayer: not handoff-shaped problems at all (BodyID aliases
//   its parent Body's `[NoDelete]` address; ContactListenerJS's issue is a wrapPointer/construction
//   cache-slot mismatch; BroadPhaseLayer has genuinely conflicting evidence per its own entry) --
//   this test template doesn't apply to any of them.
// - Vec3/Quat/Mat44/RVec3/BodyID's class-level imprecision (mixing owned constructions with
//   STATIC_ALIAS contributors): not a handoff-timing question either; would need the leak harness
//   itself to move past whole-class cache-count diffing, out of scope for this file.
