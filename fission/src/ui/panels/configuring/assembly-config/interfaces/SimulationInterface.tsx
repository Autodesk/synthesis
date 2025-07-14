import MirabufSceneObject, { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Button from "@/ui/components/Button"
import Checkbox from "@/ui/components/Checkbox"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import React, { useState } from "react"

type SimulationInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

const SimulationInterface: React.FC<SimulationInterfaceProps> = ({ selectedAssembly }) => {
    const { openPanel } = usePanelControlContext()
    const [autoReconnect, setAutoReconnect] = useState<boolean>(
        PreferencesSystem.getGlobalPreference("SimAutoReconnect")
    )

    return (
        <>
            <Checkbox
                label="Auto Reconnect?"
                defaultState={autoReconnect}
                onClick={() => {
                    PreferencesSystem.setGlobalPreference("SimAutoReconnect", !autoReconnect)
                    setAutoReconnect(!autoReconnect)
                }}
            />
            <Button
                value="Wiring Panel"
                className="self-center"
                onClick={() => {
                    setSpotlightAssembly(selectedAssembly)
                    openPanel("wiring")
                }}
            />
            <Button
                value="Auto Testing"
                className="self-center"
                onClick={() => {
                    openPanel("auto-test")
                }}
            />
        </>
    )
}

export default SimulationInterface
