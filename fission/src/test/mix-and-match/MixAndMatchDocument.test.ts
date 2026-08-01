import { describe, expect, test } from "vitest"
import {
    hasMixAndMatchSession,
    parseSession,
    readSessionFromAssembly,
    serializeSession,
    writeSessionToAssembly,
} from "@/mix-and-match/MixAndMatchDocument"
import { createEmptySession, type MixAndMatchSession } from "@/mix-and-match/MixAndMatchTypes"
import { mirabuf } from "@/proto/mirabuf"

const IDENTITY = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]
const SHIFTED = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 2, 0, 3, 1]

function sampleSession(): MixAndMatchSession {
    const session = createEmptySession()
    session.components.push({ id: "c1", libraryPartRef: "hash-frame" }, { id: "c2", libraryPartRef: "hash-pod" })
    session.timeline.push(
        { type: "spawn", componentId: "c1", libraryPartRef: "hash-frame", transform: IDENTITY },
        { type: "spawn", componentId: "c2", libraryPartRef: "hash-pod", transform: SHIFTED },
        { type: "move", componentId: "c2", transform: SHIFTED },
        { type: "weld", componentA: "c1", componentB: "c2", relativeOffset: SHIFTED },
        { type: "resize", componentId: "c1", sizeOption: "28x28" },
        { type: "delete", componentId: "c2" }
    )

    return session
}

function emptyAssembly(): mirabuf.Assembly {
    return mirabuf.Assembly.create({ data: mirabuf.AssemblyData.create({ parts: mirabuf.Parts.create({}) }) })
}

describe("Mix and Match Document", () => {
    test("Round Trips Every Entry Type", () => {
        const original = sampleSession()
        const parsed = parseSession(serializeSession(original))

        expect(parsed).toEqual(original)
    })

    test("Rejects Unknown Version", () => {
        expect(parseSession(JSON.stringify({ version: 99, components: [], timeline: [] }))).toBeUndefined()
    })

    test("Rejects Malformed JSON", () => {
        expect(parseSession("{ not json")).toBeUndefined()
    })

    test("Drops Malformed Entries Instead Of Failing", () => {
        const parsed = parseSession(
            JSON.stringify({
                version: 1,
                components: [{ id: "c1", libraryPartRef: "hash" }, { id: 7 }],
                timeline: [
                    { type: "spawn", componentId: "c1", libraryPartRef: "hash", transform: IDENTITY },
                    { type: "spawn", componentId: "c2", libraryPartRef: "hash", transform: [1, 2, 3] },
                    { type: "not-a-real-type", componentId: "c1" },
                    { type: "delete", componentId: "c1" },
                ],
            })
        )

        expect(parsed?.components).toHaveLength(1)
        expect(parsed?.timeline.map(x => x.type)).toEqual(["spawn", "delete"])
    })

    test("Writes And Reads Back Through Part User Data", () => {
        const assembly = emptyAssembly()
        expect(hasMixAndMatchSession(assembly)).toBe(false)

        const session = sampleSession()
        expect(writeSessionToAssembly(assembly, session)).toBe(true)
        expect(hasMixAndMatchSession(assembly)).toBe(true)
        expect(readSessionFromAssembly(assembly)).toEqual(session)
    })

    test("Survives A Protobuf Encode And Decode", () => {
        const assembly = emptyAssembly()
        const session = sampleSession()
        writeSessionToAssembly(assembly, session)

        const decoded = mirabuf.Assembly.decode(mirabuf.Assembly.encode(assembly).finish())

        expect(readSessionFromAssembly(decoded)).toEqual(session)
    })

    test("Reports No Session On An Ordinary Assembly", () => {
        const assembly = emptyAssembly()
        assembly.data!.parts!.userData = mirabuf.UserData.create({ data: { urdfImport: "true" } })

        expect(hasMixAndMatchSession(assembly)).toBe(false)
        expect(readSessionFromAssembly(assembly)).toBeUndefined()
    })
})
