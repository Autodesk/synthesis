import { Stack, Tooltip } from "@mui/material"
import { useEffect, useState } from "react"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Checkbox from "@/ui/components/Checkbox"
import { Button } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import FTCCreateDeviceModal from "@/ui/modals/configuring/ftc-config/FTCCreateDeviceModal"
import AutoTestPanel from "@/ui/panels/simulation/AutoTestPanel"
import WiringPanel from "@/ui/panels/simulation/WiringPanel"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

const SimulationInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, panel, registerCleanupFunction }) => {
    const { openPanel, closePanel, openModal } = useUIContext()
    const [autoReconnect, setAutoReconnect] = useState<boolean>(PreferencesSystem.getUserPreference("SimAutoReconnect"))
    const ftcActive = selectedAssembly.brain?.isFTC() ?? false

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
            {!ftcActive && (
                <Button
                    className="self-center"
                    onClick={() => {
                        setSpotlightAssembly(selectedAssembly)
                        openPanel(WiringPanel, undefined, panel)
                    }}
                >
                    Wiring Panel
                </Button>
            )}
            <Tooltip title={ftcActive ? "Not currently implemented for FTC CodeSim" : ""}>
                <span className="self-center">
                    <Button
                        className="self-center"
                        disabled={ftcActive}
                        onClick={() => {
                            openPanel(AutoTestPanel, undefined, panel)
                            if (panel) closePanel(panel.id, CloseType.OVERWRITE)
                        }}
                    >
                        Auto Testing
                    </Button>
                </span>
            </Tooltip>
            {ftcActive && (
                <Button className="self-center" onClick={() => openModal(FTCCreateDeviceModal, undefined)}>
                    Configure FTC Devices
                </Button>
            )}
        </Stack>
    )
}

export default SimulationInterface
