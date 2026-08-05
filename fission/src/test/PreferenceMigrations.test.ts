import { describe, expect, test } from "vitest"
import { migrateUserPreferences, type StoredUserPreferences } from "@/systems/preferences/PreferenceMigrations"
import type { ScoreboardMode } from "@/systems/preferences/PreferenceTypes"

const LEGACY_CASES: [boolean, boolean, ScoreboardMode][] = [
    [true, true, "on"],
    [true, false, "on"],
    [false, true, "off"],
    [false, false, "auto"],
]

describe("scoreboard mode migration", () => {
    test.each(LEGACY_CASES)(
        "RenderScoreboard %s with ScoreboardPreferenceSet %s becomes the %s mode",
        (renderScoreboard, scoreboardPreferenceSet, mode) => {
            const stored: StoredUserPreferences = {
                RenderScoreboard: renderScoreboard,
                ScoreboardPreferenceSet: scoreboardPreferenceSet,
            }

            expect(migrateUserPreferences(stored)).toBe(true)
            expect(stored).toEqual({ ScoreboardMode: mode })
        }
    )

    test("an already migrated mode wins over the legacy keys beside it", () => {
        const stored: StoredUserPreferences = { ScoreboardMode: "off", RenderScoreboard: true }

        expect(migrateUserPreferences(stored)).toBe(true)
        expect(stored).toEqual({ ScoreboardMode: "off" })
    })

    test("an unreadable mode is dropped so the default takes over", () => {
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
