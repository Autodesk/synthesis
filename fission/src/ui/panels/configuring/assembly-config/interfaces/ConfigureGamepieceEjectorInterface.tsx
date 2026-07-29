import type Jolt from "@synthesis.adsk/jolt-physics"
import { Stack } from "@mui/material"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import * as THREE from "three"
import SelectButton from "@/components/SelectButton"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import StatefulSlider from "@/ui/components/StatefulSlider"
import { Button, LabelWithTooltip, Spacer, ToggleButton, ToggleButtonGroup } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertReactRgbaColorToThreeColor,
    convertThreeMatrix4ToArray,
} from "@/util/TypeConversions"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

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
    const robotTransformation = convertJoltMat44ToThreeMatrix4(
        World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
    )
    const deltaTransformation = gizmoTransformation.premultiply(robotTransformation.invert())

    selectedRobot.ejectorPreferences.deltaTransformation = convertThreeMatrix4ToArray(deltaTransformation)
    selectedRobot.ejectorPreferences.parentNode = selectedNode
    selectedRobot.ejectorPreferences.ejectorVelocity = ejectorVelocity

    selectedRobot.ejectorPreferences.ejectOrder = ejectOrder!

    selectedRobot.savePreferences()
}

const ConfigureGamepieceEjectorInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    registerCleanupFunction,
}) => {
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(undefined)
    const [ejectorVelocity, setEjectorVelocity] = useState<number>((MIN_VELOCITY + MAX_VELOCITY) / 2.0)
    const [ejectOrder, setEjectOrder] = useState<"FIFO" | "LIFO">(
        selectedAssembly.ejectorPreferences?.ejectOrder || "FIFO"
    )

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedAssembly) {
            save(ejectorVelocity, gizmoRef.current, selectedAssembly, selectedNode, ejectOrder)
            const currentGp = selectedAssembly.activeEjectables[0]
            selectedAssembly.setEjectable(undefined)
            selectedAssembly.setEjectable(currentGp)
        }
    }, [selectedAssembly, selectedNode, ejectorVelocity, ejectOrder])

    useEffect(() => {
        const originalPrefs = structuredClone(selectedAssembly.ejectorPreferences)
        registerCleanupFunction(undefined, () => {
            selectedAssembly.ejectorPreferences = originalPrefs
        })
    }, [registerCleanupFunction, selectedAssembly])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    const placeholderMesh = useMemo(() => {
        return new THREE.Mesh(
            new THREE.ConeGeometry(0.1, 0.4, 4).rotateX(Math.PI / 2.0).translate(0, 0, 0.2),
            // TODO: dynamic color
            World.sceneRenderer.createToonMaterial(
                convertReactRgbaColorToThreeColor({ r: 255, g: 255, b: 255, a: 255 })
            )
        )
    }, [])

    const gizmoComponent = useMemo(() => {
        if (selectedAssembly?.ejectorPreferences) {
            const postGizmoCreation = (gizmo: GizmoSceneObject) => {
                const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
                material.depthTest = false

                const deltaTransformation = convertArrayToThreeMatrix4(
                    selectedAssembly.ejectorPreferences!.deltaTransformation
                )

                let nodeBodyId = selectedAssembly.mechanism.nodeToBody.get(
                    selectedAssembly.ejectorPreferences!.parentNode ?? selectedAssembly.rootNodeId
                )
                if (!nodeBodyId) {
                    // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
                    nodeBodyId = selectedAssembly.mechanism.nodeToBody.get(selectedAssembly.rootNodeId)!
                }

                /** W = L x R. See save() for math details */
                const robotTransformation = convertJoltMat44ToThreeMatrix4(
                    World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
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
    }, [
        placeholderMesh,
        selectedAssembly.ejectorPreferences,
        selectedAssembly.mechanism.nodeToBody.get,
        selectedAssembly.rootNodeId,
    ])

    useEffect(() => {
        if (selectedAssembly?.ejectorPreferences) {
            setEjectorVelocity(selectedAssembly.ejectorPreferences.ejectorVelocity)
            setSelectedNode(selectedAssembly.ejectorPreferences.parentNode)
            setEjectOrder(selectedAssembly.ejectorPreferences.ejectOrder)
        } else {
            setSelectedNode(undefined)
        }
    }, [selectedAssembly])

    // useEffect(() => {
    //     World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
    //
    //     return () => {
    //         World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
    //     }
    // }, [])

    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            if (!selectedAssembly) {
                return false
            }

            const assoc = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate
            if (!assoc || !assoc.sceneObject || assoc.sceneObject !== selectedAssembly) {
                return false
            }

            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [selectedAssembly]
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
            <Stack direction="row" spacing={2} alignItems="center" className="mt-4">
                <LabelWithTooltip
                    labelText="Eject Order"
                    tooltipText="Choose how to eject pieces: FIFO (first in, first out) ejects the oldest-loaded item first, or LIFO (last in, first out) ejects the most recently loaded item first."
                />
                <ToggleButtonGroup
                    value={ejectOrder}
                    exclusive
                    onChange={(_: unknown, v: "FIFO" | "LIFO") => v && setEjectOrder(v)}
                >
                    <ToggleButton value="FIFO">FIFO</ToggleButton>
                    <ToggleButton value="LIFO">LIFO</ToggleButton>
                </ToggleButtonGroup>
            </Stack>

            {/* Slider for user to set velocity of ejector configuration */}
            <StatefulSlider
                label="Velocity"
                min={MIN_VELOCITY}
                max={MAX_VELOCITY}
                defaultValue={ejectorVelocity}
                // TODO:
                // format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                onChange={vel => {
                    setEjectorVelocity(vel as number)
                }}
                step={0.01}
            />

            <Spacer height={10} />
            {gizmoComponent}
            <Spacer height={10} />
            <Button
                onClick={() => {
                    if (gizmoRef.current) {
                        const robotTransformation = convertJoltMat44ToThreeMatrix4(
                            World.physicsSystem.getBody(selectedAssembly.getRootNodeId()!)!.GetWorldTransform()
                        )
                        gizmoRef.current.obj.position.setFromMatrixPosition(robotTransformation)
                        gizmoRef.current.obj.rotation.setFromRotationMatrix(robotTransformation)
                    }
                    setEjectorVelocity(1)
                    setSelectedNode(selectedAssembly?.rootNodeId)
                    setEjectOrder(selectedAssembly.ejectorPreferences?.ejectOrder ?? "FIFO")
                }}
            >
                Reset
            </Button>
        </>
    )
}

export default ConfigureGamepieceEjectorInterface
