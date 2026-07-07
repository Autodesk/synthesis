import { assert, describe, expect, test, vi } from "vitest"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader.ts"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject.ts"
import { mirabuf } from "@/proto/mirabuf"
import {
    type Alliance,
    defaultFieldPreferences,
    defaultRobotPreferences,
    defaultRobotSpawnLocation,
    type ScoringZonePreferences,
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

const scoringZonePayload: ScoringZonePreferences[] = [
    {
        name: "Red Zone",
        alliance: "red" as Alliance,
        parentNode: "root",
        points: 5,
        destroyGamepiece: false,
        shouldPointsAccumulate: false,
        deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
    },
]

describe("Basic Field Mira Editor Tests", () => {
    test("writes and reads devtool data", () => {
        const parts = mockParts()
        const editor = new FieldMiraEditor(parts)

        const key = "devtool:scoring_zones"
        const payload: ScoringZonePreferences[] = [
            {
                name: "Test Zone",
                alliance: "blue" as Alliance,
                parentNode: "root",
                points: 10,
                destroyGamepiece: false,
                shouldPointsAccumulate: true,
                deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
            },
        ]

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
    test("add scoring zones and read back", () => {
        const parts = mockParts()
        const editor = new FieldMiraEditor(parts)
        editor.setUserData("devtool:scoring_zones", scoringZonePayload)
        expect(editor.getUserData("devtool:scoring_zones")).toEqual(scoringZonePayload)
        expect(editor.getAllKeys()).toContain("devtool:scoring_zones")
    })

    test("overwrite and remove scoring zones", () => {
        const parts = mockParts()
        const editor = new FieldMiraEditor(parts)
        editor.setUserData("devtool:scoring_zones", scoringZonePayload)

        const newPayload: ScoringZonePreferences[] = [
            { ...scoringZonePayload[0], name: "Blue Zone", alliance: "blue" as Alliance },
        ]
        editor.setUserData("devtool:scoring_zones", newPayload)
        expect(editor.getUserData("devtool:scoring_zones")).toEqual(newPayload)

        editor.removeUserData("devtool:scoring_zones")
        expect(editor.getUserData("devtool:scoring_zones")).toBeUndefined()
        expect(editor.getAllKeys()).not.toContain("devtool:scoring_zones")
    })
    test("cache round-trip preserves devtool scoring zones", () => {
        const parts = mockParts()
        const editor = new FieldMiraEditor(parts)
        editor.setUserData("devtool:scoring_zones", scoringZonePayload)

        const encoded = mirabuf.Parts.encode(parts).finish()
        const decoded = mirabuf.Parts.decode(encoded)
        const roundTripEditor = new FieldMiraEditor(decoded)
        expect(roundTripEditor.getUserData("devtool:scoring_zones")).toEqual(scoringZonePayload)
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
