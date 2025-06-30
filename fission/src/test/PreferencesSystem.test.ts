import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import {
    MotorPreferences,
    RobotPreferences,
    FieldPreferences,
    GraphicsPreferences,
} from "@/systems/preferences/PreferenceTypes"
import { test, describe, expect } from "vitest"

describe("Preferences System Global Values", () => {
    test("Setting values", () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", 7)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", false)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", false)

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(7)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(false)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(false)
    })

    test("Setting without saving", async () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", 13)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", false)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", true)

        window.localStorage.setItem("Preferences", "{}") // Clears local storage
        PreferencesSystem.loadPreferences()

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(15)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(true)
    })

    test("Reset to default if undefined", () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", undefined)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", undefined)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", undefined)

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(15)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(true)
    })

    test("Reset to default if null", () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", null)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", null)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", null)

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(15)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(true)
    })

    test("Setting then saving", () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", 13)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", true)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", false)

        PreferencesSystem.savePreferences()
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", 20)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", false)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", true)
        PreferencesSystem.loadPreferences()

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(13)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(false)
    })

    test("Clearing preferences", () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", 13)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", true)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", false)

        PreferencesSystem.clearPreferences()

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(15)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(true)
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
    test("Setting motor preferences", () => {
        const motorPreferences_1: MotorPreferences = { name: "testName", maxForce: 10, maxVelocity: 5 }
        const motorPreferences_2: MotorPreferences = { name: "testName2", maxForce: 20, maxVelocity: 10 }

        PreferencesSystem.setMotorPreferences("MotorPreferences_1", motorPreferences_1)
        PreferencesSystem.setMotorPreferences("MotorPreferences_2", motorPreferences_2)

        expect(PreferencesSystem.getMotorPreferences("MotorPreferences_1")).toEqual(motorPreferences_1)
        expect(PreferencesSystem.getMotorPreferences("MotorPreferences_2")).toEqual(motorPreferences_2)
        expect(PreferencesSystem.getAllMotorPreferences()).toEqual({
            MotorPreferences_1: motorPreferences_1,
            MotorPreferences_2: motorPreferences_2,
        })
    })

    test("Setting robot preferences", () => {
        const robotPreferences_1: RobotPreferences = {
            inputsSchemes: [],
            motors: [],
            intake: {
                deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                zoneDiameter: 0.7,
                parentNode: undefined,
                showZoneAlways: true,
            },
            ejector: {
                deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                ejectorVelocity: 5,
                parentNode: undefined,
            },
            driveVelocity: 3,
            driveAcceleration: 6,
        }
        const robotPreferences_2: RobotPreferences = {
            inputsSchemes: [],
            motors: [],
            intake: {
                deltaTransformation: [1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                zoneDiameter: 0.3,
                parentNode: undefined,
                showZoneAlways: false,
            },
            ejector: {
                deltaTransformation: [1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                ejectorVelocity: 10,
                parentNode: undefined,
            },
            driveVelocity: 1.5,
            driveAcceleration: 8,
        }

        PreferencesSystem.setRobotPreferences("RobotPreferences_1", robotPreferences_1)
        PreferencesSystem.setRobotPreferences("RobotPreferences_2", robotPreferences_2)

        expect(PreferencesSystem.getRobotPreferences("RobotPreferences_1")).toEqual(robotPreferences_1)
        expect(PreferencesSystem.getRobotPreferences("RobotPreferences_2")).toEqual(robotPreferences_2)
        expect(PreferencesSystem.getAllRobotPreferences()).toEqual({
            RobotPreferences_1: robotPreferences_1,
            RobotPreferences_2: robotPreferences_2,
        })
    })

    test("Setting field preferences", () => {
        const fieldPreferences_1: FieldPreferences = {
            defaultSpawnLocation: [0, 1, 0],
            scoringZones: [
                {
                    name: "Zone1",
                    alliance: "red",
                    parentNode: undefined,
                    points: 5,
                    destroyGamepiece: true,
                    persistentPoints: false,
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                },
            ],
        }
        const fieldPreferences_2: FieldPreferences = {
            defaultSpawnLocation: [1, 1, 1],
            scoringZones: [
                {
                    name: "Zone2",
                    alliance: "blue",
                    parentNode: undefined,
                    points: 20,
                    destroyGamepiece: false,
                    persistentPoints: true,
                    deltaTransformation: [1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1],
                },
            ],
        }

        PreferencesSystem.setFieldPreferences("FieldPreferences_1", fieldPreferences_1)
        PreferencesSystem.setFieldPreferences("FieldPreferences_2", fieldPreferences_2)

        expect(PreferencesSystem.getFieldPreferences("FieldPreferences_1")).toEqual(fieldPreferences_1)
        expect(PreferencesSystem.getFieldPreferences("FieldPreferences_2")).toEqual(fieldPreferences_2)
        expect(PreferencesSystem.getAllFieldPreferences()).toEqual({
            FieldPreferences_1: fieldPreferences_1,
            FieldPreferences_2: fieldPreferences_2,
        })
    })
})
