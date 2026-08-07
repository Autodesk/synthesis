import type { SxProps, Theme } from "@mui/material"
import { useCallback, useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { type ScoreboardMode, SCOREBOARD_MODES } from "@/systems/preferences/PreferenceTypes"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import { TOP_BAR_GLYPH_SX } from "@/ui/components/topbar/TopBarConfig"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"

const MUTED_GLYPH_OPACITY = 0.55

export const SCOREBOARD_GLYPH_SX: Record<ScoreboardMode, SxProps<Theme>> = {
    auto: TOP_BAR_GLYPH_SX,
    on: { ...TOP_BAR_GLYPH_SX, color: "primary.main" },
    off: { ...TOP_BAR_GLYPH_SX, opacity: MUTED_GLYPH_OPACITY },
}

export function isScoreboardVisible(mode: ScoreboardMode, gameplayActive: boolean, matchActive = false): boolean {
    return matchActive || mode === "on" || (mode === "auto" && gameplayActive)
}

export function nextScoreboardMode(mode: ScoreboardMode): ScoreboardMode {
    return SCOREBOARD_MODES[(SCOREBOARD_MODES.indexOf(mode) + 1) % SCOREBOARD_MODES.length]
}

export interface Scoreboard {
    mode: ScoreboardMode
    visible: boolean
    overriddenByMatch: boolean // the scoreboard is visible when a match is active
    setMode: (mode: ScoreboardMode) => void
}

export function useScoreboard(): Scoreboard {
    const { appMode } = useStateContext()

    const [mode, setStoredMode] = useState(PreferencesSystem.getUserPreference("ScoreboardMode"))
    const [inMatchMode, setInMatchMode] = useState(
        () => MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX
    )

    useEffect(
        () => EventSystem.listen("MatchStateChangedEvent", info => setInMatchMode(info.mode !== MatchModeType.SANDBOX)),
        []
    )

    useEffect(() => PreferencesSystem.addPreferenceEventListener("ScoreboardMode", e => setStoredMode(e.prefValue)), [])

    const gameplayActive = appMode === "Gameplay" || inMatchMode

    const setMode = useCallback(
        (next: ScoreboardMode) => {
            if (inMatchMode && !isScoreboardVisible(next, true)) {
                globalAddToast("error", "Match In Progress", "The scoreboard cannot be hidden during a match.")
                return
            }

            PreferencesSystem.setUserPreference("ScoreboardMode", next)
            PreferencesSystem.savePreferences()
        },
        [inMatchMode]
    )

    return {
        mode,
        visible: isScoreboardVisible(mode, gameplayActive, inMatchMode),
        overriddenByMatch: inMatchMode && !isScoreboardVisible(mode, gameplayActive),
        setMode,
    }
}
