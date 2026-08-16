// Wires the leak check into every test with zero per-test changes. Only loaded as a `setupFiles`
// entry on the `fission-leak` vitest project (see `vite.config.ts`) -- the normal `fission`
// project never imports this file, so it has no effect there.
//
// Registered here (outside any `describe`), `beforeEach`/`afterEach` apply to every test in every
// file of this project, same as if each test file had written them itself. Vitest runs `afterEach`
// hooks in reverse registration order, so a test file's own `afterEach` (e.g. `system.destroy()`)
// still runs before this one -- this snapshot always sees post-cleanup state.
import { afterEach, beforeEach } from "vitest"
import { diffLiveCountsFiltered, getTrackedClassNames, snapshotLiveCounts } from "@/test/JoltLeakDetection"
import JOLT from "@/util/loading/JoltSyncLoader"

const trackedClassNames = getTrackedClassNames(JOLT)
let before: Record<string, number>

beforeEach(() => {
    before = snapshotLiveCounts(JOLT, trackedClassNames)
})

// `diffLiveCountsFiltered` (see JoltLeakDetection.ts/JoltClassClassification.ts) throws by itself
// for any nonzero-delta class this suite hasn't classified yet -- that's intentional: it forces a
// human to actually read the new class's ownership story before it can be silenced, rather than
// this hook growing an ad-hoc ignore-list over time.
afterEach(() => {
    const after = snapshotLiveCounts(JOLT, trackedClassNames)
    const diffs = diffLiveCountsFiltered(before, after)

    if (diffs.length > 0) {
        throw new Error(
            `Jolt object(s) leaked:\n${diffs.map(d => `  ${d.className}: ${d.before} -> ${d.after} (delta ${d.delta})`).join("\n")}`
        )
    }
})
