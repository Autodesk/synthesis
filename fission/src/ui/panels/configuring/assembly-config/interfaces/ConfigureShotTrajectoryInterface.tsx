import * as THREE from "three"
import { useCallback, useEffect, useMemo, useState, useRef } from "react"
import SelectButton from "@/components/SelectButton"
import Slider from "@/ui/components/Slider"
import Jolt from "@barclah/jolt-physics"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import {
    Array_ThreeMatrix4,
    JoltMat44_ThreeMatrix4,
    ReactRgbaColor_ThreeColor,
    ThreeMatrix4_Array,
} from "@/util/TypeConversions"
import { useTheme } from "@/ui/ThemeContext"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import { ConfigurationSavedEvent } from "../ConfigurationSavedEvent"
import Button from "@/ui/components/Button"
import { Spacer } from "@/ui/components/StyledComponents"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"

// slider constants
const MIN_VELOCITY = 0.0
const MAX_VELOCITY = 20.0

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
function save(
    ejectorVelocity: number,
    gizmo: GizmoSceneObject,
    selectedRobot: MirabufSceneObject,
    selectedNode?: RigidNodeId
) {
    if (!selectedRobot?.ejectorPreferences || !gizmo) {
        return
    }

    selectedNode ??= selectedRobot.rootNodeId

    const nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedNode)
    if (!nodeBodyId) {
        return
    }

    const gizmoTransformation = gizmo.obj.matrixWorld
    const robotTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
    const deltaTransformation = gizmoTransformation.premultiply(robotTransformation.invert())

    selectedRobot.ejectorPreferences.deltaTransformation = ThreeMatrix4_Array(deltaTransformation)
    selectedRobot.ejectorPreferences.parentNode = selectedNode
    selectedRobot.ejectorPreferences.ejectorVelocity = ejectorVelocity

    PreferencesSystem.savePreferences()
}

interface ConfigEjectorProps {
    selectedRobot: MirabufSceneObject
}

const ConfigureShotTrajectoryInterface: React.FC<ConfigEjectorProps> = ({ selectedRobot }) => {
    const { currentTheme, themes } = useTheme()
    const theme = useMemo(() => {
        return themes[currentTheme]
    }, [currentTheme, themes])

    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(undefined)
    const [ejectorVelocity, setEjectorVelocity] = useState<number>((MIN_VELOCITY + MAX_VELOCITY) / 2.0)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedRobot) {
            save(ejectorVelocity, gizmoRef.current, selectedRobot, selectedNode)
            const currentGp = selectedRobot.activeEjectable
            selectedRobot.SetEjectable(undefined, true)
            selectedRobot.SetEjectable(currentGp)
        }
    }, [selectedRobot, selectedNode, ejectorVelocity])

    useEffect(() => {
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
    }, [saveEvent])

    const placeholderMesh = useMemo(() => {
        return new THREE.Mesh(
            new THREE.ConeGeometry(0.1, 0.4, 4).rotateX(Math.PI / 2.0).translate(0, 0, 0.2),
            World.SceneRenderer.CreateToonMaterial(ReactRgbaColor_ThreeColor(theme.HighlightHover.color))
        )
    }, [theme])

    const gizmoComponent = useMemo(() => {
        if (selectedRobot?.ejectorPreferences) {
            const postGizmoCreation = (gizmo: GizmoSceneObject) => {
                ((gizmo.obj as THREE.Mesh).material as THREE.Material).depthTest = false

                const deltaTransformation = Array_ThreeMatrix4(selectedRobot.ejectorPreferences!.deltaTransformation)

                let nodeBodyId = selectedRobot.mechanism.nodeToBody.get(
                    selectedRobot.ejectorPreferences!.parentNode ?? selectedRobot.rootNodeId
                )
                if (!nodeBodyId) {
                    // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
                    nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedRobot.rootNodeId)!
                }

                /** W = L x R. See save() for math details */
                const robotTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
                const gizmoTransformation = deltaTransformation.premultiply(robotTransformation)

                gizmo.obj.position.setFromMatrixPosition(gizmoTransformation)
                gizmo.obj.rotation.setFromRotationMatrix(gizmoTransformation)
            }

            return (<TransformGizmoControl
                key="shot-transform-gizmo"
                size={1.5}
                gizmoRef={gizmoRef}
                defaultMode="translate"
                defaultMesh={placeholderMesh}
                scaleDisabled={true}
                postGizmoCreation={postGizmoCreation}
            />)
        } else {
            gizmoRef.current = undefined
            return (<></>)
        }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [placeholderMesh, selectedRobot.ejectorPreferences])

    useEffect(() => {
        if (selectedRobot?.ejectorPreferences) {
            setEjectorVelocity(selectedRobot.ejectorPreferences.ejectorVelocity)
            setSelectedNode(selectedRobot.ejectorPreferences.parentNode)
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
                min={MIN_VELOCITY}
                max={MAX_VELOCITY}
                value={ejectorVelocity}
                label="Velocity"
                format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                onChange={(_, vel: number | number[]) => {
                    setEjectorVelocity(vel as number)
                }}
                step={0.01}
            />
            {gizmoComponent}
            {Spacer(10)}
            <Button
                value="Reset"
                onClick={() => {
                    if (gizmoRef.current) {
                        const robotTransformation = JoltMat44_ThreeMatrix4(
                            World.PhysicsSystem.GetBody(selectedRobot.GetRootNodeId()!).GetWorldTransform()
                        )
                        gizmoRef.current.obj.position.setFromMatrixPosition(robotTransformation)
                        gizmoRef.current.obj.rotation.setFromRotationMatrix(robotTransformation)
                    }
                    setEjectorVelocity(1)
                    setSelectedNode(selectedRobot?.rootNodeId)
                }}
            />
        </>
    )
}

export default ConfigureShotTrajectoryInterface
