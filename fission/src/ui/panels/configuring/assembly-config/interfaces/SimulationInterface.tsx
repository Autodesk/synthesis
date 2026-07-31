import { Tooltip } from "@mui/material"
import { useEffect, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject"
import FTCBrain from "@/systems/simulation/ftc_brain/FTCBrain"
import { getIsConnected as getFTCIsConnected } from "@/systems/simulation/ftc_brain/FTCState"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Checkbox from "@/ui/components/Checkbox"
import type { PanelImplProps } from "@/ui/components/Panel"
import { Button } from "@/ui/components/StyledComponents"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers"
import FTCCreateDeviceModal from "@/ui/modals/configuring/ftc-config/FTCCreateDeviceModal"
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
    const { openPanel, closePanel, openModal } = useUIContext()
    const [autoReconnect, setAutoReconnect] = useState<boolean>(PreferencesSystem.getUserPreference("SimAutoReconnect"))
    const [ftcConnected, setFtcConnected] = useState<boolean>(false)
    const ftcActive = selectedAssembly.brain instanceof FTCBrain && ftcConnected

    useEffect(() => {
        const handle = setInterval(() => setFtcConnected(getFTCIsConnected()), 500)
        return () => clearInterval(handle)
    }, [])

    return (
        <>
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
            {selectedAssembly.brain instanceof FTCBrain && (
                <Button className="self-center" onClick={() => openModal(FTCCreateDeviceModal, undefined)}>
                    Configure FTC Devices
                </Button>
            )}
        </>
    )
}
