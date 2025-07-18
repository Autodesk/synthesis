import { Button, Checkbox, FormControlLabel } from "@mui/material"
import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { PanelImplProps } from "@/ui/components/Panel"
import AutoTestPanel from "@/ui/panels/simulation/AutoTestPanel"
import WiringPanel from "@/ui/panels/simulation/WiringPanel"
import { CloseType, useUIContext } from "@/ui/UIProvider"

type SimulationInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

export default function SimulationInterface({
    selectedAssembly,
    panel,
}: SimulationInterfaceProps & PanelImplProps<void>) {
    const { openPanel, closePanel } = useUIContext()
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
                    if (panel) closePanel(panel.id, CloseType.Overwrite)
                }}
            >
                Auto Testing
            </Button>
        </>
    )
}
