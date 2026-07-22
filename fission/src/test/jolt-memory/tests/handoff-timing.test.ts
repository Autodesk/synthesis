// Handoff-timing: the primary hazard this suite exists to catch. See
// docs/JOLT_REFCOUNTED_DESTROY_SEMANTICS.md and the plan's Context section on RefTarget types.
// `JOLT.destroy(x)` is a raw `delete`, always, regardless of refcount. Handing a RefTarget object
// to a parent (push_back/AddShape/SetShape/...) auto-`AddRef`s it, so destroying the caller's own
// handle right after handoff frees memory the parent still points to.
//
// Variant (a) (destroy immediately after handoff) is genuine undefined behavior on a plain build:
// it may read back garbage, or it may not, depending on what reused the freed memory. This file
// always exercises that path so a real corruption bug trips under an ASan build, but only asserts
// hard pass/fail when `JOLT_ASAN_DIST` is set. On a plain build it logs what it observed instead,
// to avoid flaking on non-deterministic memory reuse.
//
// Variant (a) runs in a dedicated child process (../lib/uaf-probe.ts), not inline, because ASan's
// abort() on the real UAF isn't a catchable JS exception here. It also poisons the shared Jolt
// WASM module instance for the rest of the process, so an in-process variant (a) would take every
// other queued test down as collateral damage. A child process per probe contains the crash.
import { spawnSync } from "node:child_process"
import path from "node:path"
import { describe, expect, test } from "vitest"
import JOLT from "@/util/loading/JoltSyncLoader"
import { handoffRegistry } from "../lib/factories"

// `import.meta.url` isn't a real `file://` URL after vitest/vite transforms this module, so derive
// the probe path from the invoking process's cwd instead (always `fission/`, locally and in CI).
const uafProbePath = path.resolve(process.cwd(), "src/test/jolt-memory/lib/uaf-probe.ts")

const isAsanBuild = Boolean(process.env.JOLT_ASAN_DIST)

describe("handoff-timing: RefTarget destroy-after-handoff hazard", () => {
    for (const [key, recipe] of Object.entries(handoffRegistry)) {
        describe(key, () => {
            test("variant (a): destroy immediately after handoff is a use-after-free hazard", () => {
                const result = spawnSync("bun", [uafProbePath, key], {
                    env: process.env,
                    encoding: "utf-8",
                    timeout: 15000,
                })

                // A real crash surfaces as a nonzero exit or death by signal (platform/runtime
                // dependent).
                const crashed = result.signal !== null || (result.status !== null && result.status !== 0)

                if (isAsanBuild) {
                    expect(crashed).toBe(true)
                    return
                }

                if (crashed) {
                    // Real corruption doesn't always need ASan to become visible. Matches the
                    // observed plain-build crash for BodyInterface.SetShape (teardown itself hit
                    // "memory access out of bounds").
                    console.warn(
                        `${key}: probe process crashed without ASan (status=${result.status}, signal=${result.signal})\n${result.stderr}`,
                    )
                    return
                }

                const { corruptionDetected, detail } = JSON.parse(result.stdout.trim()) as {
                    corruptionDetected: boolean
                    detail: string
                }
                if (corruptionDetected) {
                    console.warn(`${key}: corruption detected without ASan (${detail})`)
                } else {
                    console.warn(
                        `${key}: UB didn't manifest this run, rerun under the ASan build (JOLT_ASAN_DIST) for a hard check.`,
                    )
                }
            })

            test("variant (b): hand off and never destroy is clean", () => {
                const { parent, child, teardown } = recipe.build(JOLT)
                const refCountBeforeHandoff = recipe.getRefCount(JOLT, child)
                recipe.handoff(JOLT, parent, child)
                const refCountAfterHandoff = recipe.getRefCount(JOLT, child)
                expect(refCountAfterHandoff).toBeGreaterThan(refCountBeforeHandoff)
                // Parent now owns a reference, so don't destroy `child` ourselves. `teardown()`
                // destroys the parent only.
                teardown()
            })

            if (recipe.explicitRemove) {
                test("variant (c): hand off, explicitly remove, then destroy is clean", () => {
                    const { parent, child, teardown } = recipe.build(JOLT)
                    recipe.handoff(JOLT, parent, child)
                    recipe.explicitRemove!(JOLT, parent, child)
                    expect(() => JOLT.destroy(child as Parameters<typeof JOLT.destroy>[0])).not.toThrow()
                    teardown()
                })
            }
        })
    }
})
