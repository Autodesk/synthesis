/**
 * [LEAK-DEBUG] TEMPORARY instrumentation to prove Jolt Vec3 lifetime behavior.
 *
 * It tags specific wasm wrapper objects at creation with a hidden id, and hooks
 * `JOLT.destroy` to record whether each tagged object is ever actually freed.
 *
 * Matching is done on the hidden id we set on the JS wrapper - NOT on any minified
 * internal pointer field - so it is independent of the build/minification.
 *
 * Usage: after reproducing the scenario in the browser, run in the devtools console:
 *     reportJoltLeaks()      // logs a table + stacks of everything still alive
 *     resetJoltLeaks()       // clears counters to start a fresh measurement
 *
 * Remove this file and its call sites before merging.
 */
import JOLT from "./JoltSyncLoader"

interface LeakRecord {
    id: number
    label: string
    stack: string
    destroyed: boolean
}

const LEAK_KEY = "__joltLeakId"
const PATCH_FLAG = "__joltLeakPatched"

const records = new Map<number, LeakRecord>()
let nextId = 1

function patchDestroyOnce() {
    const jolt = JOLT as unknown as Record<string, unknown>
    // Survive HMR: the JOLT singleton persists across module reloads, so guard on it.
    if (jolt[PATCH_FLAG]) return
    jolt[PATCH_FLAG] = true

    const original = (jolt.destroy as (obj: unknown) => void).bind(JOLT)
    const wrapped = (obj: unknown) => {
        const id = (obj as Record<string, number> | null | undefined)?.[LEAK_KEY]
        if (typeof id === "number") {
            const rec = records.get(id)
            if (rec) rec.destroyed = true
        }
        return original(obj)
    }

    try {
        jolt.destroy = wrapped
    } catch {
        // In case `destroy` is a non-writable property.
        Object.defineProperty(jolt, "destroy", { value: wrapped, writable: true, configurable: true })
    }

    if (typeof window !== "undefined") {
        const w = window as unknown as Record<string, unknown>
        w.reportJoltLeaks = reportJoltLeaks
        w.resetJoltLeaks = resetJoltLeaks
    }

    console.info("[LEAK-DEBUG] Jolt leak tracker installed. Run reportJoltLeaks() in the console after reproducing.")
}

patchDestroyOnce()

/** Tag a Jolt wrapper object so we can later tell whether JOLT.destroy was ever called on it. */
export function trackVec<T>(label: string, vec: T): T {
    const id = nextId++
    ;(vec as Record<string, number>)[LEAK_KEY] = id
    records.set(id, {
        id,
        label,
        // Drop the top two frames (Error + trackVec) so the stack points at the call site.
        stack: (new Error().stack ?? "").split("\n").slice(2).join("\n"),
        destroyed: false,
    })
    return vec
}

export function reportJoltLeaks() {
    const all = [...records.values()]
    const live = all.filter(r => !r.destroyed)

    const byLabel = new Map<string, { leakedAlive: number; total: number }>()
    for (const r of all) {
        const e = byLabel.get(r.label) ?? { leakedAlive: 0, total: 0 }
        e.total++
        if (!r.destroyed) e.leakedAlive++
        byLabel.set(r.label, e)
    }

    console.group(
        `%c[LEAK-DEBUG] ${live.length} still ALIVE (leaked) / ${all.length} tracked`,
        "color:#e00;font-weight:bold"
    )
    console.table([...byLabel.entries()].map(([label, e]) => ({ label, leakedAlive: e.leakedAlive, total: e.total })))
    for (const r of live) {
        console.groupCollapsed(`LEAK #${r.id} - ${r.label}`)
        console.log(r.stack)
        console.groupEnd()
    }
    console.groupEnd()

    return { leaked: live.length, tracked: all.length, byLabel: Object.fromEntries(byLabel) }
}

export function resetJoltLeaks() {
    records.clear()
    nextId = 1
    console.info("[LEAK-DEBUG] leak records reset.")
}
