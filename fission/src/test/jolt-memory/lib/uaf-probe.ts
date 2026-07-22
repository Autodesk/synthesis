// Standalone crash-test probe for handoff-timing.test.ts's variant (a): destroy immediately after
// handoff. Runs as its own OS process (spawned via child_process), never imported directly by a
// test file.
//
// Why: under the ASan build, a real use-after-free doesn't raise a catchable JS exception. ASan's
// abort() poisons the shared Jolt WASM module for the rest of the worker process, so other queued
// tests sharing that process fail too, unrelated to their own correctness. Running each probe in
// a fresh child process contains the blast radius to just this one probe.
import { handoffRegistry } from "./factories"

const key = process.argv[2]
const recipe = key ? handoffRegistry[key] : undefined
if (!recipe) {
    console.error(`uaf-probe: unknown handoff key: ${key}`)
    process.exit(2)
}

const JOLT = process.env.JOLT_ASAN_DIST
    ? await (await import(process.env.JOLT_ASAN_DIST)).default()
    : await (await import("@synthesis.adsk/jolt-physics/wasm-compat")).default()

function churnHeap() {
    // Allocate/free unrelated objects to encourage the allocator to reuse memory just freed.
    // See [[jolt_refcounted_destroy_danger]].
    for (let i = 0; i < 2000; i++) {
        const v = new JOLT.Vec3(i, i, i)
        JOLT.destroy(v)
    }
}

const { parent, child, teardown } = recipe.build(JOLT)
const refCountBeforeHandoff = recipe.getRefCount(JOLT, child)
recipe.handoff(JOLT, parent, child)
const refCountAfterHandoff = recipe.getRefCount(JOLT, child)
if (!(refCountAfterHandoff > refCountBeforeHandoff)) {
    console.error(
        `uaf-probe: handoff did not increase refcount for ${key}: ${refCountBeforeHandoff} -> ${refCountAfterHandoff}`,
    )
    process.exit(3)
}

JOLT.destroy(child as Parameters<typeof JOLT.destroy>[0])
churnHeap()

// Reading through the freed handle (and even tearing down the parent afterward) is genuine UB. It
// may return a garbage value, or throw a WASM RuntimeError immediately (observed for
// BodyInterface.SetShape: teardown itself crashed with "memory access out of bounds" on a plain,
// non-ASan build). Both outcomes count as "corruption detected" for the plain-build report. Under
// ASan, this whole process is expected to abort before ever reaching the JSON below.
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

try {
    teardown()
} catch (e) {
    corruptionDetected = true
    detail += (detail ? "; " : "") + `teardown threw: ${(e as Error).message}`
}

console.log(JSON.stringify({ corruptionDetected, detail }))
// Emscripten's abort() (what ASan calls on a real UAF) throws a catchable JS `ExitStatus`, not an
// OS-level signal, so the try/catches above would swallow it and exit 0 even when ASan caught
// corruption. Encode the result in the exit code instead, since the parent test (with
// JOLT_ASAN_DIST set) only inspects exit status, not stdout.
process.exit(corruptionDetected ? 1 : 0)
