import type React from "react"
import type { CameraControlsType, CustomOrbitControls } from "@/systems/scene/CameraControls"
import { useCallback, useEffect, useState } from "react"
import { ToggleButton, ToggleButtonGroup } from "@mui/material"
import World from "@/systems/World"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import buttonPressSound from "@/assets/sound-files/ButtonPress.mp3"
import Checkbox from "@/ui/components/Checkbox"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { PanelImplProps } from "@/ui/components/Panel"

interface OrbitSettingsProps {
    controls: CustomOrbitControls
}

function OrbitSettings({ controls }: OrbitSettingsProps) {
    const [locked, setLocked] = useState<boolean>(controls.locked)

    useEffect(() => {
        controls.locked = locked
    }, [controls, locked])

    return <Checkbox label="Lock to Robot" checked={locked} onClick={setLocked} />
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
        configureScreen(panel!, { title: "Choose a Camera", hideAccept: true, cancelText: "Close" }, {})
    }, [])

    return (
        <>
            <ToggleButtonGroup
                orientation="vertical"
                value={cameraControlType}
                exclusive
                onChange={(_, v) => {
                    if (v !== null) return

                    setCameraControls(v)
                }}
                onMouseDown={() => SoundPlayer.play(buttonPressSound)}
            >
                <ToggleButton value="Orbit">Orbit</ToggleButton>
            </ToggleButtonGroup>
            {cameraControlType === "Orbit" && (
                <OrbitSettings controls={World.sceneRenderer.currentCameraControls as CustomOrbitControls} />
            )}
        </>
    )
}

export default CameraSelectionPanel
