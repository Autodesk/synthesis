#!/usr/bin/env node
// Emits a lightweight, committed per-class hint table so a NEW unclassified-class failure (see
// `diffLiveCountsFiltered` in `JoltLeakDetection.ts`) doesn't require re-running the whole
// jolt-ownership pipeline or reading glue.cpp by hand just to get oriented. Every class the tool
// has ever seen (safe or disqualified) gets a one-line diagnostic; classes it's never seen get an
// explicit "not seen" hint instead of silence.

import { readFileSync, writeFileSync } from "node:fs"
import { fileURLToPath } from "node:url"
import path from "node:path"

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const GEN_DIR = path.join(__dirname, ".generated")
const OUT_PATH = path.join(__dirname, "../../src/test/JoltOwnershipHints.generated.ts")

const { refTargetClasses, noDestructorClasses } = JSON.parse(
    readFileSync(path.join(GEN_DIR, "function-ownership.json"), "utf8")
)
const { safe, disqualified } = JSON.parse(readFileSync(path.join(GEN_DIR, "classification.generated.json"), "utf8"))

const refTargetSet = new Set(refTargetClasses)
const noDestructorSet = new Set(noDestructorClasses)

function shortEvidence(e) {
    if (e.kind === "construct") return `new JOLT.${e.className}(...) at ${e.file}:${e.line}`
    if (e.kind === "method-return") return `${e.receiverClassName}.${e.methodName}() is ${e.category} (${e.file}:${e.line})`
    if (e.kind === "refTarget-internal-ref-unprovable")
        return `${e.receiverClassName}.${e.methodName}() is INTERNAL_REF but RefTarget-derived, unprovable (${e.file}:${e.line})`
    if (e.kind === "castObject-alias") return `JOLT.castObject(_, JOLT.${e.producedClassName}) at ${e.file}:${e.line}`
    if (e.kind === "wrapPointer-alias") return `JOLT.wrapPointer(_, JOLT.${e.producedClassName}) at ${e.file}:${e.line}`
    if (e.kind === "wrapPointer-alias-but-destroyed")
        return `JOLT.wrapPointer(_, JOLT.${e.producedClassName}) at ${e.file}:${e.line}, but destroy()'d locally`
    if (e.kind === "no-destructor-binding") return "no generated __destroy__ binding"
    return e.kind
}

function main() {
    const hints = {}
    const allTouched = new Set([...Object.keys(safe), ...Object.keys(disqualified)])

    for (const className of allTouched) {
        const evidence = safe[className] ?? [...disqualified[className].safeEvidence, ...disqualified[className].disqualifyingEvidence]
        hints[className] = {
            status: safe[className] ? "SAFE" : "DISQUALIFIED",
            isRefTarget: refTargetSet.has(className),
            hasDestructor: !noDestructorSet.has(className),
            evidence: evidence.slice(0, 5).map(shortEvidence),
        }
    }

    const header = `// GENERATED FILE -- do not hand-edit. Regenerate with: bun run jolt-ownership:generate
//
// Lightweight diagnostic hints for every class the jolt-ownership pipeline has seen (safe or
// disqualified), consulted by JoltLeakDetection.ts when a class shows a nonzero delta with no
// entry in JoltClassClassification.ts. Purpose: turn "go read glue.cpp and grep call sites
// yourself" into "read 3 lines and confirm" for the first triage pass on a genuinely new class.
// A class with no entry here was never touched by scan-callsites.mjs at all -- either added to
// fission/src after the last regen, or reached via a call shape the scanner doesn't cover yet
// ([Value] attribute field reads -- see fission/scripts/jolt-ownership/README.md).
export type OwnershipHint = {
    status: "SAFE" | "DISQUALIFIED"
    isRefTarget: boolean
    hasDestructor: boolean
    evidence: string[]
}

export const JOLT_OWNERSHIP_HINTS: Record<string, OwnershipHint> = ${JSON.stringify(hints, null, 4)}
`

    writeFileSync(OUT_PATH, header)
    console.log(`Wrote ${Object.keys(hints).length} class hints to ${OUT_PATH}`)
}

main()
