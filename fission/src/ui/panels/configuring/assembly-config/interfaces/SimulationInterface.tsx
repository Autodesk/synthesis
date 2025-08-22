import { useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Checkbox from "@/ui/components/Checkbox"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import AutoTestPanel from "@/ui/panels/simulation/AutoTestPanel"
import WiringPanel from "@/ui/panels/simulation/WiringPanel"
import type { ConfigurePanelCustomProps } from "../ConfigurePanel"

type SimulationInterfaceProps = {
    selectedAssembly: MirabufSceneObject
}

export default function SimulationInterface({
    selectedAssembly,
    panel,
}: SimulationInterfaceProps & PanelImplProps<void, ConfigurePanelCustomProps>) {
    const { openPanel, closePanel } = useUIContext()
    const [autoReconnect, setAutoReconnect] = useState<boolean>(
        PreferencesSystem.getGlobalPreference("SimAutoReconnect")
    )

    return (
        <>
            <Checkbox
                label="Auto Reconnect?"
                checked={autoReconnect}
                onClick={_ => {
                    PreferencesSystem.setGlobalPreference("SimAutoReconnect", !autoReconnect)
                    setAutoReconnect(!autoReconnect)
                }}
            />
            <Button
                className="self-center"
                onClick={() => {
                    setSpotlightAssembly(selectedAssembly)
                    openPanel(WiringPanel, undefined, panel)
                }}
            >
                Wiring Panel
            </Button>
            <Button
                className="self-center"
                onClick={() => {
                    openPanel(AutoTestPanel, undefined, panel)
                    if (panel) closePanel(panel.id, CloseType.Overwrite)
                }}
            >
                Auto Testing
            </Button>
        </>
    )
}
