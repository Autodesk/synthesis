import { Button, Checkbox, FormControlLabel } from "@mui/material"
import { useContext, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { PanelImplProps } from "@/ui/components/Panel"
import AutoTestPanel from "@/ui/panels/simulation/AutoTestPanel"
import WiringPanel from "@/ui/panels/simulation/WiringPanel"
import { UIContext } from "@/ui/UIProvider"

type SimulationInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

export default function SimulationInterface({
    selectedAssembly,
    panel,
    parent,
}: SimulationInterfaceProps & PanelImplProps) {
    const { openPanel } = useContext(UIContext)
    const [autoReconnect, setAutoReconnect] = useState<boolean>(
        PreferencesSystem.getGlobalPreference("SimAutoReconnect")
    )

    return (
        <>
            <FormControlLabel
                label="Auto Reconnect?"
                control={
                    <Checkbox
                        defaultChecked={autoReconnect}
                        onClick={() => {
                            PreferencesSystem.setGlobalPreference("SimAutoReconnect", !autoReconnect)
                            setAutoReconnect(!autoReconnect)
                        }}
                    />
                }
            />
            <Button
                className="self-center"
                onClick={() => {
                    setSpotlightAssembly(selectedAssembly)
                    openPanel(<WiringPanel />, panel)
                }}
            >
                Wiring Panel
            </Button>
            <Button
                className="self-center"
                onClick={() => {
                    openPanel(<AutoTestPanel />, panel)
                }}
            >
                Auto Testing
            </Button>
        </>
    )
}
