import * as THREE from "three"
import { useCallback, useEffect, useMemo, useState } from "react"
import SelectButton from "@/components/SelectButton"
import TransformGizmos from "@/ui/components/TransformGizmos"
import World from "@/systems/World"
import Slider from "@/ui/components/Slider"
import Jolt from "@barclah/jolt-physics"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import {
    Array_ThreeMatrix4,
    JoltMat44_ThreeMatrix4,
    ReactRgbaColor_ThreeColor,
    ThreeMatrix4_Array,
} from "@/util/TypeConversions"
import { useTheme } from "@/ui/ThemeContext"
import Button from "@/ui/components/Button"
import { Spacer } from "@/ui/components/StyledComponents"
import { ConfigurationSavedEvent } from "../ConfigurationSavedEvent"

// slider constants
const MIN_ZONE_SIZE = 0.1
const MAX_ZONE_SIZE = 1.0

/**
 * Saves ejector configuration to selected robot.
 *
 * Math Explanation:
 * Let W be the world transformation matrix of the gizmo.
 * Let R be the world transformation matrix of the selected robot node.
 * Let L be the local transformation matrix of the gizmo, relative to the selected robot node.
 *
 * We are given W and R, and want to save L with the robot. This way when we create
 * the ejection point afterwards, it will be relative to the selected robot node.
 *
 * W = L R
 * L = W R^(-1)
 *
 * ThreeJS sets the standard multiplication operation for matrices to be premultiply. I really
 * don't like this terminology as it's thrown me off multiple times, but I suppose it does go
 * against most other multiplication operations.
 *
 * @param ejectorVelocity Velocity to eject gamepiece at.
 * @param gizmo Reference to the transform gizmo object.
 * @param selectedRobot Selected robot to save data to.
 * @param selectedNode Selected node that configuration is relative to.
 */
function save(zoneSize: number, gizmo: TransformGizmos, selectedRobot: MirabufSceneObject, selectedNode?: RigidNodeId) {
    if (!selectedRobot?.intakePreferences || !gizmo) {
        return
    }

    selectedNode ??= selectedRobot.rootNodeId

    const nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedNode)
    if (!nodeBodyId) {
        return
    }

    const translation = new THREE.Vector3(0, 0, 0)
    const rotation = new THREE.Quaternion(0, 0, 0, 1)
    gizmo.mesh.matrixWorld.decompose(translation, rotation, new THREE.Vector3(1, 1, 1))

    const gizmoTransformation = new THREE.Matrix4().compose(translation, rotation, new THREE.Vector3(1, 1, 1))
    const robotTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
    const deltaTransformation = gizmoTransformation.premultiply(robotTransformation.invert())

    selectedRobot.intakePreferences.deltaTransformation = ThreeMatrix4_Array(deltaTransformation)
    selectedRobot.intakePreferences.parentNode = selectedNode
    selectedRobot.intakePreferences.zoneDiameter = zoneSize

    PreferencesSystem.savePreferences()
}

interface ConfigPickupProps {
    selectedRobot: MirabufSceneObject
}

const ConfigureGamepiecePickupInterface: React.FC<ConfigPickupProps> = ({ selectedRobot }) => {
    const { currentTheme, themes } = useTheme()
    const theme = useMemo(() => {
        return themes[currentTheme]
    }, [currentTheme, themes])

    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(undefined)
    const [zoneSize, setZoneSize] = useState<number>((MIN_ZONE_SIZE + MAX_ZONE_SIZE) / 2.0)
    const [transformGizmo, setTransformGizmo] = useState<TransformGizmos | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (transformGizmo && selectedRobot) {
            save(zoneSize, transformGizmo, selectedRobot, selectedNode)
            selectedRobot.UpdateIntakeSensor()
        }
    }, [transformGizmo, selectedRobot, selectedNode, zoneSize])

    useEffect(() => {
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
    }, [saveEvent])

    useEffect(() => {
        if (!transformGizmo) {
            return
        }

        transformGizmo.mesh.scale.set(zoneSize, zoneSize, zoneSize)
    }, [zoneSize, transformGizmo])

    // Not sure I like this, but made it a state and effect rather than a memo to add the cleanup to the end
    useEffect(() => {
        if (!selectedRobot?.intakePreferences) {
            setTransformGizmo(undefined)
            return
        }

        const gizmo = new TransformGizmos(
            new THREE.Mesh(
                new THREE.SphereGeometry(0.5),
                World.SceneRenderer.CreateToonMaterial(ReactRgbaColor_ThreeColor(theme.HighlightSelect.color))
            )
        )

        ;(gizmo.mesh.material as THREE.Material).depthTest = false
        gizmo.AddMeshToScene()
        gizmo.CreateGizmo("translate", 1.5)

        const deltaTransformation = Array_ThreeMatrix4(selectedRobot.intakePreferences.deltaTransformation)

        let nodeBodyId = selectedRobot.mechanism.nodeToBody.get(
            selectedRobot.intakePreferences.parentNode ?? selectedRobot.rootNodeId
        )
        if (!nodeBodyId) {
            // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
            nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedRobot.rootNodeId)!
        }

        /** W = L x R. See save() for math details */
        const robotTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
        const gizmoTransformation = deltaTransformation.premultiply(robotTransformation)

        gizmo.mesh.position.setFromMatrixPosition(gizmoTransformation)
        gizmo.mesh.rotation.setFromRotationMatrix(gizmoTransformation)

        setTransformGizmo(gizmo)

        return () => {
            console.log("remove gizmos")
            gizmo.RemoveGizmos()
            setTransformGizmo(undefined)
        }
    }, [selectedRobot, theme])

    useEffect(() => {
        if (selectedRobot?.intakePreferences) {
            setZoneSize(selectedRobot.intakePreferences.zoneDiameter)
            setSelectedNode(selectedRobot.intakePreferences.parentNode)
        } else {
            setSelectedNode(undefined)
        }
    }, [selectedRobot])

    useEffect(() => {
        World.PhysicsSystem.HoldPause()

        return () => {
            World.PhysicsSystem.ReleasePause()
        }
    }, [])

    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            if (!selectedRobot) {
                return false
            }

            const assoc = World.PhysicsSystem.GetBodyAssociation(body) as RigidNodeAssociate
            if (!assoc || !assoc.sceneObject || assoc.sceneObject != selectedRobot) {
                return false
            }

            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [selectedRobot]
    )

    return (
        <>
            {/* Button for user to select the parent node */}
            <SelectButton
                placeholder="Select parent node"
                value={selectedNode}
                onSelect={(body: Jolt.Body) => trySetSelectedNode(body.GetID())}
            />

            {/* Slider for user to set velocity of ejector configuration */}
            <Slider
                min={MIN_ZONE_SIZE}
                max={MAX_ZONE_SIZE}
                value={zoneSize}
                label="Zone Size"
                format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                onChange={(_, vel: number | number[]) => {
                    setZoneSize(vel as number)
                }}
                step={0.01}
            />
            {Spacer(10)}
            <Button
                value="Reset"
                onClick={() => {
                    if (transformGizmo) {
                        const robotTransformation = JoltMat44_ThreeMatrix4(
                            World.PhysicsSystem.GetBody(selectedRobot.GetRootNodeId()!).GetWorldTransform()
                        )
                        transformGizmo.mesh.position.setFromMatrixPosition(robotTransformation)
                        transformGizmo.mesh.rotation.setFromRotationMatrix(robotTransformation)
                    }
                    setZoneSize(0.5)
                    setSelectedNode(selectedRobot?.rootNodeId)
                }}
            />
        </>
    )
}

export default ConfigureGamepiecePickupInterface
