import { describe, expect, test } from "vitest"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import {
    defaultFieldPreferences,
    defaultUserPreferences,
    MAX_UNSTICK_STRENGTH,
    MIN_UNSTICK_STRENGTH,
    type FieldPreferences,
    type GraphicsPreferences,
    type RobotPreferences,
    type UserPreference,
} from "@/systems/preferences/PreferenceTypes"

function expectDefaultPreferences() {
    const defaults = defaultUserPreferences()
    const keys = Object.keys(defaults) as UserPreference[]
    keys.forEach(key => {
        expect(PreferencesSystem.getUserPreference(key), `Mismatch in preference ${key}`).toEqual(defaults[key])
    })
}

describe("Preferences System Global Values", () => {
    test("Setting values", () => {
        PreferencesSystem.setUserPreference("SceneRotationSensitivity", 7)
        PreferencesSystem.setUserPreference("RenderSceneTags", false)
        PreferencesSystem.setUserPreference("RenderScoreboard", false)

        expect(PreferencesSystem.getUserPreference("SceneRotationSensitivity")).toBe(7)
        expect(PreferencesSystem.getUserPreference("RenderSceneTags")).toBe(false)
        expect(PreferencesSystem.getUserPreference("RenderScoreboard")).toBe(false)
    })

    test("Setting without saving", () => {
        PreferencesSystem.setUserPreference("SceneRotationSensitivity", 13)
        PreferencesSystem.setUserPreference("RenderSceneTags", false)
        PreferencesSystem.setUserPreference("RenderScoreboard", true)

        window.localStorage.setItem("Preferences", "{}") // Clears local storage
        PreferencesSystem.loadPreferences()

        expectDefaultPreferences()
    })

    test("Reset to default if undefined", () => {
        PreferencesSystem.setUserPreference("SceneRotationSensitivity", undefined as unknown as number)
        PreferencesSystem.setUserPreference("RenderSceneTags", undefined as unknown as boolean)
        PreferencesSystem.setUserPreference("RenderScoreboard", undefined as unknown as boolean)

        expectDefaultPreferences()
    })

    test("Setting then saving", () => {
        PreferencesSystem.setUserPreference("SceneRotationSensitivity", 13)
        PreferencesSystem.setUserPreference("RenderSceneTags", true)
        PreferencesSystem.setUserPreference("RenderScoreboard", false)

        PreferencesSystem.savePreferences()
        PreferencesSystem.setUserPreference("SceneRotationSensitivity", 20)
        PreferencesSystem.setUserPreference("RenderSceneTags", false)
        PreferencesSystem.setUserPreference("RenderScoreboard", true)
        PreferencesSystem.loadPreferences()

        expect(PreferencesSystem.getUserPreference("SceneRotationSensitivity")).toBe(13)
        expect(PreferencesSystem.getUserPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getUserPreference("RenderScoreboard")).toBe(false)
    })

    test("Clearing preferences", () => {
        PreferencesSystem.setUserPreference("SceneRotationSensitivity", 13)
        PreferencesSystem.setUserPreference("RenderSceneTags", true)
        PreferencesSystem.setUserPreference("RenderScoreboard", false)

        PreferencesSystem.clearPreferences()

        expectDefaultPreferences()
    })

    test("Onboarding tour flag defaults to false and persists", () => {
        // First-visit detection relies on this defaulting to false for a fresh browser.
        PreferencesSystem.clearPreferences()
        expect(PreferencesSystem.getUserPreference("HasSeenOnboardingTour")).toBe(false)

        PreferencesSystem.setUserPreference("HasSeenOnboardingTour", true)
        PreferencesSystem.savePreferences()
        PreferencesSystem.loadPreferences()

        expect(PreferencesSystem.getUserPreference("HasSeenOnboardingTour")).toBe(true)
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
            unstickStrength: MAX_UNSTICK_STRENGTH,
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
            unstickStrength: MIN_UNSTICK_STRENGTH,
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
