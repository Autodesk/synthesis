import * as THREE from "three"
import { useEffect, useRef, useState } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import TransformGizmos from "@/ui/components/TransformGizmos"
import World from "@/systems/World"
import Slider from "@/ui/components/Slider"
import Jolt from "@barclah/jolt-physics"
import Label from "@/ui/components/Label"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Button from "@/ui/components/Button"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

// slider constants
const MIN_ZONE_SIZE = 0.1
const MAX_ZONE_SIZE = 1.0

const ConfigureGamepiecePickupPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const transformGizmoRef = useRef<TransformGizmos>()
    const bodyAttachmentRef = useRef<Jolt.Body>()

    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)

    // creating mesh & gizmo for the pickup node
    const setupGizmo = () => {
        if (!selectedRobot?.intakePreferences) return

        if (transformGizmoRef.current == undefined) {
            transformGizmoRef.current = new TransformGizmos(
                World.SceneRenderer.CreateSphere(0.5, World.SceneRenderer.CreateToonMaterial(new THREE.Color(0xffffff)))
            )
            transformGizmoRef.current.AddMeshToScene()
            transformGizmoRef.current.CreateGizmo("translate")
        }

        /* setting position and scale of the mesh in relation to the position, scale, and rotation of the robot */
        const scale = selectedRobot.intakePreferences.diameter
        const robotPosition = World.PhysicsSystem.GetBody(selectedRobot.GetRootNodeId()!).GetPosition()
        const theta = calculateRobotAngle()
        
        const calculatedX = - Math.cos(theta) * selectedRobot.intakePreferences.position[0] + Math.sin(theta) * selectedRobot.intakePreferences.position[2]
        const calculatedZ = Math.sin(theta) * selectedRobot.intakePreferences.position[0] + Math.cos(theta) * selectedRobot.intakePreferences.position[2]

        const position = [
            robotPosition.GetX() + calculatedX,
            robotPosition.GetY() + selectedRobot.intakePreferences.position[1],
            robotPosition.GetZ() + calculatedZ,
        ]

        transformGizmoRef.current.mesh.scale.set(scale, scale, scale)
        transformGizmoRef.current.mesh.position.set(position[0], position[1], position[2])
    }

    // Saves zone preferences to local storage
    const saveZonePreferences = () => {
        if (!selectedRobot?.intakePreferences) return

        const scale = transformGizmoRef.current?.mesh.scale
        const position = transformGizmoRef.current?.mesh.position

        if (scale == undefined || position == undefined) return

        selectedRobot.intakePreferences.diameter = scale.x
        const robotPosition = World.PhysicsSystem.GetBody(selectedRobot.GetRootNodeId()!).GetPosition()

        selectedRobot.intakePreferences.rotation = calculateRobotAngle()
        selectedRobot.intakePreferences.position = [
            position.x - robotPosition.GetX(),
            position.y - robotPosition.GetY(),
            position.z - robotPosition.GetZ(),
        ]

        const theta = selectedRobot.intakePreferences.rotation
        const calculatedX = Math.cos(theta) * selectedRobot.intakePreferences.position[0] - Math.sin(theta) * selectedRobot.intakePreferences.position[2]
        const calculatedZ = Math.sin(theta) * selectedRobot.intakePreferences.position[0] + Math.cos(theta) * selectedRobot.intakePreferences.position[2]
        selectedRobot.intakePreferences.position = [
            calculatedX,
            selectedRobot.intakePreferences.position[1],
            calculatedZ
        ]

        selectedRobot.intakePreferences.parentBody = bodyAttachmentRef.current

        PreferencesSystem.savePreferences()
    }
    
    const calculateRobotAngle = () => {
        const robotRotationY = World.PhysicsSystem.GetBody(selectedRobot!.GetRootNodeId()!).GetRotation().GetEulerAngles().GetY()
        const robotRotationZ = Math.abs(World.PhysicsSystem.GetBody(selectedRobot!.GetRootNodeId()!).GetRotation().GetEulerAngles().GetZ())

        if (robotRotationY > 0 && robotRotationZ < 2) {
            return robotRotationY
        } else if (robotRotationY > 0 && robotRotationZ > 2) {
            return Math.PI - robotRotationY
        } else if (robotRotationY < 0 && robotRotationZ > 2) {
            return Math.PI - robotRotationY
        } else {
            return 2 * Math.PI + robotRotationY
        }
    }

    const listRobots = () => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            return x instanceof MirabufSceneObject
        })
        // .filter(x => {
        //     return (x as MirabufSceneObject).assemblyType == "robot"
        // })
        return assemblies
    }

    useEffect(() => {
        setupGizmo()
    })

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
            {selectedRobot?.intakePreferences == undefined ? (
                <>
                    <Label>Select a robot</Label>
                    {/** Scroll view for selecting a robot to configure */}
                    <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[40vh] bg-background-secondary rounded-md p-2">
                        {listRobots().map(mirabufSceneObject => {
                            return (
                                <Button
                                    value={mirabufSceneObject.assemblyName}
                                    onClick={() => {
                                        setSelectedRobot(mirabufSceneObject)
                                    }}
                                    key={mirabufSceneObject.id}
                                ></Button>
                            )
                        })}
                    </div>
                    {/* TODO: remove the accept button on this version */}
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
                        defaultValue={selectedRobot.intakePreferences.diameter}
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
