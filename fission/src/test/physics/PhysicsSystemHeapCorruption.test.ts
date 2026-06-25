// Heap-corruption repro / tracker for the "memory access out of bounds" crash
// seen when importing dozer_golden_6-16-2026.mira.
//
// The crash surfaces in SynthesisBrain.configureSkidSteerDriveBehavior at a
// JOLT.destroy() call, but that destroy is only the *victim*: free() walks an
// already-corrupted dlmalloc free list. The corrupting bad-free happens earlier,
// during mechanism construction (createBodiesFromParser / createJointsFromParser).
//
// This test instruments JOLT.destroy to catch the bad free AT ITS SOURCE rather
// than waiting for the later trap. The emscripten WebIDL binder implements
// destroy(obj) as:
//
//     obj.__destroy__();                              // dlmalloc free(ptr)
//     delete getCache(getClass(obj))[getPointer(obj)] // drop the live wrapper
//
// so the per-class cache `getCache(class)` is a { ptr -> liveWrapper } map of
// everything currently allocated. Therefore at destroy time:
//   - cache[ptr] === obj        -> normal, healthy free
//   - cache[ptr] === undefined  -> DOUBLE FREE / use-after-free (ptr already freed)
//   - cache[ptr] !== obj        -> a different live wrapper owns this ptr (aliasing)
// We also wrap the real __destroy__ in try/catch to pin the exact object whose
// free traps, and run a canary alloc/free storm afterwards to force any latent
// free-list corruption to surface deterministically.

import type Jolt from "@azaleacolburn/jolt-physics"
import { describe, expect, test } from "vitest"
import { mirabuf } from "@/proto/mirabuf"
import { unzipMira } from "@/mirabuf/MirabufLoader"
import MirabufParser from "@/mirabuf/MirabufParser"
import PhysicsSystem from "@/systems/physics/PhysicsSystem"
import { convertJoltVec3ToJoltRVec3 } from "@/util/TypeConversions"
import JOLT from "@/util/loading/JoltSyncLoader"
// Vite serves this as a URL regardless of file type; we fetch the bytes below.
import miraUrl from "../assets/dozer_golden.mira?url"

interface FreeRecord {
    seq: number
    ptr: number
    cls: string
    status: string
}

// Map every Jolt wrapper constructor to its export name so the log is readable
// (the minified browser build gives classes names like "Lq" otherwise).
function buildClassNameMap(): Map<unknown, string> {
    const map = new Map<unknown, string>()
    const joltRecord = JOLT as unknown as Record<string, unknown>
    for (const key of Object.keys(JOLT)) {
        const val = joltRecord[key]
        if (typeof val === "function" && (val as { prototype?: unknown }).prototype) {
            if (!map.has(val)) map.set(val, key)
        }
    }
    return map
}

function installDestroyTracker() {
    const classNames = buildClassNameMap()
    const log: FreeRecord[] = []
    const suspects: FreeRecord[] = []
    const joltAny = JOLT as unknown as {
        destroy: (o: unknown) => void
        getPointer: (o: unknown) => number
        getClass: (o: unknown) => unknown
        getCache: (c: unknown) => Record<number, unknown> | undefined
    }
    const original = joltAny.destroy.bind(JOLT)
    let seq = 0

    joltAny.destroy = (obj: unknown) => {
        seq++
        let ptr = -1
        let cls = "<unknown>"
        let status = "ok"
        try {
            ptr = joltAny.getPointer(obj)
            const klass = joltAny.getClass(obj)
            cls = classNames.get(klass) ?? (klass as { name?: string })?.name ?? "<anon>"
            const cache = joltAny.getCache(klass)
            const live = cache ? cache[ptr] : undefined
            if (ptr === 0) status = "NULL_PTR_FREE"
            else if (live === undefined) status = "DOUBLE_FREE/UAF (ptr no longer in live cache)"
            else if (live !== obj) status = "ALIASED_FREE (a different live wrapper owns this ptr)"
        } catch (e) {
            status = `INSPECT_ERROR: ${(e as Error).message}`
        }

        const rec: FreeRecord = { seq, ptr, cls, status }
        log.push(rec)
        if (status !== "ok") suspects.push(rec)

        try {
            original(obj)
        } catch (e) {
            const trap: FreeRecord = { seq, ptr, cls, status: `TRAP_ON_FREE: ${(e as Error).message}` }
            suspects.push(trap)
            throw e
        }
    }

    return {
        log,
        suspects,
        restore: () => {
            joltAny.destroy = original
        },
    }
}

function formatSuspects(suspects: FreeRecord[], log: FreeRecord[]): string {
    if (suspects.length === 0) return "no suspect frees detected"
    const lines = suspects.map(s => {
        // Show a little context: the few frees immediately preceding this one.
        const ctx = log
            .filter(r => r.seq >= s.seq - 4 && r.seq < s.seq)
            .map(r => `      #${r.seq} ${r.cls}@${r.ptr} (${r.status})`)
            .join("\n")
        return `  #${s.seq} ${s.cls}@${s.ptr}\n    -> ${s.status}\n    preceding frees:\n${ctx}`
    })
    return `\n${lines.join("\n\n")}`
}

describe("Heap corruption on dozer_golden import", () => {
    test("mechanism construction must not perform a bad free", async () => {
        const buffer = await fetch(miraUrl).then(r => r.arrayBuffer())
        const assembly = mirabuf.Assembly.decode(unzipMira(new Uint8Array(buffer)))
        const parser = new MirabufParser(assembly)

        const tracker = installDestroyTracker()
        const system = new PhysicsSystem()

        let trapError: Error | undefined
        try {
            // This is the exact path that builds the bodies + wheel/hinge
            // constraints that the crashing code later reads. The corrupting
            // free (if any) happens in here.
            system.createMechanismFromParser(parser)

            // Canary: hammer the allocator. If the free list is already
            // corrupted, one of these frees will trap right here instead of
            // randomly later in unrelated code.
            for (let i = 0; i < 2000; i++) {
                const v = new JOLT.Vec3(i, i, i)
                JOLT.destroy(v)
            }
        } catch (e) {
            trapError = e as Error
        } finally {
            tracker.restore()
            try {
                system.destroy()
            } catch {
                /* teardown may also trap once heap is corrupt; ignore */
            }
        }

        const report =
            `total frees tracked: ${tracker.log.length}\n` +
            `suspect frees: ${tracker.suspects.length}\n` +
            (trapError ? `trap during run: ${trapError.message}\n` : "") +
            `details:${formatSuspects(tracker.suspects, tracker.log)}`

        // Surface the diagnostics regardless of pass/fail.
        // eslint-disable-next-line no-console
        console.log(`[heap-corruption]\n${report}`)

        expect(tracker.suspects, report).toHaveLength(0)
        expect(trapError?.message ?? "", report).toBe("")
    })

    // Faithful replica of SynthesisBrain.configureSkidSteerDriveBehavior's loop,
    // run directly against the constraints built from the imported file. Mirrors
    // the FIXED freeing strategy: only the genuinely heap-allocated objects
    // (wheelPos, rightVector) are destroyed; the Jolt value-return temporaries
    // (constraintMatrix, translation, robotCOM, the SubRVec3 result) are left
    // alone. Set FREE_VALUE_TEMPS=true to reproduce the original crash.
    test("configureSkidSteerDriveBehavior loop must not perform a bad free", async () => {
        const FREE_VALUE_TEMPS = false // flip to reproduce the OOB corruption

        const buffer = await fetch(miraUrl).then(r => r.arrayBuffer())
        const assembly = mirabuf.Assembly.decode(unzipMira(new Uint8Array(buffer)))
        const parser = new MirabufParser(assembly)

        const system = new PhysicsSystem()
        const mechanism = system.createMechanismFromParser(parser)

        const fixedConstraints: Jolt.TwoBodyConstraint[] = mechanism.constraints
            .filter(c => c.primaryConstraint instanceof JOLT.TwoBodyConstraint)
            .map(c => c.primaryConstraint as Jolt.TwoBodyConstraint)

        const tracker = installDestroyTracker()
        let trapError: Error | undefined
        try {
            const rightVector = new JOLT.RVec3(1, 0, 0)
            for (let i = 0; i < fixedConstraints.length; i++) {
                const constraintMatrix = fixedConstraints[i].GetConstraintToBody1Matrix()
                const translation = constraintMatrix.GetTranslation()
                const wheelPos = convertJoltVec3ToJoltRVec3(translation, false)

                const robotCOM = system.getBody(mechanism.constraints[0].childBody)!.GetCenterOfMassPosition()

                const newPos = wheelPos.SubRVec3(robotCOM)
                rightVector.Dot(newPos)

                if (FREE_VALUE_TEMPS) {
                    // The original buggy code: frees static-temp pointers -> corruption.
                    JOLT.destroy(constraintMatrix)
                    JOLT.destroy(newPos)
                } else {
                    JOLT.destroy(wheelPos) // only real heap allocation in the loop
                }
            }
            JOLT.destroy(rightVector)

            // Canary storm to surface any latent free-list corruption now.
            for (let i = 0; i < 2000; i++) {
                const v = new JOLT.Vec3(i, i, i)
                JOLT.destroy(v)
            }
        } catch (e) {
            trapError = e as Error
        } finally {
            tracker.restore()
            try {
                system.destroy()
            } catch {
                /* ignore teardown trap on corrupt heap */
            }
        }

        const report =
            `wheel constraints: ${fixedConstraints.length}\n` +
            `total frees tracked: ${tracker.log.length}\n` +
            `suspect frees: ${tracker.suspects.length}\n` +
            (trapError ? `trap during run: ${trapError.message}\n` : "") +
            `details:${formatSuspects(tracker.suspects, tracker.log)}`

        // eslint-disable-next-line no-console
        console.log(`[heap-corruption:skidsteer]\n${report}`)

        expect(tracker.suspects, report).toHaveLength(0)
        expect(trapError?.message ?? "", report).toBe("")
    })

    // Definitive root cause: Jolt "[Value]" methods/operators return a pointer to
    // a STATIC temporary (same address every call) — NOT a heap allocation.
    // Constructors (`new`) return distinct heap pointers. Passing a static-temp
    // pointer to JOLT.destroy frees a non-heap address and corrupts dlmalloc.
    test("value-return methods alias a static temporary; constructors do not", () => {
        const getPointer = (JOLT as unknown as { getPointer: (o: unknown) => number }).getPointer

        const a = new JOLT.RVec3(5, 5, 5)
        const b = new JOLT.RVec3(1, 1, 1)
        const sub1 = getPointer(a.SubRVec3(b))
        const sub2 = getPointer(a.SubRVec3(b))
        const add1 = getPointer(a.AddRVec3(b))

        // The two value-returns the crashing loop actually frees:
        const gt1 = getPointer(JOLT.Mat44.prototype.sIdentity().GetTranslation())
        const gt2 = getPointer(JOLT.Mat44.prototype.sIdentity().GetTranslation())
        // eslint-disable-next-line no-console
        console.log(`[static-temp proof] GetTranslation call#1=${gt1} call#2=${gt2} (same=${gt1 === gt2})`)
        expect(gt1, "GetTranslation also returns a reused static temporary").toBe(gt2)

        const newPtrs = [getPointer(new JOLT.RVec3(1, 1, 1)), getPointer(new JOLT.RVec3(2, 2, 2))]

        // eslint-disable-next-line no-console
        console.log(
            `[static-temp proof] SubRVec3 call#1=${sub1} call#2=${sub2} (same=${sub1 === sub2})  ` +
                `AddRVec3=${add1}  new RVec3 ptrs=${newPtrs.join(",")} (same=${newPtrs[0] === newPtrs[1]})`
        )

        // Two calls to the same value-return op hand back the identical pointer:
        expect(sub1, "SubRVec3 returns a reused static temporary").toBe(sub2)
        // Constructors hand back distinct heap allocations:
        expect(newPtrs[0], "constructors allocate distinct heap blocks").not.toBe(newPtrs[1])
    })

    // dev (#1320) still does `JOLT.destroy(constraintMatrix)` in
    // configureSkidSteerDriveBehavior. This proves GetConstraintToBody1Matrix is
    // ALSO a reused static temporary (not a heap allocation), so freeing it is a
    // corruption bug and NOT freeing it is not a leak — justifying its removal.
    test("GetConstraintToBody1Matrix returns a static temporary (must not be freed)", async () => {
        const getPointer = (JOLT as unknown as { getPointer: (o: unknown) => number }).getPointer
        const buffer = await fetch(miraUrl).then(r => r.arrayBuffer())
        const assembly = mirabuf.Assembly.decode(unzipMira(new Uint8Array(buffer)))
        const parser = new MirabufParser(assembly)
        const system = new PhysicsSystem()
        const mechanism = system.createMechanismFromParser(parser)

        const c = mechanism.constraints
            .map(mc => mc.primaryConstraint)
            .find(pc => pc instanceof JOLT.TwoBodyConstraint) as Jolt.TwoBodyConstraint

        const m1 = getPointer(c.GetConstraintToBody1Matrix())
        const m2 = getPointer(c.GetConstraintToBody1Matrix())
        // eslint-disable-next-line no-console
        console.log(`[static-temp proof] GetConstraintToBody1Matrix call#1=${m1} call#2=${m2} (same=${m1 === m2})`)
        expect(m1, "GetConstraintToBody1Matrix returns a reused static temporary").toBe(m2)

        try {
            system.destroy()
        } catch {
            /* ignore */
        }
    })
})
