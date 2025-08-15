import { describe, expect, test } from "vitest"
import { ContactType } from "@/mirabuf/ZoneTypes"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import {
    defaultFieldPreferences,
    type FieldPreferences,
    type GraphicsPreferences,
    type MotorPreferences,
    type RobotPreferences,
} from "@/systems/preferences/PreferenceTypes"

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
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", undefined as unknown as number)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", undefined as unknown as boolean)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", undefined as unknown as boolean)

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
        const motorPreferences1: MotorPreferences = { name: "testName", maxForce: 10, maxVelocity: 5 }
        const motorPreferences2: MotorPreferences = { name: "testName2", maxForce: 20, maxVelocity: 10 }

        PreferencesSystem.setMotorPreferences("MotorPreferences1", motorPreferences1)
        PreferencesSystem.setMotorPreferences("MotorPreferences2", motorPreferences2)

        expect(PreferencesSystem.getMotorPreferences("MotorPreferences1")).toEqual(motorPreferences1)
        expect(PreferencesSystem.getMotorPreferences("MotorPreferences2")).toEqual(motorPreferences2)
        expect(PreferencesSystem.getAllMotorPreferences()).toEqual({
            MotorPreferences1: motorPreferences1,
            MotorPreferences2: motorPreferences2,
        })
    })

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
            driveVelocity: 1.5,
            driveAcceleration: 8,
            unstickForce: 10000,
        }

        PreferencesSystem.setRobotPreferences("RobotPreferences1", robotPreferences1)
        PreferencesSystem.setRobotPreferences("RobotPreferences2", robotPreferences2)

        expect(PreferencesSystem.getRobotPreferences("RobotPreferences1")).toEqual(robotPreferences1)
        expect(PreferencesSystem.getRobotPreferences("RobotPreferences2")).toEqual(robotPreferences2)
        expect(PreferencesSystem.getAllRobotPreferences()).toEqual({
            RobotPreferences1: robotPreferences1,
            RobotPreferences2: robotPreferences2,
        })
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
                    persistentPoints: false,
                    deltaTransformation: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1],
                },
            ],
            protectedZones: [],
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
                    persistentPoints: true,
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
        }

        PreferencesSystem.setFieldPreferences("FieldPreferences1", fieldPreferences1)
        PreferencesSystem.setFieldPreferences("FieldPreferences2", fieldPreferences2)

        expect(PreferencesSystem.getFieldPreferences("FieldPreferences1")).toEqual(fieldPreferences1)
        expect(PreferencesSystem.getFieldPreferences("FieldPreferences2")).toEqual(fieldPreferences2)
        expect(PreferencesSystem.getAllFieldPreferences()).toEqual({
            FieldPreferences1: fieldPreferences1,
            FieldPreferences2: fieldPreferences2,
        })
    })
})
