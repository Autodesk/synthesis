import { SCOREBOARD_MODES, type ScoreboardMode, type UserPreferences } from "./PreferenceTypes"

export const LEGACY_USER_PREFERENCE_KEYS = ["RenderScoreboard", "ScoreboardPreferenceSet"] as const

type LegacyUserPreferences = Partial<Record<(typeof LEGACY_USER_PREFERENCE_KEYS)[number], unknown>>

export type StoredUserPreferences = Partial<Record<keyof UserPreferences, unknown>> & LegacyUserPreferences

const isScoreboardMode = (value: unknown): value is ScoreboardMode => SCOREBOARD_MODES.includes(value as ScoreboardMode)

function scoreboardModeFromLegacy({
    RenderScoreboard,
    ScoreboardPreferenceSet,
}: LegacyUserPreferences): ScoreboardMode {
    if (RenderScoreboard === true) return "on"
    return ScoreboardPreferenceSet === true ? "off" : "auto"
}

export function migrateUserPreferences(stored: StoredUserPreferences): boolean {
    let changed = false

    if (LEGACY_USER_PREFERENCE_KEYS.some(key => key in stored)) {
        if (!isScoreboardMode(stored.ScoreboardMode)) stored.ScoreboardMode = scoreboardModeFromLegacy(stored)

        for (const key of LEGACY_USER_PREFERENCE_KEYS) delete stored[key]
        changed = true
    }

    if (stored.ScoreboardMode !== undefined && !isScoreboardMode(stored.ScoreboardMode)) {
        delete stored.ScoreboardMode
        changed = true
    }

    return changed
}
