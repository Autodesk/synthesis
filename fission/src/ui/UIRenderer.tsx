import React, { useEffect, useState } from "react"
import { Panel } from "@/components/Panel"
import { Modal } from "./components/Modal"
import Scoreboard from "./components/Scoreboard"
import { isScoreboardVisible, toggleScoreboard } from "./helpers/ScoreboardVisibility"
import { useStateContext } from "./helpers/StateProviderHelpers"
import { useUIContext } from "./helpers/UIProviderHelpers"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import EventSystem from "@/systems/EventSystem"

export const UIRenderer: React.FC = () => {
    const { modal, panels, addToast } = useUIContext()
    const { appMode } = useStateContext()

    const [preference, setPreference] = useState(() => PreferencesSystem.getUserPreference("RenderScoreboard"))
    const [preferenceSet, setPreferenceSet] = useState(() =>
        PreferencesSystem.getUserPreference("ScoreboardPreferenceSet")
    )
    const [inMatchMode, setInMatchMode] = useState(MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX)

    useEffect(() => {
        const removeMatchStateListener = EventSystem.listen("MatchStateChangedEvent", info => {
            setInMatchMode(info.mode !== MatchModeType.SANDBOX)
        })

        const removePrefListener = PreferencesSystem.addPreferenceEventListener("RenderScoreboard", e => {
            setPreference(e.prefValue)
        })

        const removePrefSetListener = PreferencesSystem.addPreferenceEventListener("ScoreboardPreferenceSet", e => {
            setPreferenceSet(e.prefValue)
        })

        return () => {
            removeMatchStateListener()
            removePrefListener()
            removePrefSetListener()
        }
    }, [])

    const suggestedByMode = appMode === "Gameplay" || inMatchMode
    const showScoreboard = isScoreboardVisible({ preference, preferenceSet, suggestedByMode })

    useEffect(
        () =>
            EventSystem.listen("ToggleScoreboardEvent", () => {
                const next = toggleScoreboard({ preference, preferenceSet, suggestedByMode })

                PreferencesSystem.setUserPreference("RenderScoreboard", next.preference)
                PreferencesSystem.setUserPreference("ScoreboardPreferenceSet", true)
                PreferencesSystem.savePreferences()

                if (next.announcePreference) {
                    addToast("info", "Scoreboard", "Saved as a preference, so it now stays on until you turn it off.")
                }
            }),
        [preference, preferenceSet, suggestedByMode, addToast]
    )

    return (
        <>
            {showScoreboard && <Scoreboard />}
            <div id="panel-container" className="relative pointer-events-none w-screen h-screen">
                {panels.map((p, _i) => (
                    <Panel key={`panel-${p.id}`} panel={p}>
                        {React.createElement(p.content)}
                    </Panel>
                ))}
            </div>
            {modal && <Modal modal={modal}>{React.createElement(modal.content)}</Modal>}
        </>
    )
}
