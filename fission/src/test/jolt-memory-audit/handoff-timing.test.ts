// Handoff-timing template (Phase 3/4): the primary thing this suite exists to catch — see
// docs/JOLT_REFCOUNTED_DESTROY_SEMANTICS.md and the plan's Context section on RefTarget
// refcounted types. `JOLT.destroy(x)` is a raw `delete`, always, regardless of refcount; handing a
// RefTarget object to a parent (push_back/AddShape/SetShape/…) auto-`AddRef`s it, so destroying
// the caller's own handle right after handoff frees memory the parent still points to.
//
// Variant (a) (destroy immediately after handoff) is genuine undefined behavior on a plain build —
// it may read back a garbage value, or it may not, depending on what reused the freed memory. This
// file always *exercises* that code path (so a real corruption bug trips something when this suite
// runs against an ASan build — see the CI job), but only makes a hard pass/fail assertion about
// corruption when `JOLT_ASAN_DIST` is set; on a plain build it logs what it observed instead of
// asserting a specific outcome, to avoid flaking on non-deterministic memory reuse.
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import { handoffRegistry } from "./factories"

function churnHeap() {
    // Allocate/free a batch of unrelated objects to encourage the allocator to reuse memory that
    // was just freed — the same technique used to empirically confirm this bug class originally
    // (see [[jolt_refcounted_destroy_danger]]).
    for (let i = 0; i < 2000; i++) {
        const v = new JOLT.Vec3(i, i, i)
        JOLT.destroy(v)
    }
}

const isAsanBuild = Boolean(process.env.JOLT_ASAN_DIST)

describe("handoff-timing: RefTarget destroy-after-handoff hazard", () => {
    for (const [key, recipe] of Object.entries(handoffRegistry)) {
        describe(key, () => {
            test("variant (a): destroy immediately after handoff is a use-after-free hazard", () => {
                const { parent, child, teardown } = recipe.build(JOLT)
                const refCountBeforeHandoff = recipe.getRefCount(JOLT, child)
                recipe.handoff(JOLT, parent, child)
                const refCountAfterHandoff = recipe.getRefCount(JOLT, child)
                expect(refCountAfterHandoff).toBeGreaterThan(refCountBeforeHandoff)

                JOLT.destroy(child as Parameters<typeof JOLT.destroy>[0])
                churnHeap()

                // Reading through the freed handle (and even tearing down the parent afterward)
                // is genuine UB — it may return a garbage value, or it may throw a WASM
                // RuntimeError immediately (observed for BodyInterface.SetShape: teardown itself
                // crashed with "memory access out of bounds" on a plain, non-ASan build — real
                // corruption doesn't always need ASan to become visible). Both outcomes count as
                // "corruption detected"; only under the ASan build is detection required.
                let corruptionDetected = false
                let detail = ""
                try {
                    const refCountAfterChurn = recipe.getRefCount(JOLT, child)
                    if (refCountAfterChurn !== refCountAfterHandoff) {
                        corruptionDetected = true
                        detail = `refcount changed from ${refCountAfterHandoff} to ${refCountAfterChurn}`
                    }
                } catch (e) {
                    corruptionDetected = true
                    detail = `threw: ${(e as Error).message}`
                }

                if (isAsanBuild) {
                    expect(corruptionDetected).toBe(true)
                } else if (corruptionDetected) {
                    console.warn(`${key}: corruption detected without ASan (${detail})`)
                } else {
                    console.warn(
                        `${key}: UB didn't manifest this run — rerun under the ASan build (JOLT_ASAN_DIST) for a hard check.`,
                    )
                }

                try {
                    teardown()
                } catch (e) {
                    // A crash tearing down the parent after variant (a) is itself a valid
                    // corruption signal (the parent's internal state was corrupted), not a test
                    // infra failure — swallow it here.
                    console.warn(`${key}: teardown after variant (a) also threw: ${(e as Error).message}`)
                }
            })

            test("variant (b): hand off and never destroy is clean", () => {
                const { parent, child, teardown } = recipe.build(JOLT)
                const refCountBeforeHandoff = recipe.getRefCount(JOLT, child)
                recipe.handoff(JOLT, parent, child)
                const refCountAfterHandoff = recipe.getRefCount(JOLT, child)
                expect(refCountAfterHandoff).toBeGreaterThan(refCountBeforeHandoff)
                // Parent now owns a reference — do not destroy `child` ourselves, mirroring the
                // correct real-world pattern. `teardown()` destroys the parent only.
                teardown()
            })

            if (recipe.explicitRemove) {
                test("variant (c): hand off, explicitly remove, then destroy is clean", () => {
                    const { parent, child, teardown } = recipe.build(JOLT)
                    recipe.handoff(JOLT, parent, child)
                    recipe.explicitRemove!(JOLT, parent, child)
                    expect(() => JOLT.destroy(child as Parameters<typeof JOLT.destroy>[0])).not.toThrow()
                    teardown()
                })
            }
        })
    }
})
