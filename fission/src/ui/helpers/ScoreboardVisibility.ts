import type { SxProps, Theme } from "@mui/material"
import type { ScoreboardMode } from "@/systems/preferences/PreferenceTypes"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"

export const SCOREBOARD_MODE_LABELS: Record<ScoreboardMode, string> = {
    auto: "Auto (during gameplay)",
    on: "Always shown",
    off: "Always hidden",
}

const MUTED_GLYPH_OPACITY = 0.55

export const SCOREBOARD_GLYPH_SX: Record<ScoreboardMode, SxProps<Theme>> = {
    auto: TOP_BAR_GLYPH_SX,
    on: { ...TOP_BAR_GLYPH_SX, color: "primary.main" },
    off: { ...TOP_BAR_GLYPH_SX, opacity: MUTED_GLYPH_OPACITY },
}

export function isScoreboardVisible(mode: ScoreboardMode, gameplayActive: boolean): boolean {
    return mode === "on" || (mode === "auto" && gameplayActive)
}

export function toggledScoreboardMode(mode: ScoreboardMode, gameplayActive: boolean): ScoreboardMode {
    return isScoreboardVisible(mode, gameplayActive) ? "off" : "on"
}
