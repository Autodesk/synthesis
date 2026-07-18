// Parses jolt/JoltJS.idl + jolt/JoltJS-DebugRenderer.idl into a per-method ownership table.
// Run with: bun run jolt-audit:codegen (see fission/package.json)
import * as fs from "node:fs"
import * as path from "node:path"

const REPO_ROOT = path.resolve(import.meta.dirname, "..", "..", "..", "..", "..")
const IDL_FILES = [
    path.join(REPO_ROOT, "jolt", "JoltJS.idl"),
    path.join(REPO_ROOT, "jolt", "JoltJS-DebugRenderer.idl"),
]
const OUTPUT_FILE = path.resolve(import.meta.dirname, "..", "ownership-table.generated.json")

const PRIMITIVE_TYPES = new Set([
    "void",
    "boolean",
    "long",
    "short",
    "byte",
    "octet",
    "float",
    "double",
    "DOMString",
    "any",
    "unsigned long",
    "unsigned short",
    "unsigned long long",
])

const REFTARGET_MARKER_METHODS = ["AddRef", "Release", "GetRefCount"]
const HANDOFF_NAME_PATTERN = /^(push_back|Add\w+|Set\w+)$/

type ExtAttrs = {
    isValue: boolean
    isRef: boolean
    isConst: boolean
    isNoDelete: boolean
    jsImplementation: string | null
    prefix: string | null
    operator: string | null
    bindTo: string | null
}

type ArgInfo = {
    type: string
    isArray: boolean
    optional: boolean
    ownership: "COPIED" | "CLONED" | "HANDLE_NO_TAG"
    isHandoffCandidate: boolean
    extAttrs: ExtAttrs
}

type MethodInfo = {
    name: string
    isStatic: boolean
    isConstructor: boolean
    returnType: string
    returnIsArray: boolean
    returnOwnership: "NONE" | "COPY" | "INTERNAL_REF"
    args: ArgInfo[]
    extAttrs: ExtAttrs
}

type AttributeInfo = {
    name: string
    type: string
    isArray: boolean
    isStatic: boolean
    isReadonly: boolean
    ownership: "NONE" | "COPY" | "INTERNAL_REF"
    extAttrs: ExtAttrs
}

type ClassInfo = {
    name: string
    parent: string | null
    extAttrs: ExtAttrs
    ownMethods: MethodInfo[]
    ownAttributes: AttributeInfo[]
}

type OutputRow = {
    className: string
    declaredIn: string
    methodName: string
    isStatic: boolean
    isConstructor: boolean
    isNoDelete: boolean
    isJSImplementation: boolean
    isRefTarget: boolean
    args: ArgInfo[]
    returnType: string
    returnOwnership: string
}

function stripComments(source: string): string {
    return source
        .split("\n")
        .map(line => {
            const idx = line.indexOf("//")
            return idx === -1 ? line : line.slice(0, idx)
        })
        .join("\n")
}

// Depth-aware split: cuts a new top-level statement whenever `;` is seen at brace depth 0.
function splitTopLevelStatements(source: string): string[] {
    const statements: string[] = []
    let depth = 0
    let current = ""
    for (const ch of source) {
        if (ch === "{") depth++
        if (ch === "}") depth--
        current += ch
        if (ch === ";" && depth === 0) {
            const trimmed = current.trim().replace(/;$/, "").trim()
            if (trimmed.length > 0) statements.push(trimmed)
            current = ""
        }
    }
    const trailing = current.trim().replace(/;$/, "").trim()
    if (trailing.length > 0) statements.push(trailing)
    return statements
}

function splitTopLevelCommas(source: string): string[] {
    if (source.trim().length === 0) return []
    const parts: string[] = []
    let depth = 0
    let current = ""
    for (const ch of source) {
        if (ch === "[" || ch === "(") depth++
        if (ch === "]" || ch === ")") depth--
        if (ch === "," && depth === 0) {
            parts.push(current.trim())
            current = ""
        } else {
            current += ch
        }
    }
    if (current.trim().length > 0) parts.push(current.trim())
    return parts
}

function parseExtAttrs(raw: string | undefined): ExtAttrs {
    const attrs: ExtAttrs = {
        isValue: false,
        isRef: false,
        isConst: false,
        isNoDelete: false,
        jsImplementation: null,
        prefix: null,
        operator: null,
        bindTo: null,
    }
    if (!raw) return attrs
    // raw may contain multiple bracket groups concatenated, e.g. "[Const] [Ref]" — normalize to one list.
    const tokens = raw.match(/\[[^\]]*\]/g) ?? []
    for (const bracket of tokens) {
        const inner = bracket.slice(1, -1)
        for (const rawPart of inner.split(",")) {
            const part = rawPart.trim()
            if (part === "Value") attrs.isValue = true
            else if (part === "Ref") attrs.isRef = true
            else if (part === "Const") attrs.isConst = true
            else if (part === "NoDelete") attrs.isNoDelete = true
            else if (part.startsWith("JSImplementation")) {
                const m = part.match(/JSImplementation\s*=\s*"([^"]+)"/)
                attrs.jsImplementation = m ? m[1] : ""
            } else if (part.startsWith("Prefix")) {
                const m = part.match(/Prefix\s*=\s*"([^"]+)"/)
                attrs.prefix = m ? m[1] : ""
            } else if (part.startsWith("Operator")) {
                const m = part.match(/Operator\s*=\s*"([^"]+)"/)
                attrs.operator = m ? m[1] : ""
            } else if (part.startsWith("BindTo")) {
                const m = part.match(/BindTo\s*=\s*"([^"]+)"/)
                attrs.bindTo = m ? m[1] : ""
            }
            // unrecognized tokens (e.g. static/readonly leaking in) are ignored here — handled by caller regex
        }
    }
    return attrs
}

const LEADING_EXT_ATTRS = /^((?:\[[^\]]*\]\s*)+)?/
const TYPE_TOKEN = "(?:unsigned\\s+long\\s+long|unsigned\\s+long|unsigned\\s+short|[A-Za-z_]\\w*)"

function parseArg(raw: string): ArgInfo | null {
    const m = raw.match(
        new RegExp(`^${LEADING_EXT_ATTRS.source}(optional\\s+)?(${TYPE_TOKEN})(\\[\\])?\\s+(\\w+)$`),
    )
    if (!m) return null
    const [, extAttrsRaw, optionalRaw, type, arrayRaw] = m
    const extAttrs = parseExtAttrs(extAttrsRaw)
    const normalizedType = type.replace(/\s+/g, " ")
    const isPrimitive = PRIMITIVE_TYPES.has(normalizedType)
    let ownership: ArgInfo["ownership"]
    if (isPrimitive) ownership = "COPIED"
    else if (extAttrs.isRef || extAttrs.isValue) ownership = "CLONED"
    else ownership = "HANDLE_NO_TAG"
    return {
        type: normalizedType,
        isArray: Boolean(arrayRaw),
        optional: Boolean(optionalRaw),
        ownership,
        // isHandoffCandidate is finalized in a second pass once isRefTarget is known per-class.
        isHandoffCandidate: false,
        extAttrs,
    }
}

function parseMember(raw: string, className: string): { method?: MethodInfo; attribute?: AttributeInfo } {
    const attrMatch = raw.match(
        new RegExp(
            `^${LEADING_EXT_ATTRS.source}(static\\s+)?(readonly\\s+)?attribute\\s+(${TYPE_TOKEN})(\\[\\])?\\s+(\\w+)$`,
        ),
    )
    if (attrMatch) {
        const [, extAttrsRaw, staticRaw, readonlyRaw, type, arrayRaw, name] = attrMatch
        const extAttrs = parseExtAttrs(extAttrsRaw)
        const normalizedType = type.replace(/\s+/g, " ")
        const isPrimitive = PRIMITIVE_TYPES.has(normalizedType)
        const ownership: AttributeInfo["ownership"] = isPrimitive ? "NONE" : extAttrs.isValue ? "COPY" : "INTERNAL_REF"
        return {
            attribute: {
                name,
                type: normalizedType,
                isArray: Boolean(arrayRaw),
                isStatic: Boolean(staticRaw),
                isReadonly: Boolean(readonlyRaw),
                ownership,
                extAttrs,
            },
        }
    }

    const methodMatch = raw.match(
        new RegExp(`^${LEADING_EXT_ATTRS.source}(static\\s+)?(${TYPE_TOKEN})(\\[\\])?\\s+(\\w+)\\((.*)\\)$`, "s"),
    )
    if (methodMatch) {
        const [, extAttrsRaw, staticRaw, returnType, returnArrayRaw, name, argsRaw] = methodMatch
        const extAttrs = parseExtAttrs(extAttrsRaw)
        const normalizedReturnType = returnType.replace(/\s+/g, " ")
        const isPrimitiveReturn = normalizedReturnType === "void" || PRIMITIVE_TYPES.has(normalizedReturnType)
        const returnOwnership: MethodInfo["returnOwnership"] = isPrimitiveReturn
            ? "NONE"
            : extAttrs.isValue
              ? "COPY"
              : "INTERNAL_REF"
        const args = splitTopLevelCommas(argsRaw)
            .map(parseArg)
            .filter((a): a is ArgInfo => a !== null)
        return {
            method: {
                name,
                isStatic: Boolean(staticRaw),
                isConstructor: name === className,
                returnType: normalizedReturnType,
                returnIsArray: Boolean(returnArrayRaw),
                returnOwnership,
                args,
                extAttrs,
            },
        }
    }

    return {}
}

function parseIdl(source: string): { classes: Map<string, ClassInfo>; implementsEdges: Map<string, string>; enumNames: Set<string> } {
    const classes = new Map<string, ClassInfo>()
    const implementsEdges = new Map<string, string>()
    const enumNames = new Set<string>()

    for (const statement of splitTopLevelStatements(stripComments(source))) {
        const implementsMatch = statement.match(/^(\w+)\s+implements\s+(\w+)$/)
        if (implementsMatch) {
            implementsEdges.set(implementsMatch[1], implementsMatch[2])
            continue
        }

        const enumMatch = statement.match(new RegExp(`^${LEADING_EXT_ATTRS.source}enum\\s+(\\w+)\\s*\\{`))
        if (enumMatch) {
            enumNames.add(enumMatch[2])
            continue
        }

        const interfaceMatch = statement.match(
            new RegExp(`^${LEADING_EXT_ATTRS.source}interface\\s+(\\w+)(?:\\s*:\\s*(\\w+))?\\s*\\{([\\s\\S]*)\\}$`),
        )
        if (interfaceMatch) {
            const [, extAttrsRaw, name, colonParent, body] = interfaceMatch
            const extAttrs = parseExtAttrs(extAttrsRaw)
            const ownMethods: MethodInfo[] = []
            const ownAttributes: AttributeInfo[] = []
            for (const memberRaw of splitTopLevelStatements(body)) {
                const { method, attribute } = parseMember(memberRaw, name)
                if (method) ownMethods.push(method)
                if (attribute) ownAttributes.push(attribute)
            }
            classes.set(name, {
                name,
                parent: colonParent ?? null,
                extAttrs,
                ownMethods,
                ownAttributes,
            })
            continue
        }

        console.warn(`[jolt-audit codegen] unrecognized top-level statement, skipped:\n  ${statement.slice(0, 120)}`)
    }

    // `X implements Y;` can appear before or after both interfaces are declared — apply as parent
    // link only when the class doesn't already have a `: Parent` link from the interface header.
    for (const [child, parent] of implementsEdges) {
        const info = classes.get(child)
        if (info && info.parent === null) info.parent = parent
    }

    return { classes, implementsEdges, enumNames }
}

function getAncestorChain(classes: Map<string, ClassInfo>, className: string): ClassInfo[] {
    const chain: ClassInfo[] = []
    let current = classes.get(className)
    const seen = new Set<string>()
    while (current && !seen.has(current.name)) {
        chain.push(current)
        seen.add(current.name)
        current = current.parent ? classes.get(current.parent) : undefined
    }
    return chain
}

function main() {
    const classes = new Map<string, ClassInfo>()
    const enumNames = new Set<string>()
    for (const file of IDL_FILES) {
        const source = fs.readFileSync(file, "utf-8")
        const parsed = parseIdl(source)
        for (const [name, info] of parsed.classes) classes.set(name, info)
        for (const name of parsed.enumNames) enumNames.add(name)
    }

    // isRefTarget: AddRef + Release + GetRefCount present anywhere in the ancestor chain.
    const isRefTargetCache = new Map<string, boolean>()
    function isRefTarget(className: string): boolean {
        if (isRefTargetCache.has(className)) return isRefTargetCache.get(className)!
        const chain = getAncestorChain(classes, className)
        const allMethodNames = new Set(chain.flatMap(c => c.ownMethods.map(m => m.name)))
        const result = REFTARGET_MARKER_METHODS.every(m => allMethodNames.has(m))
        isRefTargetCache.set(className, result)
        return result
    }

    const rows: OutputRow[] = []
    const attributeHandoffs: { className: string; attributeName: string; type: string }[] = []
    const factoryCoverageReport: { className: string; isRefTarget: boolean; isNoDelete: boolean; isJSImplementation: boolean }[] = []

    for (const className of classes.keys()) {
        const chain = getAncestorChain(classes, className)
        const noDelete = chain.some(c => c.extAttrs.isNoDelete)
        const jsImpl = chain.some(c => c.extAttrs.jsImplementation !== null)
        const refTarget = isRefTarget(className)

        factoryCoverageReport.push({ className, isRefTarget: refTarget, isNoDelete: noDelete, isJSImplementation: jsImpl })

        for (const ancestor of chain) {
            for (const method of ancestor.ownMethods) {
                const args = method.args.map(arg => {
                    const argIsRefTarget = classes.has(arg.type) && isRefTarget(arg.type)
                    const nameLooksLikeHandoff = HANDOFF_NAME_PATTERN.test(method.name) || method.isConstructor
                    const isHandoffCandidate = argIsRefTarget && arg.ownership === "HANDLE_NO_TAG" && nameLooksLikeHandoff
                    return { ...arg, isHandoffCandidate }
                })
                rows.push({
                    className,
                    declaredIn: ancestor.name,
                    methodName: method.name,
                    isStatic: method.isStatic,
                    isConstructor: method.isConstructor && ancestor.name === className,
                    isNoDelete: noDelete,
                    isJSImplementation: jsImpl,
                    isRefTarget: refTarget,
                    args,
                    returnType: method.returnType,
                    returnOwnership: method.returnOwnership,
                })
            }
            for (const attribute of ancestor.ownAttributes) {
                // A writable, non-Value, RefTarget-typed attribute is the assignment-form of a
                // handoff (e.g. `bcs.mShape = shape` is the same hazard as `SetShape(shape)`).
                const attrIsRefTarget = classes.has(attribute.type) && isRefTarget(attribute.type)
                if (!attribute.isReadonly && !attribute.extAttrs.isValue && attrIsRefTarget) {
                    attributeHandoffs.push({ className, attributeName: attribute.name, type: attribute.type })
                }
            }
        }
    }

    fs.writeFileSync(OUTPUT_FILE, JSON.stringify({ rows, attributeHandoffs }, null, 2) + "\n")

    const refTargetClasses = factoryCoverageReport.filter(c => c.isRefTarget).map(c => c.className)
    const handoffRows = rows.filter(r => r.args.some(a => a.isHandoffCandidate))
    console.log(`Parsed ${classes.size} classes, ${rows.length} method rows -> ${OUTPUT_FILE}`)
    console.log(`RefTarget classes (${refTargetClasses.length}): ${refTargetClasses.join(", ")}`)
    console.log(`Handoff-candidate method rows (${handoffRows.length}):`)
    for (const row of handoffRows) {
        console.log(`  ${row.className}.${row.methodName}`)
    }
    console.log(`Handoff-candidate attribute assignments (${attributeHandoffs.length}):`)
    for (const attr of attributeHandoffs) {
        console.log(`  ${attr.className}.${attr.attributeName} (${attr.type})`)
    }
}

main()
