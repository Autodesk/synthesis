export interface ScoreboardState {
    preference: boolean
    preferenceSet: boolean
    suggestedByMode: boolean
}

export interface ScoreboardToggle {
    preference: boolean
    announcePreference: boolean
}

export function isScoreboardVisible({ preference, preferenceSet, suggestedByMode }: ScoreboardState): boolean {
    return preference || (!preferenceSet && suggestedByMode)
}

export function toggleScoreboard(state: ScoreboardState): ScoreboardToggle {
    const visible = isScoreboardVisible(state)
    const adoptingSuggestion = visible && !state.preference

    return { preference: adoptingSuggestion || !visible, announcePreference: adoptingSuggestion }
}
