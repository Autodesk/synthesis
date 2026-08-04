import { useEffect, useState } from "react"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Checkbox from "@/ui/components/Checkbox"
import { Button } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import WiringPanel from "@/ui/panels/simulation/WiringPanel"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"
import { Stack } from "@mui/material"

const SimulationInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, panel, registerCleanupFunction }) => {
    const { openPanel } = useUIContext()
    const [autoReconnect, setAutoReconnect] = useState<boolean>(PreferencesSystem.getUserPreference("SimAutoReconnect"))

    useEffect(() => {
        const originalAutoReconnect = PreferencesSystem.getUserPreference("SimAutoReconnect")
        registerCleanupFunction(undefined, () => {
            PreferencesSystem.setUserPreference("SimAutoReconnect", originalAutoReconnect)
        })
    }, [registerCleanupFunction])

    return (
        <Stack direction={"column"} gap={2}>
            <Checkbox
                label="Auto Reconnect?"
                checked={autoReconnect}
                onClick={_ => {
                    PreferencesSystem.setUserPreference("SimAutoReconnect", !autoReconnect)
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
        </Stack>
    )
}

export default SimulationInterface
