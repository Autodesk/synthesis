// Runs once before any jolt-memory test file/worker starts. `ownership-table.generated.json`
// is gitignored (fully derived from jolt/*.idl, only ever stale if nobody reruns codegen).
// Regenerate it here so `instrumentation.ts`'s static JSON import always resolves against the
// current IDL, both locally and in CI.
import { execSync } from "node:child_process"

export default function setup() {
    execSync("bun run jolt-memory:codegen", { stdio: "inherit" })
}
