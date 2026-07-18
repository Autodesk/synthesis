// Leak-detection instrumentation for the memory-audit suite.
//
// Originally designed around `FinalizationRegistry` (construct → drop the reference → force GC →
// assert it was collected without an explicit free). That does not work with this binder: every
// `new JOLT.ClassName(...)` (and every method/getter that returns a wrapped object, via the
// internal `k(ptr, class)` helper) inserts itself into a per-class pointer cache —
// `ClassName.IDa[pointer] = wrapperInstance` in the minified build, reachable via the exposed
// `JOLT.getCache(JOLT.ClassName)` — and only removes itself when `JOLT.destroy()` runs
// (`delete cache[pointer]`). That cache is reachable for the lifetime of the JOLT module, so a
// genuinely leaked (never-destroyed) object is *never* eligible for JS garbage collection —
// confirmed empirically: a dropped, untracked `Vec3` produces zero `FinalizationRegistry`
// callbacks even after repeated forced GC. Verified live-object counting is instead:
//
//   before = Object.keys(JOLT.getCache(JOLT.Vec3)).length
//   new JOLT.Vec3(...); new JOLT.Vec3(...)
//   Object.keys(JOLT.getCache(JOLT.Vec3)).length === before + 2
//   JOLT.destroy(v1); JOLT.destroy(v2)
//   Object.keys(JOLT.getCache(JOLT.Vec3)).length === before
//
// This is deterministic and synchronous — no GC timing, no `--expose-gc` requirement — and it's
// the binder's own ground truth for "how many live instances of this class exist right now", so
// it works uniformly across the full IDL surface (constructors and method/getter returns alike,
// since both go through the same `k()` cache-insert path).
import type Jolt from "@synthesis.adsk/jolt-physics"
import { CLASS_CLASSIFICATION } from "./class-classification"
import ownershipTable from "./ownership-table.generated.json"

// `JOLT.getCache` is a real, exposed runtime function (confirmed: `Module.getCache=g` in the
// built glue) but isn't declared in the package's .d.ts, so it's accessed via a loose cast rather
// than requiring every caller to satisfy an extended type just to pass the normal `JOLT` import.
export type JoltModule = typeof Jolt
type JoltModuleWithCache = Record<string, unknown> & { getCache: (ctor: unknown) => Record<string, unknown> }

export function countLive(JOLT: JoltModule, className: string): number {
    const module = JOLT as unknown as JoltModuleWithCache
    const ctor = module[className]
    if (!ctor) throw new Error(`countLive: JOLT.${className} is not a class on this module`)
    const cache = module.getCache(ctor)
    return Object.keys(cache).length
}

export function snapshotLiveCounts(JOLT: JoltModule, classNames: string[]): Record<string, number> {
    return Object.fromEntries(classNames.map(name => [name, countLive(JOLT, name)]))
}

// Every class name the codegen table found in the IDL, deduped once at module load.
const ALL_CLASS_NAMES = [...new Set(ownershipTable.rows.map(row => row.className))]

// Same idea as `snapshotLiveCounts`, but sourced from the full codegen table instead of a
// caller-supplied list — covers every class the IDL exposes, not just what a test author
// remembered to name. Classes the current build doesn't actually expose on `JOLT` (e.g.
// debug-only rows like DebugRendererJS, absent from a non-debug build) are silently skipped
// rather than throwing, since `countLive` requires the class to exist on the module.
export function snapshotAllLiveCounts(JOLT: JoltModule): Record<string, number> {
    const module = JOLT as unknown as JoltModuleWithCache
    const result: Record<string, number> = {}
    for (const className of ALL_CLASS_NAMES) {
        if (!module[className]) continue
        result[className] = countLive(JOLT, className)
    }
    return result
}

export type LeakDiff = { className: string; before: number; after: number; delta: number }

// Positive delta: more live instances after than before — a leak (constructed, never destroyed).
// Negative delta: fewer live instances than before — destroyed something that predates this scope
// (a sign the test's teardown is destroying a shared/parent-owned object it shouldn't).
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

// Same as `diffLiveCounts`, but splits the result by `CLASS_CLASSIFICATION` bucket before handing
// back only the bucket that's actually a correctness signal. PERMANENT_SINGLETON and
// INTERNAL_REF_UNPROVABLE deltas are logged (console.warn) for visibility, never asserted on — by
// definition, nothing in this test's own scope can prove whether those classes released correctly
// or not, so treating their nonzero delta as a failure would just be noise. Any nonzero-delta class
// missing from the table fails loud rather than silently passing: growing the classification table
// is a required, deliberate step, not an incidental side effect of writing a new scenario.
export function diffLiveCountsFiltered(before: Record<string, number>, after: Record<string, number>): LeakDiff[] {
    const allDiffs = diffLiveCounts(before, after)
    const mustReturnToBaseline: LeakDiff[] = []

    for (const diff of allDiffs) {
        const classification = CLASS_CLASSIFICATION[diff.className]

        if (!classification) {
            throw new Error(
                `unclassified class ${diff.className} had nonzero delta, classify it before this test can pass ` +
                    `(before=${diff.before}, after=${diff.after}, delta=${diff.delta}) — see class-classification.ts`
            )
        }

        if (classification.bucket === "MUST_RETURN_TO_BASELINE") {
            mustReturnToBaseline.push(diff)
            continue
        }

        console.warn(
            `[${classification.bucket}] ${diff.className} delta=${diff.delta} (before=${diff.before}, after=${diff.after}): ${classification.reason}`
        )
    }

    return mustReturnToBaseline
}

export type DestroyViolation = {
    refcountAtDestroy: number
    stack: string | undefined
}

// Wraps `JOLT.destroy` for the duration of a test: any argument that duck-types as a `RefTarget`
// (has a `GetRefCount` method — no class enumeration, no IDL lookup, so it catches *any* call
// site shaped like this, not just the ones a hand-written recipe happened to name) whose refcount
// is still >1 at the moment of destruction is a use-after-free-in-waiting — `JOLT.destroy()` is a
// raw C++ delete, not a `Release()`, so it frees memory another owner still holds a pointer to
// (see [[jolt_refcounted_destroy_danger]]). Recorded rather than thrown, so the guard doesn't mask
// whatever UB actually happens next; the caller decides what a violation means for their test.
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

// Runs `fn`, then reports any class in `classNames` whose live-object count didn't return to its
// pre-call value. `classNames` should list every class the test constructs or expects to receive
// via a `COPY`-owned return, per the generated ownership table.
export async function expectNoLeaks(
    JOLT: JoltModule,
    classNames: string[],
    fn: () => void | Promise<void>,
): Promise<LeakDiff[]> {
    const before = snapshotLiveCounts(JOLT, classNames)
    await fn()
    const after = snapshotLiveCounts(JOLT, classNames)
    return diffLiveCounts(before, after)
}
