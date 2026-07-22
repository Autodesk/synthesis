// Sequencing/interaction fuzz: random-order create/destroy permutations across the
// factory-backed classes. Catches lifetime bugs that straight-line per-function tests can't see
// (e.g. order-dependent aliasing) by interleaving many classes' construct/destroy calls instead of
// testing one class in isolation.
import fc from "fast-check"
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import { factories } from "../lib/factories"
import { expectNoLeaks } from "../lib/instrumentation"

const CLASS_NAMES = Object.keys(factories)

describe("sequencing fuzz: random-order construct/destroy", () => {
    test("no leak regardless of construct/destroy order", async () => {
        await fc.assert(
            fc.asyncProperty(
                fc.shuffledSubarray(CLASS_NAMES, { minLength: CLASS_NAMES.length }),
                fc.shuffledSubarray(CLASS_NAMES, { minLength: CLASS_NAMES.length }),
                async (constructOrder, destroyOrder) => {
                    const diffs = await expectNoLeaks(JOLT, CLASS_NAMES, () => {
                        // Construct all instances in one random order, then destroy them in a
                        // second, independent random order. Exercises interleaving, not just
                        // construct-then-immediately-destroy pairs.
                        const instances = constructOrder.map(className => ({
                            className,
                            instance: factories[className](JOLT),
                        }))
                        const byName = new Map(instances.map(({ className, instance }) => [className, instance]))
                        for (const className of destroyOrder) {
                            JOLT.destroy(byName.get(className) as Parameters<typeof JOLT.destroy>[0])
                        }
                    })

                    expect(diffs).toEqual([])
                },
            ),
            { numRuns: 25 },
        )
    })
})
