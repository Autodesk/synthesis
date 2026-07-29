import type Jolt from "@synthesis.adsk/jolt-physics"
import { Stack } from "@mui/material"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import * as THREE from "three"
import SelectButton from "@/components/SelectButton"
import EjectableSceneObject from "@/mirabuf/EjectableSceneObject"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import Checkbox from "@/ui/components/Checkbox"
import StatefulSlider from "@/ui/components/StatefulSlider"
import { Button, Spacer } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertReactRgbaColorToThreeColor,
    convertThreeMatrix4ToArray,
} from "@/util/TypeConversions"
import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"

// slider constants
const MIN_ZONE_SIZE = 0.1
const MAX_ZONE_SIZE = 1.0
const MIN_ANIMATION_DURATION = 0.1
const MAX_ANIMATION_DURATION = 2.0
const ANIMATION_DURATION_STEP = 0.05

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
    zoneSize: number,
    gizmo: GizmoSceneObject,
    selectedRobot: MirabufSceneObject,
    selectedNode?: RigidNodeId,
    showZoneAlways?: boolean,
    maxPieces?: number,
    animationDuration?: number
) {
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
    gizmo.obj.matrixWorld.decompose(translation, rotation, new THREE.Vector3(1, 1, 1))

    const gizmoTransformation = new THREE.Matrix4().compose(translation, rotation, new THREE.Vector3(1, 1, 1))
    const robotTransformation = convertJoltMat44ToThreeMatrix4(
        World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
    )
    const deltaTransformation = gizmoTransformation.premultiply(robotTransformation.invert())

    selectedRobot.intakePreferences.deltaTransformation = convertThreeMatrix4ToArray(deltaTransformation)
    selectedRobot.intakePreferences.parentNode = selectedNode
    selectedRobot.intakePreferences.zoneDiameter = zoneSize
    if (showZoneAlways !== undefined) {
        selectedRobot.intakePreferences.showZoneAlways = showZoneAlways
    }

    selectedRobot.intakePreferences.maxPieces = maxPieces!
    selectedRobot.intakePreferences.animationDuration = animationDuration!
    selectedRobot.savePreferences()
}

const ConfigureGamepieceIntakeInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    registerCleanupFunction,
}) => {
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(undefined)
    const [zoneSize, setZoneSize] = useState<number>((MIN_ZONE_SIZE + MAX_ZONE_SIZE) / 2.0)
    const [showZoneAlways, setShowZoneAlways] = useState<boolean>(false)
    const [maxPieces, setMaxPieces] = useState<number>(selectedAssembly.intakePreferences?.maxPieces || 1)
    const [animationDuration, setAnimationDuration] = useState<number>(
        selectedAssembly.intakePreferences?.animationDuration || 0.5
    )

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedAssembly) {
            save(
                zoneSize,
                gizmoRef.current,
                selectedAssembly,
                selectedNode,
                showZoneAlways,
                maxPieces,
                animationDuration
            )
            selectedAssembly.updateIntakeSensor()
        }
    }, [selectedAssembly, selectedNode, zoneSize, showZoneAlways, maxPieces, animationDuration])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    useEffect(() => {
        if (!gizmoRef.current) {
            return
        }

        gizmoRef.current.obj.scale.set(zoneSize, zoneSize, zoneSize)
    }, [zoneSize])

    const placeholderMesh = useMemo(() => {
        // TODO: dynamic color?
        const material = World.sceneRenderer.createToonMaterial(
            convertReactRgbaColorToThreeColor({ r: 255, g: 255, b: 255, a: 255 })
        )
        material.transparent = true
        material.opacity = 0.6
        return new THREE.Mesh(new THREE.SphereGeometry(0.5), material)
    }, [])

    const gizmoComponent = useMemo(() => {
        if (selectedAssembly?.intakePreferences) {
            const postGizmoCreation = (gizmo: GizmoSceneObject) => {
                const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
                material.depthTest = false

                const deltaTransformation = convertArrayToThreeMatrix4(
                    selectedAssembly.intakePreferences!.deltaTransformation
                )

                let nodeBodyId = selectedAssembly.mechanism.nodeToBody.get(
                    selectedAssembly.intakePreferences!.parentNode ?? selectedAssembly.rootNodeId
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

                gizmo.setTransform(gizmoTransformation)
            }

            return (
                <TransformGizmoControl
                    key="pickup-transform-gizmo"
                    size={1.5}
                    gizmoRef={gizmoRef}
                    defaultMode="translate"
                    defaultMesh={placeholderMesh}
                    scaleDisabled={true}
                    rotateDisabled={true}
                    postGizmoCreation={postGizmoCreation}
                />
            )
        } else {
            gizmoRef.current = undefined
            return <></>
        }
    }, [
        selectedAssembly.intakePreferences,
        placeholderMesh,
        selectedAssembly.rootNodeId,
        selectedAssembly.mechanism.nodeToBody,
    ])

    useEffect(() => {
        if (selectedAssembly?.intakePreferences) {
            setZoneSize(selectedAssembly.intakePreferences.zoneDiameter)
            setSelectedNode(selectedAssembly.intakePreferences.parentNode)
            setMaxPieces(selectedAssembly.intakePreferences.maxPieces)
            setShowZoneAlways(selectedAssembly.intakePreferences.showZoneAlways ?? false)
            setAnimationDuration(selectedAssembly.intakePreferences.animationDuration ?? 0.5)
        } else {
            setSelectedNode(undefined)
            setShowZoneAlways(false)
            setAnimationDuration(0.5)
        }
    }, [selectedAssembly])

    useEffect(() => {
        // World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)

        // Hide the visual indicator when entering configuration mode
        if (selectedAssembly) {
            selectedAssembly.setIntakeVisualIndicatorVisible(false)
        }

        return () => {
            // World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)

            // Show the visual indicator when exiting configuration mode
            if (selectedAssembly) {
                selectedAssembly.setIntakeVisualIndicatorVisible(true)
            }
        }
    }, [selectedAssembly])

    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            if (!selectedAssembly) {
                return false
            }

            const assoc = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate
            if (!assoc || !assoc.sceneObject || assoc.sceneObject != selectedAssembly) {
                return false
            }

            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [selectedAssembly]
    )

    useEffect(() => {
        const originalPrefs = structuredClone(selectedAssembly.intakePreferences)
        registerCleanupFunction(undefined, () => {
            selectedAssembly.intakePreferences = originalPrefs
            selectedAssembly.updateIntakeSensor()
        })
    }, [registerCleanupFunction, selectedAssembly])

    return (
        <Stack direction="column">
            {/* Button for user to select the parent node */}
            <SelectButton
                placeholder="Select parent node"
                value={selectedNode}
                onSelect={(body: Jolt.Body) => trySetSelectedNode(body.GetID())}
            />

            {/* Slider for user to set velocity of ejector configuration */}
            <StatefulSlider
                label="Intake Zone Diameter (m)"
                min={MIN_ZONE_SIZE}
                max={MAX_ZONE_SIZE}
                defaultValue={zoneSize}
                // TODO:
                // format={{ minimumFractionDigits: 2, maximumFractionDigits: 2 }}
                onChange={vel => {
                    setZoneSize(vel as number)
                }}
                step={0.01}
            />
            <StatefulSlider
                label="Intake Animation Duration (s)"
                min={MIN_ANIMATION_DURATION}
                max={MAX_ANIMATION_DURATION}
                defaultValue={animationDuration ?? 0.5}
                onChange={v => {
                    const val = typeof v === "number" ? v : v[0]
                    setAnimationDuration(val)
                    EjectableSceneObject.setAnimationDuration(val)
                }}
                step={ANIMATION_DURATION_STEP}
                // TODO:
                // format={{ maximumFractionDigits: 2 }}
            />

            {/* Slider for adjusting max pieces the robot can intake */}
            <StatefulSlider
                label="Max Pieces"
                min={1}
                max={50}
                step={1}
                defaultValue={maxPieces ?? 1}
                onChange={v => setMaxPieces(v as number)}
            />

            {/* Checkbox for showing intake zone indicator at all times */}
            <Checkbox label="Show intake zone indicator always" checked={showZoneAlways} onClick={setShowZoneAlways} />
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
                    setZoneSize(0.5)
                    setSelectedNode(selectedAssembly?.rootNodeId)
                    setMaxPieces(selectedAssembly.intakePreferences?.maxPieces ?? 1)
                    setAnimationDuration(0.5)
                }}
            >
                Reset
            </Button>
        </Stack>
    )
}

export default ConfigureGamepieceIntakeInterface
