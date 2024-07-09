import * as THREE from "three"
import { useEffect, useRef } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import TransformGizmos from "@/ui/components/TransformGizmos"
import World from "@/systems/World"
import Slider from "@/ui/components/Slider"
import Jolt from "@barclah/jolt-physics"

// slider constants
const MIN_ZONE_SIZE = 0.1
const MAX_ZONE_SIZE = 1.0
const DEFAULT_ZONE_SIZE = 1.0 // default zone size

const ConfigureGamepiecePickupPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    // const [, setNode] = useState<string>("Click to select")
    const transformGizmoRef = useRef<TransformGizmos>()
    const bodyAttachmentRef = useRef<Jolt.Body>()

    // creating mesh & gizmo for the pickup node
    useEffect(() => {
        transformGizmoRef.current = new TransformGizmos(
            World.SceneRenderer.CreateSphere(0.5, World.SceneRenderer.CreateToonMaterial(new THREE.Color(0xffffff)))
        )
        transformGizmoRef.current.AddMeshToScene()
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
                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()

                // send configuration information to APS + RAM
                if (bodyAttachmentRef.current && transformGizmoRef.current) {
                    World.SimulationSystem.AddPickupConfiguration(
                        bodyAttachmentRef.current,
                        transformGizmoRef.current.mesh.position
                    )
                }
            }}
            onCancel={() => {
                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()
            }}
        >
            {/* Button for user to select pickup node */}
            <SelectButton
                placeholder="Select pickup node"
                onSelect={(body: Jolt.Body) => (bodyAttachmentRef.current = body)}
            />

            {/* Slider for user to set size of pickup configuration */}
            <Slider
                min={MIN_ZONE_SIZE}
                max={MAX_ZONE_SIZE}
                defaultValue={DEFAULT_ZONE_SIZE}
                label="Zone Size"
                format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                onChange={(size: number) => {
                    transformGizmoRef.current?.mesh.scale.set(size, size, size)
                }}
                step={0.01}
            />
        </Panel>
    )
}

export default ConfigureGamepiecePickupPanel
