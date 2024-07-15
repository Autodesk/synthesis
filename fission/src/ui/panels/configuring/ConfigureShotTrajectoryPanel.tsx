import * as THREE from "three"
import { useEffect, useRef, useState } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import TransformGizmos from "@/ui/components/TransformGizmos"
import Slider from "@/ui/components/Slider"
import Jolt from "@barclah/jolt-physics"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Label from "@/ui/components/Label"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { EjectorPreferences } from "@/systems/preferences/PreferenceTypes"
import Button from "@/ui/components/Button"

// slider constants
const MIN_VELOCITY = 0.1
const MAX_VELOCITY = 1.0

const ConfigureShotTrajectoryPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    // const [, setNode] = useState<string>("Click to select")
    const transformGizmoRef = useRef<TransformGizmos>()
    const bodyAttachmentRef = useRef<Jolt.Body>()

    const [selectedEjector, setSelectedEjector] = useState<EjectorPreferences | undefined>(undefined)
    const [ejectorVelocity, setEjectorVelocity] = useState<number>(0)

    // creating mesh & gizmo for the pickup node
    const setupGizmo = () => {
        if (selectedEjector == undefined) return

        if (transformGizmoRef.current == undefined) {
            transformGizmoRef.current = new TransformGizmos(
                new THREE.Mesh(new THREE.BoxGeometry(0.5, 2.0), new THREE.MeshBasicMaterial({ color: 0xffffff }))
            )
            transformGizmoRef.current.AddMeshToScene()
            transformGizmoRef.current.CreateGizmo("translate", 1.5)
            transformGizmoRef.current.CreateGizmo("rotate", 2.0)
        }

        const position = selectedEjector.position
        const direction = selectedEjector.direction

        transformGizmoRef.current?.mesh.position.set(position[0], position[1], position[2])
        transformGizmoRef.current?.mesh.rotation.setFromQuaternion(
            new THREE.Quaternion(direction[0], direction[1], direction[2], direction[3])
        )
    }

    // Saves zone preferences to local storage
    const saveEjectorPreferences = () => {
        if (selectedEjector == undefined) return

        const position = transformGizmoRef.current?.mesh.position
        const direction = transformGizmoRef.current?.mesh.quaternion

        if (position == undefined || direction == undefined) return

        selectedEjector.ejectorVelocity = ejectorVelocity
        selectedEjector.position = [position.x, position.y, position.z]
        selectedEjector.direction = [direction.x, direction.y, direction.z, direction.w]

        selectedEjector.parentBody = bodyAttachmentRef.current

        PreferencesSystem.savePreferences()
    }

    useEffect(() => {
        setupGizmo()
    }, [])

    return (
        <Panel
            name="Configure Ejector"
            icon={<FaGear />}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()

                saveEjectorPreferences()
            }}
            onCancel={() => {
                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()
            }}
        >
            {selectedEjector == undefined ? (
                <>
                    <Label>Select a robot</Label>
                    {/** Scroll view for selecting a robot to configure */}
                    <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[40vh] bg-background-secondary rounded-md p-2">
                        {Object.keys(SynthesisBrain.numberRobotsSpawned).map(robot => {
                            return (
                                <Button
                                    value={robot}
                                    onClick={() => {
                                        setSelectedEjector(PreferencesSystem.getRobotPreferences(robot)?.ejector)
                                        setEjectorVelocity(
                                            PreferencesSystem.getRobotPreferences(robot)?.ejector.ejectorVelocity ??
                                                MIN_VELOCITY
                                        )
                                    }}
                                    key={robot}
                                ></Button>
                            )
                        })}
                    </div>
                </>
            ) : (
                <>
                    {/* Button for user to select the parent node */}
                    <SelectButton
                        placeholder="Select pickup node"
                        onSelect={(body: Jolt.Body) => (bodyAttachmentRef.current = body)}
                    />

                    {/* Slider for user to set velocity of ejector configuration */}
                    <Slider
                        min={MIN_VELOCITY}
                        max={MAX_VELOCITY}
                        defaultValue={selectedEjector.ejectorVelocity}
                        label="Velocity"
                        format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                        onChange={(vel: number) => setEjectorVelocity(vel)}
                        step={0.01}
                    />
                </>
            )}
        </Panel>
    )
}

export default ConfigureShotTrajectoryPanel
