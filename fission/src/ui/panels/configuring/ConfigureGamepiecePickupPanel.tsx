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
import { MiraType } from "@/mirabuf/MirabufLoader"
import JOLT from "@/util/loading/JoltSyncLoader"

// slider constants
const MIN_ZONE_SIZE = 0.1
const MAX_ZONE_SIZE = 1.0

const ConfigureGamepiecePickupPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const transformGizmoRef = useRef<TransformGizmos>()
    const bodyAttachmentRef = useRef<Jolt.Body>()

    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)

    /**
     * Creating a mesh to mimic the pickup node
     * - adds a transform gizmo to the Mesh
     * - sets the position of the mesh in relation to the position and rotation of the robot
     * - sets the scale of the mesh to the previously saved configuration
     */
    const setupGizmo = (): void => {
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

        // Re-calculating the position of the pickup node in relation to the robot based on the robot's local rotation and position
        const calculatedX =
            -Math.cos(theta) * selectedRobot.intakePreferences.position[0] +
            Math.sin(theta) * selectedRobot.intakePreferences.position[2]
        const calculatedZ =
            Math.sin(theta) * selectedRobot.intakePreferences.position[0] +
            Math.cos(theta) * selectedRobot.intakePreferences.position[2]

        // Calculating the position of the pickup mesh relative to the robot
        const position = [
            robotPosition.GetX() + calculatedX,
            robotPosition.GetY() + selectedRobot.intakePreferences.position[1],
            robotPosition.GetZ() + calculatedZ,
        ]

        transformGizmoRef.current.mesh.scale.set(scale, scale, scale)
        transformGizmoRef.current.mesh.position.set(position[0], position[1], position[2])
    }

    /**
     * Saves pickup configuration preferences to local storage
     */
    const savePickupPreferences = (): void => {
        if (!selectedRobot?.intakePreferences) return

        const scale = transformGizmoRef.current?.mesh.scale
        const position = transformGizmoRef.current?.mesh.position
        const robotPosition = World.PhysicsSystem.GetBody(selectedRobot.GetRootNodeId()!).GetPosition()

        if (scale == undefined || position == undefined) return

        selectedRobot.intakePreferences.diameter = scale.x
        const theta = calculateRobotAngle()

        // resetting the position of the pickup node in relation to the robot at the default position it faces
        const calculatedX =
            Math.cos(theta) * (position.x - robotPosition.GetX()) -
            Math.sin(theta) * (position.z - robotPosition.GetZ())
        const calculatedZ =
            Math.sin(theta) * (position.x - robotPosition.GetX()) +
            Math.cos(theta) * (position.z - robotPosition.GetZ())

        selectedRobot.intakePreferences.position = [calculatedX, position.y - robotPosition.GetY(), calculatedZ]
        selectedRobot.intakePreferences.parentBody = bodyAttachmentRef.current

        PreferencesSystem.savePreferences()
    }

    /**
     * @returns The angle of the robot in radians
     */
    const calculateRobotAngle = (): number => {
        const robotRotation = World.PhysicsSystem.GetBody(selectedRobot!.GetRootNodeId()!).GetRotation().GetRotationAngle(new JOLT.Vec3(0, 1, 0)) // getting the rotation of the robot on the Y axis
        if (robotRotation > 0) {
            return robotRotation
        } else {
            return 2 * Math.PI + robotRotation
        }
    }

    /**
     * @returns A list of all robots as MirabufSceneObjects
     */
    const listRobots = (): MirabufSceneObject[] => {
        // filtering out robots that are not dynamic and not MirabufSceneObjects
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]
        return assemblies
    }

    /**
     * Checks if the body is a child of the selected MirabufSceneObject
     *
     * @param body The Jolt body to check if it is a child of the selected robot
     */
    const checkSelectedNode = (body: Jolt.Body): boolean => {
        let returnValue = false
        selectedRobot?.mirabufInstance?.parser.rigidNodes.forEach(rn => {
            if (World.PhysicsSystem.GetBody(selectedRobot.mechanism.GetBodyByNodeId(rn.id)!).GetID() === body.GetID()) {
                bodyAttachmentRef.current = body
                returnValue = true
            }
        })
        return returnValue
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

                savePickupPreferences()
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
                        onSelect={(body: Jolt.Body) => checkSelectedNode(body)}
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
