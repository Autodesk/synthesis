import React from "react"
import { Panel } from "@/components/Panel"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { Modal } from "./components/Modal"
import Scoreboard from "./components/Scoreboard"
import { useUIContext } from "./helpers/UIProviderHelpers"

export const UIRenderer: React.FC = () => {
    const { modal, panels } = useUIContext()

    return (
        <>
            {PreferencesSystem.getGlobalPreference("RenderScoreboard") && <Scoreboard />}
            <div id="panel-container" className="relative pointer-events-none w-[100vw] h-[100vh]">
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
