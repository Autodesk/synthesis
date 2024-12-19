import MirabufSceneObject, { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Button from "@/ui/components/Button"
import Checkbox from "@/ui/components/Checkbox"
import { usePanelControlContext } from "@/ui/PanelContext"
import { useState } from "react"

type SimulationInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

export default function SimulationInterface({ selectedAssembly }: SimulationInterfaceProps) {
    const { openPanel } = usePanelControlContext()
    const [autoReconnect, setAutoReconnect] = useState<boolean>(
        PreferencesSystem.getGlobalPreference<boolean>("SimAutoReconnect")
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
