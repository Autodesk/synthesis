import React, { useCallback, useEffect, useState } from "react"
import { AiOutlineCamera } from "react-icons/ai"
import { CameraControlsType, CustomOrbitControls } from "@/systems/scene/CameraControls"
import { SoundPlayer } from "@/systems/sound/SoundPlayer"
import World from "@/systems/World"
import Checkbox from "@/ui/components/Checkbox"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"

interface OrbitSettingsProps {
    controls: CustomOrbitControls
}

const OrbitSettings: React.FC<OrbitSettingsProps> = ({ controls }) => {
    const [locked, setLocked] = useState<boolean>(controls.locked)

    useEffect(() => {
        controls.locked = locked
    }, [controls, locked])

    return <Checkbox label={"Lock to Robot"} defaultState={locked} onClick={v => setLocked(v)} />
}

const CameraSelectionPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
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

    return (
        <Panel
            openLocation="right"
            name={"Choose a Camera"}
            icon={<AiOutlineCamera />}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Close"
            contentClassName="items-center"
        >
            <ToggleButtonGroup
                orientation="vertical"
                value={cameraControlType}
                exclusive
                onChange={(_, v) => {
                    if (v != null) {
                        return
                    }

                    setCameraControls(v)
                }}
                {...SoundPlayer.buttonSoundEffects()}
            >
                <ToggleButton value={"Orbit"}>Orbit</ToggleButton>
            </ToggleButtonGroup>
            {cameraControlType == "Orbit" ? (
                <OrbitSettings controls={World.sceneRenderer.currentCameraControls as CustomOrbitControls} />
            ) : (
                <></>
            )}
        </Panel>
    )
}

export default CameraSelectionPanel
