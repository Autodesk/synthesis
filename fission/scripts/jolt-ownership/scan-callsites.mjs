#!/usr/bin/env node
// Walks fission/src (production code only, not src/test/**) with the TypeScript compiler API and
// records every place production code touches a tracked Jolt class's cache:
//   - `new JOLT.X(...)` constructions (always COPY, caller-owned)
//   - `obj.Method(...)` calls, resolved via the checker to (receiver's static class, method name,
//     and the call expression's own resolved return type -- the two can differ, e.g.
//     `VehicleConstraint.GetWheel()` is declared to return `Wheel`, cached under `Wheel`,
//     regardless of the receiver's own class)
//   - `JOLT.castObject(_, JOLT.X)` / `JOLT.wrapPointer(_, JOLT.X)` -- both are pure
//     address-reinterpretation in the binder (confirmed from the built glue:
//     `Module.castObject=function(a,b){return k(a.GDa,b)}`, same `k()` cache-getter `wrapPointer`
//     uses), never an allocation, regardless of what class table entry says
//   - `JOLT.destroy(...)` calls, recorded for evidence/cross-reference only
//
// Heuristic scope: only recognizes the Jolt module accessed via the literal identifier `JOLT`
// (verified by grep against this codebase: every call site in fission/src spells it that way,
// via `import JOLT from "@/util/loading/JoltSyncLoader"`). Does not attempt to trace re-exports,
// aliasing, or destructuring of the module.
import ts from "typescript"
import { readFileSync, writeFileSync } from "node:fs"
import { fileURLToPath } from "node:url"
import path from "node:path"

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const FISSION_ROOT = path.join(__dirname, "../..")
const TSCONFIG_PATH = path.join(FISSION_ROOT, "tsconfig.json")
const FUNCTION_TABLE_PATH = path.join(__dirname, ".generated/function-ownership.json")
const OUT_PATH = path.join(__dirname, ".generated/call-sites.json")

const { functionTable } = JSON.parse(readFileSync(FUNCTION_TABLE_PATH, "utf8"))
const knownClassNames = new Set(Object.values(functionTable).map(e => e.className))

function isTestPath(fileName) {
    return fileName.includes("/src/test/")
}

function loadProgram() {
    const configFile = ts.readConfigFile(TSCONFIG_PATH, ts.sys.readFile)
    const parsed = ts.parseJsonConfigFileContent(configFile.config, ts.sys, FISSION_ROOT)
    const rootNames = parsed.fileNames.filter(f => !isTestPath(f))
    return ts.createProgram({ rootNames, options: parsed.options })
}

function lineOf(sourceFile, node) {
    return sourceFile.getLineAndCharacterOfPosition(node.getStart()).line + 1
}

function relFile(sourceFile) {
    return path.relative(FISSION_ROOT, sourceFile.fileName)
}

// Name alone isn't enough -- fission has its own `PhysicsSystem` class wrapping Jolt's, and
// three.js has `Plane`/`Vector2`, both colliding with real Jolt interface names. Require the
// resolved type's declaration to actually live in the jolt-physics package's own .d.ts.
function classNameOfType(type) {
    const symbol = type?.getSymbol?.() ?? type?.symbol
    const name = symbol?.getName?.() ?? symbol?.name
    if (!name || !knownClassNames.has(name)) return undefined
    const declarations = symbol?.getDeclarations?.() ?? []
    const fromJolt = declarations.some(d => d.getSourceFile().fileName.includes("jolt-physics"))
    return fromJolt ? name : undefined
}

function isJoltIdentifier(node) {
    return ts.isIdentifier(node) && node.text === "JOLT"
}

function main() {
    const program = loadProgram()
    const checker = program.getTypeChecker()

    const constructs = []
    const methodCalls = []
    const castObjectAliases = []
    const wrapPointerAliases = []
    const destroyCalls = []
    const unresolvedCalls = []

    for (const sourceFile of program.getSourceFiles()) {
        if (sourceFile.isDeclarationFile) continue
        if (isTestPath(sourceFile.fileName)) continue
        if (!sourceFile.fileName.startsWith(FISSION_ROOT)) continue

        const visit = node => {
            if (ts.isNewExpression(node) && ts.isPropertyAccessExpression(node.expression)) {
                const { expression: obj, name } = node.expression
                if (isJoltIdentifier(obj) && knownClassNames.has(name.text)) {
                    constructs.push({ className: name.text, file: relFile(sourceFile), line: lineOf(sourceFile, node) })
                }
            }

            if (ts.isCallExpression(node) && ts.isPropertyAccessExpression(node.expression)) {
                const { expression: receiver, name: methodName } = node.expression

                if (isJoltIdentifier(receiver)) {
                    // JOLT.castObject(_, JOLT.X) / JOLT.wrapPointer(_, JOLT.X) / JOLT.destroy(x)
                    if (methodName.text === "castObject" || methodName.text === "wrapPointer") {
                        const classArg = node.arguments[1]
                        if (classArg && ts.isPropertyAccessExpression(classArg) && isJoltIdentifier(classArg.expression)) {
                            const producedClassName = classArg.name.text
                            const bucket = methodName.text === "castObject" ? castObjectAliases : wrapPointerAliases
                            let parent = node.parent
                            while (ts.isAsExpression(parent) || ts.isParenthesizedExpression(parent) || ts.isNonNullExpression(parent)) {
                                parent = parent.parent
                            }
                            const boundName =
                                ts.isVariableDeclaration(parent) && ts.isIdentifier(parent.name) ? parent.name.text : null
                            bucket.push({ producedClassName, boundName, file: relFile(sourceFile), line: lineOf(sourceFile, node) })
                        }
                    } else if (methodName.text === "destroy") {
                        destroyCalls.push({ argText: node.arguments[0]?.getText() ?? "", file: relFile(sourceFile), line: lineOf(sourceFile, node) })
                    }
                } else {
                    // obj.Method(...) -- resolve receiver's static class and the call's own return type.
                    const receiverType = checker.getTypeAtLocation(receiver)
                    const receiverClassName = classNameOfType(receiverType)
                    if (receiverClassName) {
                        const callType = checker.getTypeAtLocation(node)
                        const producedClassName = classNameOfType(callType)
                        const entry = functionTable[`${receiverClassName}.${methodName.text}`]
                        if (entry) {
                            methodCalls.push({
                                receiverClassName,
                                methodName: methodName.text,
                                producedClassName: producedClassName ?? null,
                                file: relFile(sourceFile),
                                line: lineOf(sourceFile, node),
                            })
                        } else if (producedClassName) {
                            unresolvedCalls.push({
                                receiverClassName,
                                methodName: methodName.text,
                                producedClassName,
                                file: relFile(sourceFile),
                                line: lineOf(sourceFile, node),
                            })
                        }
                    }
                }
            }

            ts.forEachChild(node, visit)
        }

        visit(sourceFile)
    }

    writeFileSync(
        OUT_PATH,
        JSON.stringify({ constructs, methodCalls, castObjectAliases, wrapPointerAliases, destroyCalls, unresolvedCalls }, null, 2)
    )

    console.log(`constructs: ${constructs.length}`)
    console.log(`methodCalls (resolved against function table): ${methodCalls.length}`)
    console.log(`castObjectAliases: ${castObjectAliases.length}`)
    console.log(`wrapPointerAliases: ${wrapPointerAliases.length}`)
    console.log(`destroyCalls: ${destroyCalls.length}`)
    console.log(`unresolvedCalls (returned a tracked class but no function-table entry -- investigate): ${unresolvedCalls.length}`)
    console.log(`Wrote ${OUT_PATH}`)
}

main()
