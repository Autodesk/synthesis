import type { SxProps, Theme } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem, { useUserPreference } from "@/systems/preferences/PreferencesSystem"
import { type ScoreboardMode, SCOREBOARD_MODES } from "@/systems/preferences/PreferenceTypes"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"

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

export function nextScoreboardMode(mode: ScoreboardMode): ScoreboardMode {
    return SCOREBOARD_MODES[(SCOREBOARD_MODES.indexOf(mode) + 1) % SCOREBOARD_MODES.length]
}

export interface Scoreboard {
    mode: ScoreboardMode
    visible: boolean
    setMode: (mode: ScoreboardMode) => void
}

export function useScoreboard(): Scoreboard {
    const { appMode } = useStateContext()

    const [mode, writeMode] = useUserPreference("ScoreboardMode")
    const [inMatchMode, setInMatchMode] = useState(
        () => MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX
    )

    useEffect(
        () => EventSystem.listen("MatchStateChangedEvent", info => setInMatchMode(info.mode !== MatchModeType.SANDBOX)),
        []
    )

    const gameplayActive = appMode === "Gameplay" || inMatchMode

    const setMode = useCallback(
        (next: ScoreboardMode) => {
            writeMode(next)
            PreferencesSystem.savePreferences()
        },
        [writeMode]
    )

    return { mode, visible: isScoreboardVisible(mode, gameplayActive), setMode }
}
