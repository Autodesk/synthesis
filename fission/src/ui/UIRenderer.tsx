import React, { useEffect, useState } from "react"
import { Panel } from "@/components/Panel"
import { Modal } from "./components/Modal"
import Scoreboard from "./components/Scoreboard"
import { useUIContext } from "./helpers/UIProviderHelpers"
import MatchMode, { MatchStateChangeEvent } from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"

export const UIRenderer: React.FC = () => {
    const { modal, panels } = useUIContext()

    const [prefRenderScoreboard, setPrefRenderScoreboard] = useState(
        PreferencesSystem.getGlobalPreference("RenderScoreboard")
    )
    const [inMatchMode, setInMatchMode] = useState(MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX)

    useEffect(() => {
        const onMatchStateChange = (e: MatchStateChangeEvent) => {
            setInMatchMode(e.matchModeType !== MatchModeType.SANDBOX)
        }
        MatchStateChangeEvent.addListener(onMatchStateChange)

        const removePrefListener = PreferencesSystem.addPreferenceEventListener("RenderScoreboard", e => {
            setPrefRenderScoreboard(e.prefValue)
        })

        return () => {
            MatchStateChangeEvent.removeListener(onMatchStateChange)
            removePrefListener()
        }
    }, [])

    const showScoreboard = prefRenderScoreboard || inMatchMode

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
