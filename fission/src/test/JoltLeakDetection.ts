// biome-ignore-all lint/style/useNamingConvention: Match Jolt functions
// Leak-detection instrumentation, ported from the `memory-audit` branch's dedicated
// `jolt-memory` test subproject (`fission/src/test/jolt-memory/lib/instrumentation.ts`). Only the
// self-contained pieces are kept here -- no codegen ownership table, no class-classification
// allow-list -- so this can be dropped into any existing test without extra infrastructure. See
// the `memory-audit` branch if a future test needs the full audit-everything machinery.
//
// `FinalizationRegistry` doesn't work with this binder: every `new JOLT.ClassName(...)` (and
// every method/getter returning a wrapped object) inserts itself into a per-class pointer cache
// (`JOLT.getCache(JOLT.ClassName)`), removed only when `JOLT.destroy()` runs. That cache is
// reachable for the module's lifetime, so a leaked object never becomes GC-eligible -- a dropped,
// untracked object produces zero `FinalizationRegistry` callbacks even after a forced GC. Live-
// object counting via that cache is deterministic and synchronous instead, with no GC timing or
// `--expose-gc` needed.
import type Jolt from "@synthesis.adsk/jolt-physics"
import { CLASS_CLASSIFICATION } from "@/test/JoltClassClassification"

export type JoltModule = typeof Jolt
type JoltModuleWithCache = Record<string, unknown> & { getCache: (ctor: unknown) => Record<string, unknown> }

export function countLive(JOLT: JoltModule, className: string): number {
    const module = JOLT as unknown as JoltModuleWithCache
    const ctor = module[className]
    if (!ctor) throw new Error(`countLive: JOLT.${className} is not a class on this module`)
    return Object.keys(module.getCache(ctor)).length
}

export function snapshotLiveCounts(JOLT: JoltModule, classNames: string[]): Record<string, number> {
    return Object.fromEntries(classNames.map(name => [name, countLive(JOLT, name)]))
}

// Auto-discovers every cache-tracked class on `JOLT`, so callers don't need a hand-maintained (or
// codegen'd) list of class names. `getCache` never throws -- it's `(ctor) => (ctor || fallback).IDa`
// in the built glue -- so this just probes every own key that looks like a class constructor and
// keeps the ones whose cache is actually a live object.
export function getTrackedClassNames(JOLT: JoltModule): string[] {
    const module = JOLT as unknown as JoltModuleWithCache
    return Object.keys(module)
        .filter(key => typeof module[key] === "function" && "prototype" in (module[key] as object))
        .filter(key => {
            const cache = module.getCache(module[key])
            return typeof cache === "object" && cache !== null
        })
}

export type LeakDiff = { className: string; before: number; after: number; delta: number }

// Positive delta: more live instances after than before, a leak (constructed, never destroyed).
// Negative delta: fewer live instances than before, meaning something predating this scope got
// destroyed, a sign the checked code is destroying a shared/parent-owned object it shouldn't.
export function diffLiveCounts(before: Record<string, number>, after: Record<string, number>): LeakDiff[] {
    return Object.keys(after)
        .map(className => ({
            className,
            before: before[className] ?? 0,
            after: after[className],
            delta: after[className] - (before[className] ?? 0),
        }))
        .filter(d => d.delta !== 0)
}

function formatDiffs(diffs: LeakDiff[]): string {
    return diffs.map(d => `  ${d.className}: ${d.before} -> ${d.after} (delta ${d.delta})`).join("\n")
}

// Same as `diffLiveCounts`, but splits by `CLASS_CLASSIFICATION` bucket and returns only the
// bucket that's a real correctness signal. PERMANENT_SINGLETON and INTERNAL_REF_UNPROVABLE deltas
// are logged (console.warn) for visibility, never asserted on, since nothing at this level can
// prove whether those classes released correctly. Any nonzero-delta class missing from the table
// fails loud rather than passing silently, since growing the classification table must stay a
// deliberate step (see JoltClassClassification.ts's header).
export function diffLiveCountsFiltered(before: Record<string, number>, after: Record<string, number>): LeakDiff[] {
    const allDiffs = diffLiveCounts(before, after)
    const mustReturnToBaseline: LeakDiff[] = []
    const unclassified: LeakDiff[] = []

    for (const diff of allDiffs) {
        const classification = CLASS_CLASSIFICATION[diff.className]

        if (!classification) {
            unclassified.push(diff)
            continue
        }

        if (classification.bucket === "MUST_RETURN_TO_BASELINE") {
            mustReturnToBaseline.push(diff)
            continue
        }

        console.warn(
            `[${classification.bucket}] ${diff.className} delta=${diff.delta} (before=${diff.before}, after=${diff.after}): ${classification.reason}`
        )
    }

    // Reported together (not one-at-a-time) so a single run surfaces every class this test needs
    // classified, instead of a whack-a-mole discovery loop across repeated runs.
    if (unclassified.length > 0) {
        throw new Error(
            `unclassified class(es) had a nonzero delta, classify them before this check can pass ` +
                `(see JoltClassClassification.ts):\n${formatDiffs(unclassified)}`
        )
    }

    return mustReturnToBaseline
}

// True only when the `fission-leak` vitest project is running (see `vite.config.ts` and
// `bun run test:leak`), never on the normal `fission` project. This is what makes
// `checkForLeaks` an opt-in check: leaving a call to it in a test does not change behavior or
// cost anything on a normal test run.
export const LEAK_CHECK_ENABLED = import.meta.env.VITE_JOLT_LEAK_CHECK === "1"

// Snapshots `classNames`' live counts (per `JOLT.getCache`) before and after `fn`, and throws if
// any of them didn't return to baseline. No-ops (just awaits `fn`) unless `LEAK_CHECK_ENABLED`,
// so this is safe to leave in place on a test that also runs under the normal `fission` project.
// `classNames` should list every Jolt class the checked code constructs or expects to receive
// ownership of.
export async function checkForLeaks(
    JOLT: JoltModule,
    classNames: string[],
    fn: () => void | Promise<void>
): Promise<void> {
    if (!LEAK_CHECK_ENABLED) {
        await fn()
        return
    }

    const before = snapshotLiveCounts(JOLT, classNames)
    await fn()
    const after = snapshotLiveCounts(JOLT, classNames)
    const diffs = diffLiveCountsFiltered(before, after)

    if (diffs.length > 0) {
        throw new Error(`Jolt object(s) leaked:\n${formatDiffs(diffs)}`)
    }
}

export type DestroyViolation = { refcountAtDestroy: number; stack: string | undefined }

// Wraps `JOLT.destroy` for the duration of a test. Any argument duck-typed as a `RefTarget` (has
// a `GetRefCount` method) whose refcount is still >1 at destruction is a use-after-free waiting to
// happen: `JOLT.destroy()` is a raw C++ delete, not `Release()`, so it frees memory another owner
// still holds a pointer to. Recorded rather than thrown, so the guard doesn't mask whatever UB
// happens next -- the caller decides what a violation means for their test. Independent of
// `LEAK_CHECK_ENABLED`: this is synchronous and cheap, and catches a different bug class (a bad
// free) than the live-count diff above (a missing free).
export function patchDestroyGuard(JOLT: JoltModule): { violations: DestroyViolation[]; unpatch: () => void } {
    const module = JOLT as unknown as Record<string, (obj: unknown) => void>
    const original = module.destroy
    const violations: DestroyViolation[] = []

    module.destroy = (obj: unknown) => {
        const maybeRefTarget = obj as { GetRefCount?: () => number } | null | undefined
        if (typeof maybeRefTarget?.GetRefCount === "function") {
            const refcountAtDestroy = maybeRefTarget.GetRefCount()
            if (refcountAtDestroy > 1) {
                violations.push({ refcountAtDestroy, stack: new Error().stack })
            }
        }
        original(obj)
    }

    return {
        violations,
        unpatch: () => {
            module.destroy = original
        },
    }
}
