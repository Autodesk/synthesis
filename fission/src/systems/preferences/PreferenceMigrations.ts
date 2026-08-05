import { SCOREBOARD_MODES, type ScoreboardMode, type UserPreferences } from "./PreferenceTypes"

type LegacyUserPreferences = Partial<Record<"RenderScoreboard" | "ScoreboardPreferenceSet", unknown>>

export type StoredUserPreferences = Partial<Record<keyof UserPreferences, unknown>> & LegacyUserPreferences

interface UserPreferenceMigration {
    legacyKeys: readonly (keyof StoredUserPreferences)[]
    apply: (stored: StoredUserPreferences) => boolean
}

const SCOREBOARD_LEGACY_KEYS = ["RenderScoreboard", "ScoreboardPreferenceSet"] as const

const isScoreboardMode = (value: unknown): value is ScoreboardMode => SCOREBOARD_MODES.includes(value as ScoreboardMode)

function scoreboardModeFromLegacy({
    RenderScoreboard,
    ScoreboardPreferenceSet,
}: LegacyUserPreferences): ScoreboardMode {
    if (RenderScoreboard === true) return "on"
    return ScoreboardPreferenceSet === true ? "off" : "auto"
}

const scoreboardModeMigration: UserPreferenceMigration = {
    legacyKeys: SCOREBOARD_LEGACY_KEYS,
    apply: stored => {
        let changed = false

        if (SCOREBOARD_LEGACY_KEYS.some(key => key in stored)) {
            if (!isScoreboardMode(stored.ScoreboardMode)) stored.ScoreboardMode = scoreboardModeFromLegacy(stored)

            for (const key of SCOREBOARD_LEGACY_KEYS) delete stored[key]
            changed = true
        }

        if ("ScoreboardMode" in stored && !isScoreboardMode(stored.ScoreboardMode)) {
            delete stored.ScoreboardMode
            changed = true
        }

        return changed
    },
}

const USER_PREFERENCE_MIGRATIONS: readonly UserPreferenceMigration[] = [scoreboardModeMigration]

export const LEGACY_USER_PREFERENCE_KEYS: readonly (keyof StoredUserPreferences)[] = USER_PREFERENCE_MIGRATIONS.flatMap(
    migration => migration.legacyKeys
)

export function migrateUserPreferences(stored: StoredUserPreferences): boolean {
    let changed = false

    for (const migration of USER_PREFERENCE_MIGRATIONS) {
        if (migration.apply(stored)) changed = true
    }

    return changed
}
