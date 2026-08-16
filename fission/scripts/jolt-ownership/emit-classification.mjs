#!/usr/bin/env node
// Turns classification.generated.json's provably-safe set into a real, committed TypeScript
// source file with the same `ClassClassification` shape JoltClassClassification.ts already uses,
// so the leak-check test never has to run this pipeline (or need emscripten installed) itself --
// this is a dev-time step, re-run manually when jolt/JoltJS.idl or fission/src's Jolt call sites
// change, same as the old memory-audit plan's `jolt-audit:codegen`.

import { readFileSync, writeFileSync } from "node:fs"
import { fileURLToPath } from "node:url"
import path from "node:path"

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const GEN_DIR = path.join(__dirname, ".generated")
const OUT_PATH = path.join(__dirname, "../../src/test/JoltClassClassification.generated.ts")

const { safe } = JSON.parse(readFileSync(path.join(GEN_DIR, "classification.generated.json"), "utf8"))

function summarizeEvidence(evidence) {
    const groups = new Map()
    for (const e of evidence) {
        let key, description
        if (e.kind === "method-return") {
            key = `method:${e.receiverClassName}.${e.methodName}`
            description =
                `\`${e.receiverClassName}.${e.methodName}()\` is ${e.category} in generated glue.cpp` +
                (e.glueLine ? ` (glue.cpp:${e.glueLine})` : "") +
                `, confirmed as this class's only non-alias contributor`
        } else if (e.kind === "castObject-alias") {
            key = `castObject:${e.producedClassName}`
            description =
                `only ever populated via \`JOLT.castObject(_, JOLT.${e.producedClassName})\`, which the binder ` +
                `implements as \`castObject=function(a,b){return k(a.GDa,b)}\` -- the same cache-getter \`k()\` ` +
                `constructors/getters use, called with the existing pointer, never a new allocation`
        } else if (e.kind === "wrapPointer-alias") {
            key = `wrapPointer:${e.producedClassName}`
            description =
                `only ever populated via \`JOLT.wrapPointer(_, JOLT.${e.producedClassName})\` (same never-allocates ` +
                `\`k()\` cache-getter as castObject)` +
                (e.boundName
                    ? `; checked that no \`JOLT.destroy()\` call site in the same file targets its bound local ` +
                      `(\`${e.boundName}\`)`
                    : `; embedded directly in an object literal with no local binding to check -- safety here ` +
                      `rests on the transient-callback-argument contract (ContactListenerJS's *Ptr args), not an ` +
                      `absence-of-destroy scan`)
        } else if (e.kind === "no-destructor-binding") {
            key = "no-destructor"
            description =
                `has no generated \`__destroy__\` binding at all (glue.cpp confirms no \`emscripten_bind_<Class>___destroy___0\`) -- ` +
                `\`JOLT.destroy()\` throws \`"Cannot destroy object. (Did you create it yourself?)"\` unconditionally, ` +
                `for every instance, independent of any call site`
        } else {
            continue
        }

        if (!groups.has(key)) groups.set(key, { description, sites: [] })
        if (e.file) groups.get(key).sites.push(`${e.file}:${e.line}`)
    }
    return [...groups.values()]
}

function renderReason(className, evidence) {
    const groups = summarizeEvidence(evidence)
    const parts = groups.map(g => {
        const siteNote =
            g.sites.length === 0
                ? ""
                : g.sites.length <= 2
                  ? ` (${g.sites.join(", ")})`
                  : ` (${g.sites.length} call sites, e.g. ${g.sites[0]})`
        return g.description + siteNote
    })
    return parts.join("; ") + "."
}

function main() {
    const classNames = Object.keys(safe).sort()
    const entries = classNames
        .map(className => {
            const reason = renderReason(className, safe[className])
            const escaped = reason.replace(/\\/g, "\\\\").replace(/`/g, "\\`")
            return `    ${className}: {\n        bucket: "INTERNAL_REF_UNPROVABLE",\n        reason:\n            \`${escaped}\`,\n    },`
        })
        .join("\n")

    const header = `// GENERATED FILE -- do not hand-edit. Regenerate with:
//   bun run jolt-ownership:generate
// (see fission/scripts/jolt-ownership/README.md for what each step does and why)
//
// Every entry below is proven, not attributed: each one's reason cites the exact generated
// glue.cpp line (ground truth for what the binder's C++ actually does, from webidl_binder.py
// over jolt/JoltJS.idl) and the fission/src production call site(s) that exercise it. A class only
// appears here if EVERY production call site that produces an instance of it is a return the
// binder can never legally destroy() (STATIC_ALIAS / INTERNAL_REF on a non-RefTarget class /
// ALIASES_THIS), a pure castObject/wrapPointer address-alias, or a class with no generated
// __destroy__ binding at all. A single disqualifying call site (a real allocation, a RefTarget's
// INTERNAL_REF -- which can hide a genuine refcounted allocation behind an identical-looking
// glue.cpp shape, see Constraint/Shape's manual entries in JoltClassClassification.ts -- or
// anything the tool couldn't classify) drops the class from this file entirely; it stays a manual,
// human-attested entry in JoltClassClassification.ts instead. See
// fission/scripts/jolt-ownership/generate-classification.mjs for the exact join logic.
import type { ClassClassification } from "@/test/JoltClassClassification"

export const GENERATED_CLASS_CLASSIFICATION: Record<string, ClassClassification> = {
${entries}
}
`

    writeFileSync(OUT_PATH, header)
    console.log(`Wrote ${classNames.length} generated entries to ${OUT_PATH}`)
}

main()
