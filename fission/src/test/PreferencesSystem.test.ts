import { describe, expect, test } from "vitest"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import {
    defaultFieldPreferences,
    defaultUserPreferences,
    type FieldPreferences,
    type GraphicsPreferences,
    type RobotPreferences,
    type UserPreference,
    type UserPreferences,
    USER_PREFERENCE_KEY,
} from "@/systems/preferences/PreferenceTypes"

const PREFERENCES_STORAGE_KEY = "Preferences"

/**
 * Captures the full current user-preferences state by resolving every key
 * defined by defaultUserPreferences() through the public getter, so the whole
 * set can be asserted with toMatchSnapshot().
 */
function captureUserPreferences(): UserPreferences {
    const keys = Object.keys(defaultUserPreferences()) as UserPreference[]
    return Object.fromEntries(keys.map(key => [key, PreferencesSystem.getUserPreference(key)])) as UserPreferences
}

function readSavedUserPreferences(): Record<string, unknown> {
    const saved = JSON.parse(window.localStorage.getItem(PREFERENCES_STORAGE_KEY) ?? "{}")
    return saved[USER_PREFERENCE_KEY] ?? {}
}

describe("Preferences System Global Values", () => {
    test("Setting values", () => {
        PreferencesSystem.setUserPreference("ZoomSensitivity", 7)
        PreferencesSystem.setUserPreference("RenderSceneTags", false)
        PreferencesSystem.setUserPreference("ShowViewCube", false)

        expect(PreferencesSystem.getUserPreference("ZoomSensitivity")).toBe(7)
        expect(PreferencesSystem.getUserPreference("RenderSceneTags")).toBe(false)
        expect(PreferencesSystem.getUserPreference("ShowViewCube")).toBe(false)
    })

    test("Setting without saving", () => {
        PreferencesSystem.setUserPreference("ZoomSensitivity", 13)
        PreferencesSystem.setUserPreference("RenderSceneTags", false)
        PreferencesSystem.setUserPreference("ShowViewCube", true)

        window.localStorage.setItem(PREFERENCES_STORAGE_KEY, "{}") // Clears local storage
        PreferencesSystem.loadPreferences()

        expect(captureUserPreferences()).toMatchSnapshot("default user preferences")
    })

    test("Reset to default if undefined", () => {
        PreferencesSystem.setUserPreference("ZoomSensitivity", undefined as unknown as number)
        PreferencesSystem.setUserPreference("RenderSceneTags", undefined as unknown as boolean)
        PreferencesSystem.setUserPreference("ShowViewCube", undefined as unknown as boolean)

        expect(captureUserPreferences()).toMatchSnapshot("default user preferences")
    })

    test("Setting then saving", () => {
        PreferencesSystem.setUserPreference("ZoomSensitivity", 13)
        PreferencesSystem.setUserPreference("RenderSceneTags", true)
        PreferencesSystem.setUserPreference("ShowViewCube", false)

        PreferencesSystem.savePreferences()
        PreferencesSystem.setUserPreference("ZoomSensitivity", 20)
        PreferencesSystem.setUserPreference("RenderSceneTags", false)
        PreferencesSystem.setUserPreference("ShowViewCube", true)
        PreferencesSystem.loadPreferences()

        expect(PreferencesSystem.getUserPreference("ZoomSensitivity")).toBe(13)
        expect(PreferencesSystem.getUserPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getUserPreference("ShowViewCube")).toBe(false)
    })

    test("Clearing preferences", () => {
        PreferencesSystem.setUserPreference("ZoomSensitivity", 13)
        PreferencesSystem.setUserPreference("RenderSceneTags", true)
        PreferencesSystem.setUserPreference("ShowViewCube", false)

        PreferencesSystem.clearPreferences()

        expect(captureUserPreferences()).toMatchSnapshot("default user preferences")
    })

    test("Loading a scoreboard choice saved by an older build", () => {
        window.localStorage.setItem(
            PREFERENCES_STORAGE_KEY,
            JSON.stringify({ [USER_PREFERENCE_KEY]: { RenderScoreboard: true, ScoreboardPreferenceSet: true } })
        )

        PreferencesSystem.loadPreferences()

        expect(PreferencesSystem.getUserPreference("ScoreboardMode")).toBe("on")
        expect(readSavedUserPreferences()).not.toHaveProperty("RenderScoreboard")
    })

    test("Loading a scoreboard choice saved before preferences were nested", () => {
        window.localStorage.setItem(
            PREFERENCES_STORAGE_KEY,
            JSON.stringify({ RenderScoreboard: false, UseMetric: true })
        )

        PreferencesSystem.loadPreferences()

        expect(PreferencesSystem.getUserPreference("ScoreboardMode")).toBe("auto")
        expect(PreferencesSystem.getUserPreference("UseMetric")).toBe(true)
        expect(readSavedUserPreferences()).not.toHaveProperty("RenderScoreboard")
    })

    test("Graphics preferences", () => {
        PreferencesSystem.getGraphicsPreferences()

        const graphicsPreferences: GraphicsPreferences = {
            lightIntensity: 0.8,
            fancyShadows: true,
            maxFar: 1000,
            cascades: 4,
            shadowMapSize: 2048,
            antiAliasing: true,
        }

        PreferencesSystem.getGraphicsPreferences().fancyShadows = graphicsPreferences.fancyShadows
        PreferencesSystem.getGraphicsPreferences().lightIntensity = graphicsPreferences.lightIntensity
        PreferencesSystem.getGraphicsPreferences().maxFar = graphicsPreferences.maxFar
        PreferencesSystem.getGraphicsPreferences().cascades = graphicsPreferences.cascades
        PreferencesSystem.getGraphicsPreferences().shadowMapSize = graphicsPreferences.shadowMapSize
        PreferencesSystem.getGraphicsPreferences().antiAliasing = graphicsPreferences.antiAliasing

        expect(PreferencesSystem.getGraphicsPreferences().lightIntensity).toEqual(graphicsPreferences.lightIntensity)
        expect(PreferencesSystem.getGraphicsPreferences().fancyShadows).toEqual(graphicsPreferences.fancyShadows)
        expect(PreferencesSystem.getGraphicsPreferences().maxFar).toEqual(graphicsPreferences.maxFar)
        expect(PreferencesSystem.getGraphicsPreferences().cascades).toEqual(graphicsPreferences.cascades)
        expect(PreferencesSystem.getGraphicsPreferences().shadowMapSize).toEqual(graphicsPreferences.shadowMapSize)
        expect(PreferencesSystem.getGraphicsPreferences().antiAliasing).toEqual(graphicsPreferences.antiAliasing)
    })
})

describe("Preference System Robot/Field", () => {
    test("Setting robot preferences", () => {
        const robotPreferences1: RobotPreferences = {
            inputsSchemes: [],
            motors: [],
            intake: {
                deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                zoneDiameter: 0.7,
                parentNode: undefined,
                showZoneAlways: true,
                maxPieces: 3,
                animationDuration: 0.5,
            },
            ejector: {
                deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                ejectorVelocity: 5,
                parentNode: undefined,
                ejectOrder: "FIFO",
            },
            cameras: [],
            driveVelocity: 3,
            driveAcceleration: 6,
            unstickForce: 8000,
        }
        const robotPreferences2: RobotPreferences = {
            inputsSchemes: [],
            motors: [],
            intake: {
                deltaTransformation: [1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                zoneDiameter: 0.3,
                parentNode: undefined,
                showZoneAlways: false,
                maxPieces: 1,
                animationDuration: 0.5,
            },
            ejector: {
                deltaTransformation: [1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                ejectorVelocity: 10,
                parentNode: undefined,
                ejectOrder: "LIFO",
            },
            cameras: [],
            driveVelocity: 1.5,
            driveAcceleration: 8,
            unstickForce: 10000,
        }

        PreferencesSystem.setRobotPreferences("RobotPreferences1", robotPreferences1)
        PreferencesSystem.setRobotPreferences("RobotPreferences2", robotPreferences2)

        expect(PreferencesSystem.getRobotPreferences("RobotPreferences1")).toEqual(robotPreferences1)
        expect(PreferencesSystem.getRobotPreferences("RobotPreferences2")).toEqual(robotPreferences2)
    })

    test("Setting field preferences", () => {
        const fieldPreferences1: FieldPreferences = {
            spawnLocations: defaultFieldPreferences().spawnLocations,
            scoringZones: [
                {
                    name: "Zone1",
                    alliance: "red",
                    parentNode: undefined,
                    points: 5,
                    destroyGamepiece: true,
                    shouldPointsAccumulate: true,
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                },
            ],
            protectedZones: [],
            cameraPoints: [],
        }
        const fieldPreferences2: FieldPreferences = {
            spawnLocations: defaultFieldPreferences().spawnLocations,
            scoringZones: [
                {
                    name: "Zone2",
                    alliance: "blue",
                    parentNode: undefined,
                    points: 20,
                    destroyGamepiece: false,
                    shouldPointsAccumulate: false,
                    deltaTransformation: [1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1],
                },
            ],
            protectedZones: [
                {
                    name: "ProtectedZone1",
                    alliance: "red",
                    parentNode: undefined,
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                    penaltyPoints: 2,
                    contactType: ContactType.ROBOT_ENTERS,
                    activeDuring: [MatchModeType.AUTONOMOUS, MatchModeType.TELEOP],
                },
            ],
            cameraPoints: [],
        }

        PreferencesSystem.setFieldPreferences("FieldPreferences1", fieldPreferences1)
        PreferencesSystem.setFieldPreferences("FieldPreferences2", fieldPreferences2)

        expect(PreferencesSystem.getFieldPreferences("FieldPreferences1")).toEqual(fieldPreferences1)
        expect(PreferencesSystem.getFieldPreferences("FieldPreferences2")).toEqual(fieldPreferences2)
    })
})
