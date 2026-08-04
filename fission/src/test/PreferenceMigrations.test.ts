import { describe, expect, test } from "vitest"
import { migrateUserPreferences, type StoredUserPreferences } from "@/systems/preferences/PreferenceMigrations"

describe("scoreboard mode migration", () => {
    test("a scoreboard the user pinned on becomes the on mode", () => {
        const stored: StoredUserPreferences = { RenderScoreboard: true, ScoreboardPreferenceSet: true }

        expect(migrateUserPreferences(stored)).toBe(true)
        expect(stored).toEqual({ ScoreboardMode: "on" })
    })

    test("a scoreboard the user turned off becomes the off mode", () => {
        const stored: StoredUserPreferences = { RenderScoreboard: false, ScoreboardPreferenceSet: true }

        expect(migrateUserPreferences(stored)).toBe(true)
        expect(stored).toEqual({ ScoreboardMode: "off" })
    })

    test("a user who never chose becomes the auto mode", () => {
        const stored: StoredUserPreferences = { RenderScoreboard: false, ScoreboardPreferenceSet: false }

        expect(migrateUserPreferences(stored)).toBe(true)
        expect(stored).toEqual({ ScoreboardMode: "auto" })
    })

    test("legacy keys left behind by a newer build do not overwrite its mode", () => {
        const stored: StoredUserPreferences = { ScoreboardMode: "off", RenderScoreboard: true }

        expect(migrateUserPreferences(stored)).toBe(true)
        expect(stored).toEqual({ ScoreboardMode: "off" })
    })

    test("a hand edited mode is dropped so the default takes over", () => {
        const stored: StoredUserPreferences = { ScoreboardMode: "always", UseMetric: true }

        expect(migrateUserPreferences(stored)).toBe(true)
        expect(stored).toEqual({ UseMetric: true })
    })

    test("current preferences are left untouched", () => {
        const stored: StoredUserPreferences = { ScoreboardMode: "auto", UseMetric: true }

        expect(migrateUserPreferences(stored)).toBe(false)
        expect(stored).toEqual({ ScoreboardMode: "auto", UseMetric: true })
    })
})
