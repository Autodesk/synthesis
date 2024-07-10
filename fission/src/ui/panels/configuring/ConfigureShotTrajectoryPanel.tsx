import { useEffect, useRef, useState } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import Slider from "@/components/Slider"
import Jolt from "@barclah/jolt-physics"
import World from "@/systems/World"
import TransformGizmos from "@/ui/components/TransformGizmos"
import * as THREE from "three"

const ConfigureShotTrajectoryPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const defaultShootSpeed = 5
    const [, setShootSpeed] = useState<number>(defaultShootSpeed)
    const transformGizmoRef = useRef<TransformGizmos>()
    const bodyAttachmentRef = useRef<Jolt.Body>()

    // creating mesh & gizmo for the Ejector node
    useEffect(() => {
        transformGizmoRef.current = new TransformGizmos(
            new THREE.Mesh(new THREE.BoxGeometry(0.5, 2.0), new THREE.MeshBasicMaterial({ color: 0xffffff }))
        )
        transformGizmoRef.current.AddMeshToScene()
        transformGizmoRef.current.CreateGizmo("translate", 1.5)
        transformGizmoRef.current.CreateGizmo("rotate", 2.0)
    }, [])

    return (
        <Panel
            name="Configure Shooting"
            icon={<FaGear />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                // send node and speed config
                if (bodyAttachmentRef.current && transformGizmoRef.current) {
                    World.SimulationSystem.AddPickupConfiguration(
                        bodyAttachmentRef.current,
                        transformGizmoRef.current.mesh.position
                    )
                }

                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()
            }}
            onCancel={() => {
                // cancel node and speed config

                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()
            }}
        >
            <SelectButton
                placeholder="Select shoot node"
                onSelect={(body: Jolt.Body) => (bodyAttachmentRef.current = body)}
            />
            <Slider
                min={0}
                max={10}
                step={0.1}
                defaultValue={defaultShootSpeed}
                label="Speed"
                format={{ maximumFractionDigits: 2 }}
                onChange={setShootSpeed}
            />
        </Panel>
    )
}

export default ConfigureShotTrajectoryPanel
