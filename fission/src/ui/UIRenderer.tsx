import React, { useEffect, useRef, useState } from "react"
import { Panel } from "@/components/Panel"
import { Modal } from "./components/Modal"
import Scoreboard from "./components/Scoreboard"
import { useStateContext } from "./helpers/StateProviderHelpers"
import { useUIContext } from "./helpers/UIProviderHelpers"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import EventSystem from "@/systems/EventSystem"

export const UIRenderer: React.FC = () => {
    const { modal, panels } = useUIContext()
    const { appMode } = useStateContext()

    const [prefRenderScoreboard, setPrefRenderScoreboard] = useState(
        PreferencesSystem.getUserPreference("RenderScoreboard")
    )
    const [inMatchMode, setInMatchMode] = useState(MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX)
    const [hasUserToggled, setHasUserToggled] = useState(false)

    useEffect(() => {
        const removeMatchStateListener = EventSystem.listen("MatchStateChangedEvent", info => {
            setInMatchMode(info.mode !== MatchModeType.SANDBOX)
        })

        const removePrefListener = PreferencesSystem.addPreferenceEventListener("RenderScoreboard", e => {
            setPrefRenderScoreboard(e.prefValue)
        })

        return () => {
            removeMatchStateListener()
            removePrefListener()
        }
    }, [])

    const showScoreboard = hasUserToggled
        ? prefRenderScoreboard
        : appMode === "Gameplay" || prefRenderScoreboard || inMatchMode

    const showScoreboardRef = useRef(showScoreboard)
    showScoreboardRef.current = showScoreboard

    useEffect(
        () =>
            EventSystem.listen("ToggleScoreboardEvent", () => {
                setHasUserToggled(true)
                PreferencesSystem.setUserPreference("RenderScoreboard", !showScoreboardRef.current)
                PreferencesSystem.savePreferences()
            }),
        []
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
