#!/usr/bin/env node
// Parses generated glue.cpp (see generate-glue.sh) into a per-function return-ownership table.
// Ground truth, not attribution: every category below is decided by matching the literal C++
// pattern webidl_binder.py emits for that binding, the same patterns confirmed by hand in
// docs/JOLT_FUNCTIONS_OWNERSHIP_INVARIANTS.md (branch branp/291/static-alias-doc-corrections):
//
//   COPY          - `return new T(...)` (a real heap allocation)
//   STATIC_ALIAS  - `static (thread_local )?T temp; return (temp = ..., &temp);` (function-local
//                   scratch, shared across every call to that exact bound function)
//   ALIASES_THIS  - `return &(*self <op>= ...);` (in-place op returning the receiver itself)
//   INTERNAL_REF  - `return self->member;` / `return self->Getter();` / `return &self->member;`
//                   (a reference into state owned by someone else, no allocation at all)
//   NONE          - primitive/void return, nothing to own
//   UNKNOWN       - didn't match any known shape; flagged for manual review, never guessed
//
// Also records, per class, whether an `emscripten_bind_<Class>___destroy___0` binding exists at
// all -- if it doesn't (the `[NoDelete]` case, e.g. `Body`), `JOLT.destroy()` throws
// unconditionally for every instance of that class, a mechanical fact independent of any specific
// call site's ownership story.
//
// And, for `void`-returning setters, a best-effort CONSUMED heuristic: `self->member = arg;`
// (raw pointer stored) vs `self->member = *arg;` (dereferenced, i.e. a clone) -- the same
// distinction the classification file's CONSUMED entries (ObjectLayerPairFilterTable, etc.) are
// built on.

import { readFileSync, writeFileSync, mkdirSync } from "node:fs"
import { fileURLToPath } from "node:url"
import path from "node:path"

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const GLUE_PATH = path.join(__dirname, ".generated/glue.cpp")
const IDL_PATH = path.join(__dirname, "../../../jolt/JoltJS.idl")
const OUT_PATH = path.join(__dirname, ".generated/function-ownership.json")

function loadInterfaceNames(idlText) {
    const withoutComments = idlText.replace(/\/\/.*$/gm, "")
    const names = new Set()
    const re = /\binterface\s+(\w+)/g
    let m
    while ((m = re.exec(withoutComments))) names.add(m[1])
    return names
}

// Longest-prefix match against the known interface-name set, since some interface names
// themselves contain underscores (e.g. `BodyInterface_AddState`), so a naive split on the first
// underscore would misparse class vs method.
function splitSymbol(symbol, classNames) {
    const candidates = [...classNames].filter(name => symbol.startsWith(name + "_")).sort((a, b) => b.length - a.length)
    for (const className of candidates) {
        const rest = symbol.slice(className.length + 1)
        const arityMatch = rest.match(/^(.*)_(\d+)$/)
        if (arityMatch) {
            return { className, methodName: arityMatch[1], arity: Number(arityMatch[2]) }
        }
    }
    return null
}

function extractFunctions(glueText) {
    const lines = glueText.split("\n")
    const sigRe = /^(?<returnType>.+?)\s+EMSCRIPTEN_KEEPALIVE\s+emscripten_bind_(?<symbol>\w+)\((?<params>[^)]*)\)\s*\{\s*$/
    const functions = []

    for (let i = 0; i < lines.length; i++) {
        const m = sigRe.exec(lines[i])
        if (!m) continue

        let depth = 1
        const bodyLines = []
        let j = i + 1
        for (; j < lines.length && depth > 0; j++) {
            const line = lines[j]
            for (const ch of line) {
                if (ch === "{") depth++
                else if (ch === "}") depth--
            }
            if (depth > 0) bodyLines.push(line)
        }

        functions.push({
            returnType: m.groups.returnType.trim(),
            symbol: m.groups.symbol,
            params: m.groups.params.trim(),
            body: bodyLines.join("\n").trim(),
            line: i + 1,
        })
        i = j - 1
    }

    return functions
}

function classifyReturn(fn, className) {
    if (fn.body === "delete self;") return { category: "DESTRUCTOR" }
    if (fn.methodName === className) return { category: "COPY", evidence: "constructor" }

    const isVoid = fn.returnType === "void"
    const isPrimitive = !isVoid && !/[*&]/.test(fn.returnType)

    if (isVoid || isPrimitive) {
        const result = { category: "NONE" }
        // Best-effort CONSUMED heuristic for simple single-statement setters.
        const assign = fn.body.match(/^self->(\w+)\s*=\s*(\*?)(\w+);$/)
        if (isVoid && assign) {
            const [, member, deref, argName] = assign
            const argType = (fn.params.match(new RegExp(`(?:const\\s+)?(\\w+)\\*\\s*${argName}\\b`)) || [])[1]
            if (argType) {
                result.setterMember = member
                result.setterArgType = argType
                result.setterOwnership = deref ? "CLONED" : "CONSUMED"
            }
        }
        return result
    }

    if (/return new\s+\w+/.test(fn.body)) return { category: "COPY" }
    if (/return &\(\*self\b/.test(fn.body)) return { category: "ALIASES_THIS" }
    if (/static\s+(thread_local\s+)?[\w:<>]+\s+temp;[\s\S]*return\s*\(\s*temp\s*=[\s\S]*,\s*&temp\s*\);/.test(fn.body)) {
        return { category: "STATIC_ALIAS" }
    }
    if (/return\s+&?self->[\w]+(\(.*\))?;/.test(fn.body)) return { category: "INTERNAL_REF" }

    return { category: "UNKNOWN" }
}

function main() {
    const glueText = readFileSync(GLUE_PATH, "utf8")
    const idlText = readFileSync(IDL_PATH, "utf8")
    const classNames = loadInterfaceNames(idlText)

    const rawFunctions = extractFunctions(glueText)
    const functionTable = {}
    const classesWithDestructor = new Set()
    const unparsedSymbols = []

    for (const fn of rawFunctions) {
        const split = splitSymbol(fn.symbol, classNames)
        if (!split) {
            unparsedSymbols.push(fn.symbol)
            continue
        }
        const { className, methodName, arity } = split
        const classified = classifyReturn({ ...fn, methodName }, className)

        if (classified.category === "DESTRUCTOR") {
            classesWithDestructor.add(className)
            continue
        }

        const key = `${className}.${methodName}`
        functionTable[key] ??= { className, methodName, arities: {} }
        functionTable[key].arities[arity] = {
            ...classified,
            returnType: fn.returnType,
            line: fn.line,
        }
    }

    // Flag any (Class, Method) whose category disagrees across overloaded arities -- should never
    // happen for these bindings in practice, but worth knowing about rather than silently picking one.
    const inconsistent = []
    for (const [key, entry] of Object.entries(functionTable)) {
        const categories = new Set(Object.values(entry.arities).map(a => a.category))
        if (categories.size > 1) inconsistent.push({ key, categories: [...categories] })
    }

    const allClassNames = [...classNames]
    const noDestructorClasses = allClassNames.filter(name => !classesWithDestructor.has(name))

    // Structural RefTarget detection (same heuristic as the memory-audit branch's plan: a class
    // exposing all three of AddRef/Release/GetRefCount is refcounted). This matters because a
    // RefTarget's `return self->Method();`/`return self->member;` (INTERNAL_REF-shaped in
    // glue.cpp) can be the sole live reference to a C++-refcounted heap object -- proving that
    // reference is safe to leave undestroyed needs the AddRef/Release call graph, not just the
    // return statement's shape, which is indistinguishable from an ordinary non-owned struct-
    // member accessor at the glue.cpp text level. Non-RefTarget INTERNAL_REF returns don't have
    // this ambiguity: there's no refcount to have gotten wrong.
    const refTargetClasses = allClassNames.filter(name =>
        ["AddRef", "Release", "GetRefCount"].every(method => functionTable[`${name}.${method}`])
    )

    mkdirSync(path.dirname(OUT_PATH), { recursive: true })
    writeFileSync(
        OUT_PATH,
        JSON.stringify(
            {
                generatedFrom: "jolt/JoltJS.idl via webidl_binder.py",
                functionCount: rawFunctions.length,
                functionTable,
                noDestructorClasses,
                refTargetClasses,
                unparsedSymbols,
                inconsistentCategories: inconsistent,
            },
            null,
            2
        )
    )

    console.log(`Parsed ${rawFunctions.length} bound functions -> ${Object.keys(functionTable).length} (class, method) entries`)
    console.log(`${noDestructorClasses.length} classes have no generated destructor (JOLT.destroy() throws unconditionally)`)
    console.log(`${refTargetClasses.length} classes are RefTarget-derived (AddRef/Release/GetRefCount) -- INTERNAL_REF alone doesn't prove these safe`)
    console.log(`${unparsedSymbols.length} symbols failed to parse (see unparsedSymbols in output)`)
    if (inconsistent.length) console.log(`WARNING: ${inconsistent.length} (class, method) pairs disagree across arities`, inconsistent)
    console.log(`Wrote ${OUT_PATH}`)
}

main()
