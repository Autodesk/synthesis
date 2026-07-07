import { assert, describe, expect, test, vi } from "vitest"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader.ts"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject.ts"
import { mirabuf } from "@/proto/mirabuf"
import {
    defaultFieldPreferences,
    defaultRobotPreferences,
    defaultRobotSpawnLocation,
} from "@/systems/preferences/PreferenceTypes.ts"
import FieldMiraEditor from "../../mirabuf/FieldMiraEditor.ts"

function mockParts(): mirabuf.IParts {
    return { userData: { data: {} } }
}

vi.mock("@/systems/World", () => ({
    default: {
        sceneRenderer: {
            setupMaterial: vi.fn(),
        },
        physicsSystem: {
            createMechanismFromParser: vi.fn().mockReturnValue(() => ({})),
        },
    },
}))

describe("Basic Field Mira Editor Tests", () => {
    test("writes and reads devtool data", () => {
        const parts = mockParts()
        const editor = new FieldMiraEditor(parts)

        const key = "synthesis:robot_preferences"
        const payload = defaultRobotPreferences()

        editor.setUserData(key, payload)
        expect(editor.getUserData(key)).toEqual(payload)
        expect(editor.getAllKeys()).toContain(key)

        editor.removeUserData(key)
        expect(editor.getUserData(key)).toBeUndefined()
    })

    test("default state: no keys, getUserData yields undefined", () => {
        const editor = new FieldMiraEditor(mockParts())
        expect(editor.getAllKeys()).toEqual([])
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
        expect(editor.getAllKeys()).toEqual(["synthesis:robot_preferences"])
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
        const file = await MirabufCachingService.cacheRemote("/api/mira/fields/FRC Field 2018 v13.mira", MiraType.FIELD)
            .then(async x => ({ hash: x!.hash, asset: await MirabufCachingService.get(x!.hash) }))
            .catch(e => {
                console.error("Could not get mirabuf file", e)
                return undefined
            })
        assert.exists(file)

        const mirabuf = await createMirabuf(file.hash, file.asset!)
        assert.exists(mirabuf)
        assert.exists(mirabuf.fieldPreferences)
        expect(mirabuf.fieldPreferences.spawnLocations.hasConfiguredLocations).toBe(true)
        expect(mirabuf.fieldPreferences.spawnLocations.red["1"]).not.toStrictEqual(defaultRobotSpawnLocation())
        expect(mirabuf.fieldPreferences.spawnLocations.default).not.toStrictEqual(defaultRobotSpawnLocation())
    })
})
