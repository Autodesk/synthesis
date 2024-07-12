import * as THREE from "three"
import { useEffect, useRef, useState } from "react"
import { FaGear } from "react-icons/fa6"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import TransformGizmos from "@/ui/components/TransformGizmos"
import Slider from "@/ui/components/Slider"
import Jolt from "@barclah/jolt-physics"
import Label from "@/ui/components/Label"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Button from "@/ui/components/Button"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import { MiraType } from "@/mirabuf/MirabufLoader"
import JOLT from "@/util/loading/JoltSyncLoader"
import { JoltQuat_ThreeQuaternion, ThreeQuaternion_JoltQuat } from "@/util/TypeConversions"

// slider constants
const MIN_VELOCITY = 0.1
const MAX_VELOCITY = 1.0

const ConfigureShotTrajectoryPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    // const [, setNode] = useState<string>("Click to select")
    const transformGizmoRef = useRef<TransformGizmos>()
    const bodyAttachmentRef = useRef<Jolt.Body>()

    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)

    // creating mesh & gizmo for the pickup node
    const setupGizmo = () => {
        if (!selectedRobot?.ejectorPreferences) return

        if (transformGizmoRef.current == undefined) {
            transformGizmoRef.current = new TransformGizmos(
                new THREE.Mesh(new THREE.BoxGeometry(0.25, 1.0, 0.25), new THREE.MeshBasicMaterial({ color: 0xffffff }))
            )
            transformGizmoRef.current.AddMeshToScene()
            transformGizmoRef.current.CreateGizmo("translate", 1.5)
            transformGizmoRef.current.CreateGizmo("rotate", 2.0)
        }

        const robotPosition = World.PhysicsSystem.GetBody(selectedRobot.GetRootNodeId()!).GetPosition()
        const theta = calculateRobotAngle()

        // Re-calculating the position of the pickup node in relation to the robot based on the robot's local rotation and position
        const calculatedX =
            -Math.cos(theta) * selectedRobot.ejectorPreferences.position[0] +
            Math.sin(theta) * selectedRobot.ejectorPreferences.position[2]
        const calculatedZ =
            Math.sin(theta) * selectedRobot.ejectorPreferences.position[0] +
            Math.cos(theta) * selectedRobot.ejectorPreferences.position[2]

        // Calculating the position of the pickup mesh relative to the robot
        const position = [
            robotPosition.GetX() + calculatedX,
            robotPosition.GetY() + selectedRobot.ejectorPreferences.position[1],
            robotPosition.GetZ() + calculatedZ,
        ]
        const direction = selectedRobot.ejectorPreferences.direction

        transformGizmoRef.current?.mesh.position.set(position[0], position[1], position[2])
        transformGizmoRef.current?.mesh.rotation.setFromVector3(new THREE.Vector3(1, 1, 1))
        transformGizmoRef.current?.mesh.rotation.setFromQuaternion(
            new THREE.Quaternion(direction[0], direction[1], direction[2], direction[3])
        )

        // setting the rotation of the mesh in relation to the robot
        // transformGizmoRef.current?.mesh.rotateY(calculateRobotAngle() - selectedRobot.ejectorPreferences.relativeRotation)
        // const rotation = ThreeQuaternion_JoltQuat(transformGizmoRef.current!.mesh.quaternion)
        // rotation.SetY(calculateMeshAngle() - selectedRobot.ejectorPreferences.relativeRotation)
        // rotation.SetY(Math.PI)
        // transformGizmoRef.current?.mesh.rotation.setFromQuaternion(JoltQuat_ThreeQuaternion(rotation))
    }

    // Saves zone preferences to local storage
    const saveEjectorPreferences = () => {
        if (!selectedRobot?.ejectorPreferences) return

        const position = transformGizmoRef.current?.mesh.position
        const direction = transformGizmoRef.current?.mesh.quaternion
        const robotPosition = World.PhysicsSystem.GetBody(selectedRobot.GetRootNodeId()!).GetPosition()
        const theta = calculateRobotAngle()

        if (position == undefined || direction == undefined) return

        // resetting the position of the pickup node in relation to the robot at the default position it faces
        const calculatedX =
            Math.cos(theta) * (position.x - robotPosition.GetX()) -
            Math.sin(theta) * (position.z - robotPosition.GetZ())
        const calculatedZ =
            Math.sin(theta) * (position.x - robotPosition.GetX()) +
            Math.cos(theta) * (position.z - robotPosition.GetZ())

        selectedRobot.ejectorPreferences.position = [calculatedX, position.y - robotPosition.GetY(), calculatedZ]
        selectedRobot.ejectorPreferences.direction = [direction.x, direction.y, direction.z, direction.w]
        selectedRobot.ejectorPreferences.relativeRotation = theta

        selectedRobot.ejectorPreferences.parentBody = bodyAttachmentRef.current

        PreferencesSystem.savePreferences()
    }

    /**
     * @returns The angle of the robot in radians
     */
    const calculateRobotAngle = (): number => {
        const robotRotation = World.PhysicsSystem.GetBody(selectedRobot!.GetRootNodeId()!)
            .GetRotation()
            .GetRotationAngle(new JOLT.Vec3(0, 1, 0)) // getting the rotation of the robot on the Y axis
        if (robotRotation > 0) {
            return robotRotation
        } else {
            return 2 * Math.PI + robotRotation
        }
    }

    const calculateMeshAngle = (): number => {
        const meshRotation = ThreeQuaternion_JoltQuat(transformGizmoRef.current!.mesh.quaternion).GetRotationAngle(
            new JOLT.Vec3(0, 1, 0)
        )
        if (meshRotation > 0) {
            return meshRotation
        } else {
            return 2 * Math.PI + meshRotation
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
            {selectedRobot?.ejectorPreferences == undefined ? (
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
                    {/* Button for user to select the parent node */}
                    <SelectButton
                        placeholder="Select pickup node"
                        onSelect={(body: Jolt.Body) => checkSelectedNode(body)}
                    />

                    {/* Slider for user to set velocity of ejector configuration */}
                    <Slider
                        min={MIN_VELOCITY}
                        max={MAX_VELOCITY}
                        defaultValue={selectedRobot.ejectorPreferences.ejectorVelocity}
                        label="Velocity"
                        format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                        onChange={(vel: number) => {
                            if (selectedRobot.ejectorPreferences) {
                                selectedRobot.ejectorPreferences.ejectorVelocity = vel
                            }
                        }}
                        step={0.01}
                    />
                </>
            )}
        </Panel>
    )
}

export default ConfigureShotTrajectoryPanel
