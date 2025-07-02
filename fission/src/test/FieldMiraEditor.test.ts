import { describe, expect, test } from "vitest"
import FieldMiraEditor from "../mirabuf/FieldMiraEditor"

function mockParts(): any {
    return { userData: { data: {} } }
}

describe("Basic Field Mira Editor Tests", () => {
    test("writes and reads devtool data", () => {
        const parts = mockParts()
        const editor = new FieldMiraEditor(parts)

        const key = "devtool:scoring_zones"
        const payload = [
            {
                id: "zone-A",
                pose: { position: { x: 0, y: 0, z: 0 }, rotation: { x: 0, y: 0, z: 0, w: 1 } },
                size: { x: 1, y: 1, z: 1 },
            },
        ]

        editor.setUserData(key, payload)
        expect(editor.getUserData(key)).toEqual(payload)
        expect(editor.getAllDevtoolKeys()).toContain(key)

        editor.removeUserData(key)
        expect(editor.getUserData(key)).toBeUndefined()
    })

    test("default state: no keys, getUserData yields undefined", () => {
        const editor = new FieldMiraEditor(mockParts())
        expect(editor.getAllDevtoolKeys()).toEqual([])
        expect(editor.getUserData("devtool:foo")).toBeUndefined()
    })

    test("multiple keys round-trip in order of insertion", () => {
        const editor = new FieldMiraEditor(mockParts())
        editor.setUserData("devtool:a", { v: 1 })
        editor.setUserData("devtool:b", [2, 3])
        expect(editor.getAllDevtoolKeys()).toEqual(["devtool:a", "devtool:b"])
        expect(editor.getUserData("devtool:b")).toEqual([2, 3])
    })

    test("malformed JSON in underlying data is caught and returns undefined", () => {
        const parts = mockParts()
        parts.userData.data["devtool:bad"] = "{ not valid json "
        const editor = new FieldMiraEditor(parts)
        expect(() => editor.getUserData("devtool:bad")).not.toThrow()
        expect(editor.getUserData("devtool:bad")).toBeUndefined()
    })

    test("returned object is a deep clone, not a live reference", () => {
        const editor = new FieldMiraEditor(mockParts())
        const payload = { nested: { x: 1 } }
        editor.setUserData("devtool:test", payload)
        const read = editor.getUserData<{ nested: { x: number } }>("devtool:test")!
        read.nested.x = 42
        expect(editor.getUserData("devtool:test")!.nested.x).toBe(1)
    })

    test("removeUserData only deletes the target key", () => {
        const editor = new FieldMiraEditor(mockParts())
        editor.setUserData("devtool:keep", { a: 1 })
        editor.setUserData("devtool:drop", { b: 2 })
        editor.removeUserData("devtool:drop")
        expect(editor.getAllDevtoolKeys()).toEqual(["devtool:keep"])
    })
})
