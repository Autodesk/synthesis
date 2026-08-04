import { useCallback, useEffect, useState } from "react"
import EventSystem from "@/systems/EventSystem"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
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

    const [mode, setMode] = useState(() => PreferencesSystem.getUserPreference("ScoreboardMode"))
    const [inMatchMode, setInMatchMode] = useState(
        () => MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX
    )

    useEffect(() => {
        const removeMatchStateListener = EventSystem.listen("MatchStateChangedEvent", info => {
            setInMatchMode(info.mode !== MatchModeType.SANDBOX)
        })

        const removeModeListener = PreferencesSystem.addPreferenceEventListener("ScoreboardMode", e => {
            setMode(e.prefValue)
        })

        return () => {
            removeMatchStateListener()
            removeModeListener()
        }
    }, [])

    const gameplayActive = appMode === "Gameplay" || inMatchMode

    const saveMode = useCallback((next: ScoreboardMode) => {
        PreferencesSystem.setUserPreference("ScoreboardMode", next)
        PreferencesSystem.savePreferences()
    }, [])

    const toggle = useCallback(
        () => saveMode(toggledScoreboardMode(mode, gameplayActive)),
        [saveMode, mode, gameplayActive]
    )

    return { mode, visible: isScoreboardVisible(mode, gameplayActive), setMode: saveMode, toggle }
}
