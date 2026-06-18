import type React from "react"
import { useCallback, useEffect, useState } from "react"
import { CameraMode, type CameraControlsType, type CustomOrbitControls } from "@/systems/scene/CameraControls"
import EventSystem from "@/systems/EventSystem"
import World from "@/systems/World"
import type { PanelImplProps } from "@/ui/components/Panel"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import CommandRegistry from "@/ui/components/CommandRegistry"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"

interface OrbitSettingsProps {
    controls: CustomOrbitControls
}

CommandRegistry.get().registerCommand({
    id: "open-camera-config",
    label: "Open Camera Configuration",
    description: "Open the Camera Config panel",
    keywords: ["camera", "config", "orbit", "follow", "locked", "face"],
    perform: () => import("./CameraSelectionPanel").then(m => globalOpenPanel(m.default, undefined)),
})

const OrbitSettings: React.FC<OrbitSettingsProps> = ({ controls }) => {
    const [mode, setMode] = useState<CameraMode>(controls.mode)

    useEffect(() => {
        return EventSystem.listen("CameraModeChangedEvent", ({ mode: newMode }) => {
            setMode(newMode as CameraMode)
        })
    }, [])

    useEffect(() => {
        controls.mode = mode
    }, [controls, mode])

    return (
        <ToggleButtonGroup
            orientation="vertical"
            value={mode}
            exclusive
            onChange={(_, v) => {
                if (v !== null) setMode(v as CameraMode)
            }}
        >
            <ToggleButton value={CameraMode.Follow}>Follow</ToggleButton>
            <ToggleButton value={CameraMode.Locked}>Locked</ToggleButton>
            <ToggleButton value={CameraMode.Face}>Face</ToggleButton>
        </ToggleButtonGroup>
    )
}

const CameraSelectionPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [cameraControlType, setCameraControlType] = useState<CameraControlsType>(
        World.sceneRenderer.currentCameraControls.controlsType
    )

    const setCameraControls = useCallback((t: CameraControlsType) => {
        switch (t) {
            case "Orbit":
                World.sceneRenderer.setCameraControls(t)
                setCameraControlType(t)
                break
            default:
                console.error("Unrecognized camera control option detected")
                break
        }
    }, [])

    useEffect(() => {
        configureScreen(panel!, { title: "Camera Config", hideAccept: true, cancelText: "Close" }, {})
    }, [])

    return (
        <div className="flex gap-2">
            <ToggleButtonGroup
                orientation="vertical"
                value={cameraControlType}
                exclusive
                onChange={(_, v) => {
                    if (v === null) return

                    setCameraControls(v)
                }}
            >
                <ToggleButton value="Orbit">Orbit</ToggleButton>
            </ToggleButtonGroup>
            {cameraControlType === "Orbit" && (
                <OrbitSettings controls={World.sceneRenderer.currentCameraControls as CustomOrbitControls} />
            )}
        </div>
    )
}

export default CameraSelectionPanel
