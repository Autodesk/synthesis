import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { test, describe, expect } from "vitest"

describe("Preferences System", () => {
    test("Settings without saving", () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", 15)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", true)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", false)

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(15)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(false)
    })
    test("Reset to default if undefined", () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", undefined)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", undefined)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", undefined)

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(15)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(true)
    })
    test("Settings then saving", () => {
        PreferencesSystem.setGlobalPreference("ZoomSensitivity", 13)
        PreferencesSystem.setGlobalPreference("RenderSceneTags", true)
        PreferencesSystem.setGlobalPreference("RenderScoreboard", false)

        PreferencesSystem.savePreferences()

        expect(PreferencesSystem.getGlobalPreference("ZoomSensitivity")).toBe(13)
        expect(PreferencesSystem.getGlobalPreference("RenderSceneTags")).toBe(true)
        expect(PreferencesSystem.getGlobalPreference("RenderScoreboard")).toBe(false)
    })
})
