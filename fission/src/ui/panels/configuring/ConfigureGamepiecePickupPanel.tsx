import * as THREE from "three"
import { useEffect, useState, useRef } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import TransformGizmos from "@/ui/components/TransformGizmos"
import World from "@/systems/World"

const ConfigureGamepiecePickupPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [, setNode] = useState<string>("Click to select")
    const transformGizmoRef = useRef<TransformGizmos>()

    useEffect(() => {
        // implementing spherical placeholder for intake placement
        transformGizmoRef.current = new TransformGizmos(
            World.SceneRenderer.CreateSphere(0.5, World.SceneRenderer.CreateToonMaterial(new THREE.Color(0xffffff)))
        )
        transformGizmoRef.current.AddMeshToScene()
        transformGizmoRef.current.CreateGizmo("scale")
        transformGizmoRef.current.CreateGizmo("translate")
    }, [])

    return (
        <Panel
            name="Configure Pickup"
            icon={<FaGear />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                // send zone config
            }}
            onCancel={() => {
                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()
            }}
        >
            <SelectButton onSelect={setNode} placeholder="Select pickup node" />
        </Panel>
    )
}

export default ConfigureGamepiecePickupPanel
