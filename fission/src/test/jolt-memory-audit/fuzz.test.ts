// Sequencing/interaction fuzz (Phase 5): random-order create/destroy permutations across the
// factory-backed classes. Catches lifetime bugs that straight-line per-function tests can't see
// (e.g. order-dependent aliasing) by interleaving many classes' construct/destroy calls instead of
// testing one class in isolation.
//
// The plan originally called for `fast-check`, but this environment's package registry
// (fission/.npmrc points at Autodesk's private Artifactory) isn't reachable here, and installing
// a new dependency without network access isn't safe to attempt blind. A small seeded PRNG +
// Fisher-Yates shuffle gets the same property — many random orderings, deterministic per seed,
// no new dependency — so that's what this file uses instead. Swap in `fast-check` later if/when
// registry access is available and richer shrinking/generators are worth the dependency.
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import { factories } from "./factories"
import { expectNoLeaks } from "./instrumentation"

// mulberry32 — tiny deterministic PRNG, good enough for shuffling test order (not cryptography).
function mulberry32(seed: number) {
    let a = seed
    return () => {
        a |= 0
        a = (a + 0x6d2b79f5) | 0
        let t = Math.imul(a ^ (a >>> 15), 1 | a)
        t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t
        return ((t ^ (t >>> 14)) >>> 0) / 4294967296
    }
}

function shuffle<T>(items: T[], rand: () => number): T[] {
    const result = [...items]
    for (let i = result.length - 1; i > 0; i--) {
        const j = Math.floor(rand() * (i + 1))
        ;[result[i], result[j]] = [result[j], result[i]]
    }
    return result
}

const CLASS_NAMES = Object.keys(factories)
const SEEDS = [1, 2, 3, 4, 5]

describe("sequencing fuzz: random-order construct/destroy", () => {
    for (const seed of SEEDS) {
        test(`seed ${seed}: no leak regardless of construct/destroy order`, async () => {
            const rand = mulberry32(seed)
            const order = shuffle(CLASS_NAMES, rand)

            const diffs = await expectNoLeaks(JOLT, CLASS_NAMES, () => {
                // Construct all instances in one random order, then destroy them in a second,
                // independent random order — exercises interleaving, not just construct-then-
                // immediately-destroy pairs.
                const instances = order.map(className => ({
                    className,
                    instance: factories[className](JOLT),
                }))
                const destroyOrder = shuffle(instances, rand)
                for (const { instance } of destroyOrder) {
                    JOLT.destroy(instance as Parameters<typeof JOLT.destroy>[0])
                }
            })

            expect(diffs).toEqual([])
        })
    }
})
