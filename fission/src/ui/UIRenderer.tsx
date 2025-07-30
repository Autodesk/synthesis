import type React from "react"
import { Panel } from "@/components/Panel"
import { Modal } from "./components/Modal"
import { useUIContext } from "./helpers/UIProviderHelpers"
import Scoreboard from "./components/Scoreboard"

export const UIRenderer: React.FC = () => {
    const { modal, panels } = useUIContext()

    return (
        <>
            <Scoreboard />
            <div id="panel-container" className="relative pointer-events-none w-[100vw] h-[100vh]">
                {panels.map((p, _i) => (
                    <Panel key={`panel-${p.id}`} panel={p}>
                        {p.content}
                    </Panel>
                ))}
            </div>
            {modal && <Modal modal={modal}>{modal.content}</Modal>}
        </>
    )
}
