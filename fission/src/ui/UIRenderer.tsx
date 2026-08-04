import React from "react"
import { Panel } from "@/components/Panel"
import { Modal } from "./components/Modal"
import Scoreboard from "./components/Scoreboard"
import { useScoreboard } from "./helpers/useScoreboard"
import { useUIContext } from "./helpers/UIProviderHelpers"

export const UIRenderer: React.FC = () => {
    const { modal, panels } = useUIContext()
    const { visible: showScoreboard } = useScoreboard()

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
