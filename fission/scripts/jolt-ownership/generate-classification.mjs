#!/usr/bin/env node
// Joins function-ownership.json (glue.cpp ground truth) with call-sites.json (every place
// fission/src production code actually touches a tracked Jolt class) to decide, per class, whether
// its cache growth is provably always non-owned -- and therefore safe to exclude from the leak
// check -- or whether at least one contributing call site is a genuine, destroyable allocation.
//
// A class is only ever marked SAFE if *every* contributing production call site is one of:
//   - a method/getter return classified STATIC_ALIAS / INTERNAL_REF / ALIASES_THIS in glue.cpp
//   - a JOLT.castObject(_, JOLT.X) target (binder-level fact: never allocates, same address,
//     different cache key -- confirmed from the built glue's `castObject=function(a,b){return
//     k(a.GDa,b)}`, the same k() cache-getter `new`/getters use)
//   - a JOLT.wrapPointer(_, JOLT.X) target, same never-allocates fact, PLUS a same-file check that
//     no `JOLT.destroy()` call site targets that wrapPointer's bound local variable (best-effort:
//     many of these are embedded directly in an object literal with no local binding at all, in
//     which case there's nothing to check locally and the safety claim rests on the transient-
//     callback-argument contract, same as the hand-authored file already assumed -- flagged
//     explicitly in the generated reason, not silently upgraded to full proof)
//   - a class with no generated `__destroy__` binding at all ([NoDelete] in the IDL) -- destroy()
//     throws unconditionally for every instance, independent of any call site
//
// Any `new JOLT.X(...)` construction, or any call resolving to a COPY-returning function, or any
// UNKNOWN-shaped glue.cpp body, disqualifies the class from the safe set entirely -- conservative
// by design: the goal is zero false "safe" verdicts, not maximum coverage. Real RefTarget
// refcounting-freed classes (Shape, Constraint, BoxShape, ...) are NOT provable this way at all --
// proving those needs Jolt's actual AddRef/Release call graph, not just glue.cpp's return-pattern
// text, so they correctly fall out of the safe set here and stay a manual, hand-attested entry.

import { readFileSync, writeFileSync } from "node:fs"
import { fileURLToPath } from "node:url"
import path from "node:path"

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const GEN_DIR = path.join(__dirname, ".generated")
const OUT_PATH = path.join(GEN_DIR, "classification.generated.json")

const { functionTable, noDestructorClasses, refTargetClasses } = JSON.parse(
    readFileSync(path.join(GEN_DIR, "function-ownership.json"), "utf8")
)
const callSites = JSON.parse(readFileSync(path.join(GEN_DIR, "call-sites.json"), "utf8"))

const NEVER_OWNED = new Set(["STATIC_ALIAS", "INTERNAL_REF", "ALIASES_THIS"])
const noDestructorSet = new Set(noDestructorClasses)
const refTargetSet = new Set(refTargetClasses)

function pickCategory(receiverClassName, methodName) {
    const entry = functionTable[`${receiverClassName}.${methodName}`]
    if (!entry) return undefined
    // Confirmed zero cross-arity disagreement for this IDL snapshot (see parse-glue.mjs's
    // `inconsistentCategories` check) -- safe to read any one arity as representative.
    return Object.values(entry.arities)[0]?.category
}

function main() {
    // className -> { safeEvidence: [...], disqualifyingEvidence: [...] }
    const perClass = new Map()
    const touch = className => {
        if (!perClass.has(className)) perClass.set(className, { safeEvidence: [], disqualifyingEvidence: [] })
        return perClass.get(className)
    }

    for (const c of callSites.constructs) {
        touch(c.className).disqualifyingEvidence.push({ kind: "construct", ...c })
    }

    for (const call of callSites.methodCalls) {
        if (!call.producedClassName) continue // primitive/void/enum return, no cache entry produced
        const bucket = touch(call.producedClassName)
        const category = pickCategory(call.receiverClassName, call.methodName)
        const glueLine = functionTable[`${call.receiverClassName}.${call.methodName}`]?.arities
        const lineInfo = glueLine ? Object.values(glueLine)[0]?.line : undefined

        if (NEVER_OWNED.has(category) && refTargetSet.has(call.producedClassName)) {
            // A RefTarget's INTERNAL_REF-shaped return can be the sole live reference to a
            // C++-refcounted heap object -- the glue.cpp text can't distinguish that from an
            // ordinary non-owned struct-member accessor. Proving this needs the AddRef/Release
            // call graph, not just the return statement's shape, so it does NOT count as safe.
            bucket.disqualifyingEvidence.push({
                kind: "refTarget-internal-ref-unprovable",
                category,
                receiverClassName: call.receiverClassName,
                methodName: call.methodName,
                file: call.file,
                line: call.line,
            })
        } else if (NEVER_OWNED.has(category)) {
            bucket.safeEvidence.push({
                kind: "method-return",
                category,
                receiverClassName: call.receiverClassName,
                methodName: call.methodName,
                glueLine: lineInfo,
                file: call.file,
                line: call.line,
            })
        } else {
            bucket.disqualifyingEvidence.push({
                kind: "method-return",
                category: category ?? "UNKNOWN",
                receiverClassName: call.receiverClassName,
                methodName: call.methodName,
                file: call.file,
                line: call.line,
            })
        }
    }

    for (const c of callSites.castObjectAliases) {
        touch(c.producedClassName).safeEvidence.push({ kind: "castObject-alias", ...c })
    }

    for (const c of callSites.wrapPointerAliases) {
        const destroyedLocally = c.boundName
            ? callSites.destroyCalls.some(d => d.file === c.file && d.argText === c.boundName)
            : false
        if (destroyedLocally) {
            touch(c.producedClassName).disqualifyingEvidence.push({
                kind: "wrapPointer-alias-but-destroyed",
                ...c,
            })
        } else {
            touch(c.producedClassName).safeEvidence.push({ kind: "wrapPointer-alias", ...c })
        }
    }

    // NO_DESTRUCTOR classes are safe unconditionally, independent of call sites -- but only report
    // them if fission/src actually touches them (no point classifying a class nothing ever uses).
    const touchedClasses = new Set(perClass.keys())
    for (const className of noDestructorSet) {
        if (!touchedClasses.has(className)) continue
        touch(className).safeEvidence.push({ kind: "no-destructor-binding" })
    }

    const safe = {}
    const disqualified = {}
    for (const [className, { safeEvidence, disqualifyingEvidence }] of perClass) {
        if (disqualifyingEvidence.length === 0 && safeEvidence.length > 0) {
            safe[className] = safeEvidence
        } else {
            disqualified[className] = { safeEvidence, disqualifyingEvidence }
        }
    }

    writeFileSync(OUT_PATH, JSON.stringify({ safe, disqualified }, null, 2))

    console.log(`${Object.keys(safe).length} classes provably safe (every contributing call site is non-owned)`)
    console.log(`${Object.keys(disqualified).length} classes have at least one disqualifying (owned/unknown) call site`)
    console.log(`Wrote ${OUT_PATH}`)
}

main()
