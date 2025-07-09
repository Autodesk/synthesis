import * as THREE from "three"
import { useCallback, useEffect, useMemo, useState, useRef } from "react"
import SelectButton from "@/components/SelectButton"
import Slider from "@/ui/components/Slider"
import Jolt from "@azaleacolburn/jolt-physics"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import {
    arrayThreeMatrix4,
    joltMat44ThreeMatrix4,
    reactRgbaColorThreeColor,
    threeMatrix4Array,
} from "@/util/TypeConversions"
import { useTheme } from "@/ui/helpers/UseThemeHelpers"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import { ConfigurationSavedEvent } from "../ConfigurationSavedEvent"
import Button from "@/ui/components/Button"
import { LabelWithTooltip, Spacer } from "@/ui/components/StyledComponents"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsSystem"
import { ToggleButtonGroup, ToggleButton } from "@/ui/components/ToggleButtonGroup"

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
    selectedNode?: RigidNodeId,
    ejectOrder?: "FIFO" | "LIFO"
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
    const robotTransformation = joltMat44ThreeMatrix4(World.physicsSystem.getBody(nodeBodyId).GetWorldTransform())
    const deltaTransformation = gizmoTransformation.premultiply(robotTransformation.invert())

    selectedRobot.ejectorPreferences.deltaTransformation = threeMatrix4Array(deltaTransformation)
    selectedRobot.ejectorPreferences.parentNode = selectedNode
    selectedRobot.ejectorPreferences.ejectorVelocity = ejectorVelocity

    selectedRobot.ejectorPreferences.ejectOrder = ejectOrder!

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
    const [ejectOrder, setEjectOrder] = useState<"FIFO" | "LIFO">(
        selectedRobot.ejectorPreferences?.ejectOrder || "FIFO"
    )

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedRobot) {
            save(ejectorVelocity, gizmoRef.current, selectedRobot, selectedNode, ejectOrder)
            const currentGp = selectedRobot.activeEjectables[0]
            selectedRobot.setEjectable(undefined)
            selectedRobot.setEjectable(currentGp)
        }
    }, [selectedRobot, selectedNode, ejectorVelocity, ejectOrder])

    useEffect(() => {
        ConfigurationSavedEvent.listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.removeListener(saveEvent)
        }
    }, [saveEvent])

    const placeholderMesh = useMemo(() => {
        return new THREE.Mesh(
            new THREE.ConeGeometry(0.1, 0.4, 4).rotateX(Math.PI / 2.0).translate(0, 0, 0.2),
            World.sceneRenderer.createToonMaterial(reactRgbaColorThreeColor(theme.HighlightHover.color))
        )
    }, [theme])

    const gizmoComponent = useMemo(() => {
        if (selectedRobot?.ejectorPreferences) {
            const postGizmoCreation = (gizmo: GizmoSceneObject) => {
                const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
                material.depthTest = false

                const deltaTransformation = arrayThreeMatrix4(selectedRobot.ejectorPreferences!.deltaTransformation)

                let nodeBodyId = selectedRobot.mechanism.nodeToBody.get(
                    selectedRobot.ejectorPreferences!.parentNode ?? selectedRobot.rootNodeId
                )
                if (!nodeBodyId) {
                    // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
                    nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedRobot.rootNodeId)!
                }

                /** W = L x R. See save() for math details */
                const robotTransformation = joltMat44ThreeMatrix4(
                    World.physicsSystem.getBody(nodeBodyId).GetWorldTransform()
                )
                const gizmoTransformation = deltaTransformation.premultiply(robotTransformation)

                gizmo.obj.position.setFromMatrixPosition(gizmoTransformation)
                gizmo.obj.rotation.setFromRotationMatrix(gizmoTransformation)
            }

            return (
                <TransformGizmoControl
                    key="shot-transform-gizmo"
                    size={1.5}
                    gizmoRef={gizmoRef}
                    defaultMode="translate"
                    defaultMesh={placeholderMesh}
                    scaleDisabled={true}
                    postGizmoCreation={postGizmoCreation}
                />
            )
        } else {
            gizmoRef.current = undefined
            return <></>
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [placeholderMesh, selectedRobot.ejectorPreferences])

    useEffect(() => {
        if (selectedRobot?.ejectorPreferences) {
            setEjectorVelocity(selectedRobot.ejectorPreferences.ejectorVelocity)
            setSelectedNode(selectedRobot.ejectorPreferences.parentNode)
            setEjectOrder(selectedRobot.ejectorPreferences.ejectOrder)
        } else {
            setSelectedNode(undefined)
        }
    }, [selectedRobot])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)

        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])

    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            if (!selectedRobot) {
                return false
            }

            const assoc = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate
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

            {/* Toggle for adjusting eject order */}
            <div className="mt-4 flex items-center space-x-2">
                {LabelWithTooltip(
                    "Eject Order",
                    "Choose how to eject pieces: FIFO (first in, first out) ejects the oldest-loaded item first, or LIFO (last in, first out) ejects the most recently loaded item first."
                )}
                <ToggleButtonGroup
                    value={ejectOrder}
                    exclusive
                    onChange={(_, v) => v && setEjectOrder(v as "FIFO" | "LIFO")}
                >
                    <ToggleButton value="FIFO">FIFO</ToggleButton>
                    <ToggleButton value="LIFO">LIFO</ToggleButton>
                </ToggleButtonGroup>
            </div>

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

            {Spacer(10)}
            {gizmoComponent}
            {Spacer(10)}
            <Button
                value="Reset"
                onClick={() => {
                    if (gizmoRef.current) {
                        const robotTransformation = joltMat44ThreeMatrix4(
                            World.physicsSystem.getBody(selectedRobot.getRootNodeId()!).GetWorldTransform()
                        )
                        gizmoRef.current.obj.position.setFromMatrixPosition(robotTransformation)
                        gizmoRef.current.obj.rotation.setFromRotationMatrix(robotTransformation)
                    }
                    setEjectorVelocity(1)
                    setSelectedNode(selectedRobot?.rootNodeId)
                    setEjectOrder(selectedRobot.ejectorPreferences?.ejectOrder ?? "FIFO")
                }}
            />
        </>
    )
}

export default ConfigureShotTrajectoryInterface
