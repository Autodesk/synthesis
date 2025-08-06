import { ToggleButton, ToggleButtonGroup } from "@mui/material"
import SceneRenderer from "@/systems/scene/SceneRenderer"
import type React from "react"
import { useCallback, useEffect, useState } from "react"
import buttonPressSound from "@/assets/sound-files/ButtonPress.mp3"
import type { CameraControlsType, CustomOrbitControls } from "@/systems/scene/CameraControls"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import Checkbox from "@/ui/components/Checkbox"
import type { PanelImplProps } from "@/ui/components/Panel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

interface OrbitSettingsProps {
    controls: CustomOrbitControls
}

const OrbitSettings: React.FC<OrbitSettingsProps> = ({ controls }) => {
    const [locked, setLocked] = useState<boolean>(controls.locked)

    useEffect(() => {
        controls.locked = locked
    }, [controls, locked])

    return <Checkbox label="Lock to Robot" checked={locked} onClick={setLocked} />
}

const CameraSelectionPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()
    const [cameraControlType, setCameraControlType] = useState<CameraControlsType>(
        SceneRenderer.currentCameraControls.controlsType
    )

    const setCameraControls = useCallback((t: CameraControlsType) => {
        switch (t) {
            case "Orbit":
                SceneRenderer.setCameraControls(t)
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
                <OrbitSettings controls={SceneRenderer.currentCameraControls as CustomOrbitControls} />
            )}
        </>
    )
}

export default CameraSelectionPanel
