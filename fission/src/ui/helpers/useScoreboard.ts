import { useCallback, useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem, { useUserPreference } from "@/systems/preferences/PreferencesSystem"
import type { ScoreboardMode } from "@/systems/preferences/PreferenceTypes"
import { isScoreboardVisible, toggledScoreboardMode } from "@/ui/helpers/ScoreboardVisibility"
import { useStateContext } from "@/ui/helpers/StateProviderHelpers"

export interface Scoreboard {
    mode: ScoreboardMode
    visible: boolean
    setMode: (mode: ScoreboardMode) => void
    toggle: () => void
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

    const toggle = useCallback(
        () => setMode(toggledScoreboardMode(mode, gameplayActive)),
        [setMode, mode, gameplayActive]
    )

    return { mode, visible: isScoreboardVisible(mode, gameplayActive), setMode, toggle }
}
