import type Jolt from "@synthesis.adsk/jolt-physics"
import { describe, expect, test } from "vitest"
import { LEAK_CHECK_ENABLED, checkForLeaks, diffLiveCounts, snapshotLiveCounts } from "@/test/JoltLeakDetection"
import JOLT from "@/util/loading/JoltSyncLoader"

// Self-contained meta-tests for the leak detector itself (deliberately not exercised against real
// app code here, since a real object's ownership graph can be more subtle than it looks -- see
// git history on this file's introduction for a case where a plausible-looking fix based on this
// detector's finding turned out to corrupt the WASM heap instead of fixing a leak). Only run under
// the `fission-leak` project (`bun run test:leak`): under the normal `fission` project,
// `checkForLeaks` no-ops, so the "fails when leaked" case below would never throw.
//
// Uses `IndexedTriangle` as the "plain, unclassified" example class throughout: it's a simple
// value type nothing in `JoltClassClassification.ts` has an entry for, so a leaked one is always
// a hard failure -- unlike `Vec3`, which IS classified (INTERNAL_REF_UNPROVABLE), on purpose, to
// demonstrate the classification table's documented caveat below.
describe.runIf(LEAK_CHECK_ENABLED)("JoltLeakDetection", () => {
    test("passes when every constructed IndexedTriangle is destroyed", async () => {
        await checkForLeaks(JOLT, ["IndexedTriangle"], () => {
            const t = new JOLT.IndexedTriangle(0, 1, 2, 0)
            JOLT.destroy(t)
        })
    })

    test("fails when an IndexedTriangle is leaked", async () => {
        // Captured so it can be cleaned up below -- otherwise this intentional leak would also
        // trip JoltLeakDetectionSetup.ts's own global afterEach, failing this test for the wrong
        // reason (the outer hook doesn't know this leak was expected mid-test).
        let leaked: Jolt.IndexedTriangle | undefined

        await expect(
            checkForLeaks(JOLT, ["IndexedTriangle"], () => {
                leaked = new JOLT.IndexedTriangle(0, 1, 2, 0) // intentionally never destroyed
            })
        ).rejects.toThrow(/IndexedTriangle/)

        JOLT.destroy(leaked!)
    })

    test("a classified class's leak is silently accepted by checkForLeaks (documented caveat)", async () => {
        // Vec3 is classified INTERNAL_REF_UNPROVABLE, so a leaked one is logged (console.warn),
        // never thrown -- checkForLeaks only asserts on MUST_RETURN_TO_BASELINE-bucket classes.
        await expect(
            checkForLeaks(JOLT, ["Vec3"], () => {
                new JOLT.Vec3(1, 2, 3) // leaked, same as the IndexedTriangle case above
            })
        ).resolves.toBeUndefined()
    })

    test("diffLiveCounts (unfiltered) still reports a classified class's nonzero delta", () => {
        const before = snapshotLiveCounts(JOLT, ["Vec3", "Quat"])
        const v = new JOLT.Vec3(1, 2, 3)
        const after = snapshotLiveCounts(JOLT, ["Vec3", "Quat"])
        JOLT.destroy(v)

        expect(diffLiveCounts(before, after)).toEqual([
            { className: "Vec3", before: before.Vec3, after: after.Vec3, delta: 1 },
        ])
    })
})
