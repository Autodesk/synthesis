import type { ScoreboardMode } from "@/systems/preferences/PreferenceTypes"

export const SCOREBOARD_MODE_LABELS: Record<ScoreboardMode, string> = {
    auto: "Auto (during gameplay)",
    on: "Always shown",
    off: "Always hidden",
}

export function isScoreboardVisible(mode: ScoreboardMode, gameplayActive: boolean): boolean {
    return mode === "on" || (mode === "auto" && gameplayActive)
}

export function toggledScoreboardMode(mode: ScoreboardMode, gameplayActive: boolean): ScoreboardMode {
    return isScoreboardVisible(mode, gameplayActive) ? "off" : "on"
}
