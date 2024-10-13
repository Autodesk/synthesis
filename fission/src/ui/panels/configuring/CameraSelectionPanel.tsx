import { CameraControlsType, CustomOrbitControls } from "@/systems/scene/CameraControls"
import World from "@/systems/World"
import Checkbox from "@/ui/components/Checkbox"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { useCallback, useEffect, useState } from "react"
import { AiOutlineCamera } from "react-icons/ai"

interface OrbitSettingsProps {
    controls: CustomOrbitControls
}

function OrbitSettings({ controls }: OrbitSettingsProps) {
    const [locked, setLocked] = useState<boolean>(controls.locked)

    useEffect(() => {
        controls.locked = locked
    }, [controls, locked])

    return <Checkbox label={"Lock to Robot"} defaultState={locked} onClick={v => setLocked(v)} />
}

const CameraSelectionPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
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
            >
                <ToggleButton value={"Orbit"}>Orbit</ToggleButton>
            </ToggleButtonGroup>
            {cameraControlType == "Orbit" ? (
                <OrbitSettings controls={World.SceneRenderer.currentCameraControls as CustomOrbitControls} />
            ) : (
                <></>
            )}
        </Panel>
    )
}

export default CameraSelectionPanel
