import * as THREE from "three"
import { useEffect, useRef, useState } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import TransformGizmos from "@/ui/components/TransformGizmos"
import World from "@/systems/World"
import Slider from "@/ui/components/Slider"
import Jolt from "@barclah/jolt-physics"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Label from "@/ui/components/Label"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { IntakePreferences } from "@/systems/preferences/PreferenceTypes"
import Button from "@/ui/components/Button"

// slider constants
const MIN_ZONE_SIZE = 0.1
const MAX_ZONE_SIZE = 1.0

const ConfigureGamepiecePickupPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    // const [, setNode] = useState<string>("Click to select")
    const transformGizmoRef = useRef<TransformGizmos>()
    const bodyAttachmentRef = useRef<Jolt.Body>()

    const [selectedZone, setSelectedZone] = useState<IntakePreferences | undefined>(undefined)

    // creating mesh & gizmo for the pickup node
    const setupGizmo = () => {
        if (selectedZone == undefined) return

        if (transformGizmoRef.current == undefined) {
            transformGizmoRef.current = new TransformGizmos(
                World.SceneRenderer.CreateSphere(0.5, World.SceneRenderer.CreateToonMaterial(new THREE.Color(0xffffff)))
            )
            transformGizmoRef.current.AddMeshToScene()
            transformGizmoRef.current.CreateGizmo("translate")
        }

        const scale = selectedZone.diameter
        const position = selectedZone.position

        transformGizmoRef.current?.mesh.scale.set(scale, scale, scale)
        transformGizmoRef.current?.mesh.position.set(position[0], position[1], position[2])
    }

    // Saves zone preferences to local storage
    const saveZonePreferences = () => {
        if (selectedZone == undefined) return

        const scale = transformGizmoRef.current?.mesh.scale
        const position = transformGizmoRef.current?.mesh.position

        if (scale == undefined || position == undefined) return

        selectedZone.diameter = scale.x
        selectedZone.position = [position.x, position.y, position.z]
        
        selectedZone.parentBody = bodyAttachmentRef.current

        PreferencesSystem.savePreferences()
    }

    useEffect(() => {
       setupGizmo()
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

                saveZonePreferences()
            }}
            onCancel={() => {
                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()
            }}
        >
            {selectedZone == undefined ? (
                <>
                    <Label>Select a robot</Label>
                    {/** Scroll view for selecting a robot to configure */}
                    <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[40vh] bg-background-secondary rounded-md p-2">
                        {Object.keys(SynthesisBrain.numberRobotsSpawned).map(robot => {
                            return (
                                <Button
                                    value={robot}
                                    onClick={() => {
                                        setSelectedZone(PreferencesSystem.getRobotPreferences(robot)?.intake)
                                    }}
                                    key={robot}
                                ></Button>
                            )
                        })}
                    </div>
                </>
            ) : (
                <>
                    {/* Button for user to select pickup node */}
                    <SelectButton
                        placeholder="Select pickup node"
                        onSelect={(body: Jolt.Body) => (bodyAttachmentRef.current = body)}
                    />

                    {/* Slider for user to set size of pickup configuration */}
                    <Slider
                        min={MIN_ZONE_SIZE}
                        max={MAX_ZONE_SIZE}
                        defaultValue={selectedZone.diameter}
                        label="Zone Size"
                        format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                        onChange={(size: number) => {
                            transformGizmoRef.current?.mesh.scale.set(size, size, size)
                        }}
                        step={0.01}
                    />
                </>
            )}
        </Panel>
    )
}

export default ConfigureGamepiecePickupPanel
