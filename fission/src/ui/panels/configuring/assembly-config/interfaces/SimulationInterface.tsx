import { Stack } from "@mui/material"
import { useEffect, useState } from "react"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SimDriverStation from "@/systems/simulation/wpilib_brain/sim/SimDriverStation"
import { RobotSimMode } from "@/systems/simulation/wpilib_brain/WPILibTypes"
import Checkbox from "@/ui/components/Checkbox"
import { Button } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import AutoTestPanel from "@/ui/panels/simulation/AutoTestPanel"
import WiringPanel from "@/ui/panels/simulation/WiringPanel"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

const SimulationInterface: ConfigurationSubpanelComponent = ({ selectedAssembly, panel, registerCleanupFunction }) => {
    const { openPanel, closePanel } = useUIContext()
    const [autoReconnect, setAutoReconnect] = useState<boolean>(PreferencesSystem.getUserPreference("SimAutoReconnect"))
    const [teleopEnabled, setTeleopEnabled] = useState<boolean>(() => SimDriverStation.isEnabled())

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
            <Button
                className="self-center"
                onClick={() => {
                    openPanel(AutoTestPanel, undefined, panel)
                    if (panel) closePanel(panel.id, CloseType.OVERWRITE)
                }}
            >
                Auto Testing
            </Button>
            <Button
                className="self-center"
                onClick={() => {
                    const next = !teleopEnabled
                    SimDriverStation.setMode(next ? RobotSimMode.TELEOP : RobotSimMode.DISABLED)
                    setTeleopEnabled(next)
                }}
            >
                {teleopEnabled ? "Disable Robot" : "Enable FTC Teleop"}
            </Button>
        </Stack>
    )
}

export default SimulationInterface
