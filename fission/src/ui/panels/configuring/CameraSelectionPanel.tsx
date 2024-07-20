import { CameraControlsType } from "@/systems/scene/CameraControls"
import World from "@/systems/World"
import Panel, { PanelPropsImpl } from "@/ui/components/Panel"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { useEffect, useState } from "react"
import { AiOutlineCamera } from "react-icons/ai"

const CameraSelectionPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [cameraControlType, setCameraControlType] = useState<CameraControlsType>(World.SceneRenderer.currentCameraControls.controlsType)

    useEffect(() => {
        if (World.SceneRenderer.currentCameraControls.controlsType != cameraControlType) {
            World.SceneRenderer.SetCameraControls(cameraControlType)
        }
    }, [cameraControlType])

    return (
        <Panel
            openLocation="bottom-right"
            name={"Choose a Camera"}
            icon={<AiOutlineCamera />}
            panelId={panelId}
            acceptEnabled={false}
            cancelName="Close"
        >
            <ToggleButtonGroup
                orientation="vertical"
                value={cameraControlType}
                exclusive
                onChange={(_, v) => v != null && setCameraControlType(v as CameraControlsType)}
            >
                <ToggleButton value={CameraControlsType.OrbitFocus}>Orbit Focus</ToggleButton>
                <ToggleButton value={CameraControlsType.OrbitFree}>Orbit Free</ToggleButton>
            </ToggleButtonGroup>
        </Panel>
    )
}

export default CameraSelectionPanel