import type React from "react"
import type { CameraControlsType, CustomOrbitControls } from "@/systems/scene/CameraControls"
import { useCallback, useEffect, useState } from "react"
import { Checkbox, FormControlLabel } from "@mui/material"
import World from "@/systems/World"
import { ToggleButtonGroup, ToggleButton } from "@/ui/components/ToggleButtonGroup"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import buttonPressSound from "@/assets/sound-files/ButtonPress.mp3"

interface OrbitSettingsProps {
    controls: CustomOrbitControls
}

function OrbitSettings({ controls }: OrbitSettingsProps) {
    const [locked, setLocked] = useState<boolean>(controls.locked)

    useEffect(() => {
        controls.locked = locked
    }, [controls, locked])

    return (
        <FormControlLabel
            control={<Checkbox defaultChecked={locked} onChange={e => setLocked(e.target.checked)} />}
            label="Lock to Robot"
        />
    )
}

const CameraSelectionPanel: React.FC = () => {
    const [cameraControlType, setCameraControlType] = useState<CameraControlsType>(
        World.SceneRenderer.currentCameraControls.controlsType
    )

    const setCameraControls = useCallback((t: CameraControlsType) => {
        switch (t) {
            case "Orbit":
                World.SceneRenderer.SetCameraControls(t)
                setCameraControlType(t)
                break
            default:
                console.error("Unrecognized camera control option detected")
                break
        }
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
                <OrbitSettings controls={World.SceneRenderer.currentCameraControls as CustomOrbitControls} />
            )}
        </>
    )
}

export default CameraSelectionPanel
