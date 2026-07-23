import { assert, describe, expect, test, vi, beforeEach, afterEach } from "vitest"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import { mirabuf } from "@/proto/mirabuf"
import { defaultFieldPreferences, defaultRobotPreferences } from "@/systems/preferences/PreferenceTypes.ts"
import FieldMiraEditor from "../../mirabuf/FieldMiraEditor.ts"
import { getMiraInstance } from "@/test/GetAssets.ts"
import { mockConsole } from "@/test/mocks/Common.ts"
import PhysicsSystem from "@/systems/physics/PhysicsSystem.ts"

function mockParts(): mirabuf.IParts {
    return { userData: { data: {} } }
}

const physicsSystem = new PhysicsSystem()

vi.mock("@/systems/World", () => ({
    default: {
        sceneRenderer: {
            setupMaterial: vi.fn(),
        },
        get physicsSystem() {
            return physicsSystem
        },
    },
}))

describe("Basic Field Mira Editor Tests", () => {
    beforeEach(() => {
        mockConsole()
    })
    afterEach(() => {
        vi.restoreAllMocks()
    })
    test("writes and reads devtool data", () => {
        const parts = mockParts()
        const editor = new FieldMiraEditor(parts)

        const key = "synthesis:robot_preferences"
        const payload = defaultRobotPreferences()

        editor.setUserData(key, payload)
        expect(editor.getUserData(key)).toEqual(payload)
        expect(editor.getSynthesisKeys()).toContain(key)

        editor.removeUserData(key)
        expect(editor.getUserData(key)).toBeUndefined()
    })

    test("default state: no keys, getUserData yields undefined", () => {
        const editor = new FieldMiraEditor(mockParts())
        expect(editor.getSynthesisKeys()).toEqual([])
        expect(editor.getUserData("synthesis:robot_preferences")).toBeUndefined()
    })

    test("malformed JSON in underlying data is caught and returns undefined", () => {
        const parts = mockParts()
        if (parts.userData?.data) {
            parts.userData.data["synthesis:robot_preferences"] = "{ not valid json "
        }
        const editor = new FieldMiraEditor(parts)
        expect(() => editor.getUserData("synthesis:robot_preferences")).not.toThrow()
        expect(editor.getUserData("synthesis:robot_preferences")).toBeUndefined()
    })

    test("returned object is a deep clone, not a live reference", () => {
        const editor = new FieldMiraEditor(mockParts())
        const payload = defaultRobotPreferences()
        editor.setUserData("synthesis:robot_preferences", payload)
        const read = editor.getUserData("synthesis:robot_preferences")!

        read.ejector.ejectorVelocity = 42
        const reread = editor.getUserData("synthesis:robot_preferences")!
        expect(reread.ejector.ejectorVelocity).toBe(defaultRobotPreferences().ejector.ejectorVelocity)
    })

    test("removeUserData only deletes the target key", () => {
        const editor = new FieldMiraEditor(mockParts())
        editor.setUserData("synthesis:robot_preferences", defaultRobotPreferences())
        editor.setUserData("synthesis:field_preferences", defaultFieldPreferences())
        editor.removeUserData("synthesis:field_preferences")
        expect(editor.getSynthesisKeys()).toEqual(["synthesis:robot_preferences"])
    })
})

describe("Devtool Scoring Zones Caching Tests", () => {
    test("cache round-trip preserves devtool scoring zones", () => {
        const parts = mockParts()
        const editor = new FieldMiraEditor(parts)
        editor.setUserData("synthesis:robot_preferences", defaultRobotPreferences())

        const encoded = mirabuf.Parts.encode(parts).finish()
        const decoded = mirabuf.Parts.decode(encoded)
        const roundTripEditor = new FieldMiraEditor(decoded)
        expect(roundTripEditor.getUserData("synthesis:robot_preferences")).toEqual(defaultRobotPreferences())
    })
})

describe("Asset tests", () => {
    test("FRC Field 2018 has spawn locations", async () => {
        const miraInstance = await getMiraInstance(2018)
        assert.exists(miraInstance)
        const mirabuf = new MirabufSceneObject(miraInstance)
        assert.exists(mirabuf)
        assert.exists(mirabuf.fieldPreferences)
        expect(mirabuf.fieldPreferences.spawnLocations).toMatchSnapshot()
    })
})
