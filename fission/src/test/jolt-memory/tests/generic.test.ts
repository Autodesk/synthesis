// Generic per-class template: for every class with a seeded factory, a clean construct -> destroy
// round trip must leave the binder's live-object cache exactly where it started. See
// instrumentation.ts for why this replaces a FinalizationRegistry-based approach.
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import { factories } from "../lib/factories"
import { countLive, expectNoLeaks } from "../lib/instrumentation"

describe("generic construct/destroy round trip", () => {
    for (const className of Object.keys(factories)) {
        test(`${className}: construct then destroy leaves no leak`, async () => {
            const diffs = await expectNoLeaks(JOLT, [className], () => {
                const instance = factories[className](JOLT)
                JOLT.destroy(instance as Parameters<typeof JOLT.destroy>[0])
            })
            expect(diffs).toEqual([])
        })
    }

    // Proves the leak detector catches a real leak, not just a tautology. Uses its own class so
    // before/after counts stay exact regardless of test execution order.
    test("meta: a never-destroyed instance IS reported as a leak", async () => {
        const diffs = await expectNoLeaks(JOLT, ["IndexedTriangle"], () => {
            factories.IndexedTriangle(JOLT) // intentionally never destroyed
        })
        expect(diffs).toEqual([{ className: "IndexedTriangle", before: 0, after: 1, delta: 1 }])
    })

    test("meta: countLive reflects concurrently live instances of the same class", () => {
        const before = countLive(JOLT, "Vec3")
        const a = factories.Vec3(JOLT)
        const b = factories.Vec3(JOLT)
        expect(countLive(JOLT, "Vec3")).toBe(before + 2)
        JOLT.destroy(a as Parameters<typeof JOLT.destroy>[0])
        JOLT.destroy(b as Parameters<typeof JOLT.destroy>[0])
        expect(countLive(JOLT, "Vec3")).toBe(before)
    })
})
