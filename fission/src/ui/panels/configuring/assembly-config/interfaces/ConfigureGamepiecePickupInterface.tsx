import type Jolt from "@azaleacolburn/jolt-physics"
import { Switch } from "@mui/base/Switch"
import { Button, FormControlLabel, Slider, Stack, Typography } from "@mui/material"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import * as THREE from "three"
import SelectButton from "@/components/SelectButton"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import { Spacer } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertReactRgbaColorToThreeColor,
    convertThreeMatrix4ToArray,
} from "@/util/TypeConversions"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import EjectableSceneObject from "@/mirabuf/EjectableSceneObject"
import StatefulSlider from "@/ui/components/StatefulSlider"

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
function save(
    zoneSize: number,
    gizmo: GizmoSceneObject,
    selectedRobot: MirabufSceneObject,
    selectedNode?: RigidNodeId,
    showZoneAlways?: boolean,
    maxPieces?: number
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
        World.physicsSystem.getBody(nodeBodyId).GetWorldTransform()
    )
    const deltaTransformation = gizmoTransformation.premultiply(robotTransformation.invert())

    selectedRobot.intakePreferences.deltaTransformation = convertThreeMatrix4ToArray(deltaTransformation)
    selectedRobot.intakePreferences.parentNode = selectedNode
    selectedRobot.intakePreferences.zoneDiameter = zoneSize
    if (showZoneAlways !== undefined) {
        selectedRobot.intakePreferences.showZoneAlways = showZoneAlways
    }

    selectedRobot.intakePreferences.maxPieces = maxPieces!

    PreferencesSystem.savePreferences()
}

interface ConfigPickupProps {
    selectedRobot: MirabufSceneObject
}

const ConfigureGamepiecePickupInterface: React.FC<ConfigPickupProps> = ({ selectedRobot }) => {
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(undefined)
    const [zoneSize, setZoneSize] = useState<number>((MIN_ZONE_SIZE + MAX_ZONE_SIZE) / 2.0)
    const [showZoneAlways, setShowZoneAlways] = useState<boolean>(false)
    const [maxPieces, setMaxPieces] = useState<number>(selectedRobot.intakePreferences?.maxPieces || 1)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedRobot) {
            save(zoneSize, gizmoRef.current, selectedRobot, selectedNode, showZoneAlways, maxPieces)
            selectedRobot.updateIntakeSensor()
        }
    }, [selectedRobot, selectedNode, zoneSize, showZoneAlways, maxPieces])

    useEffect(() => {
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
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
        if (selectedRobot?.intakePreferences) {
            const postGizmoCreation = (gizmo: GizmoSceneObject) => {
                const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
                material.depthTest = false

                const deltaTransformation = convertArrayToThreeMatrix4(
                    selectedRobot.intakePreferences!.deltaTransformation
                )

                let nodeBodyId = selectedRobot.mechanism.nodeToBody.get(
                    selectedRobot.intakePreferences!.parentNode ?? selectedRobot.rootNodeId
                )
                if (!nodeBodyId) {
                    // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
                    nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedRobot.rootNodeId)!
                }

                /** W = L x R. See save() for math details */
                const robotTransformation = convertJoltMat44ToThreeMatrix4(
                    World.physicsSystem.getBody(nodeBodyId).GetWorldTransform()
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
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedRobot?.intakePreferences, placeholderMesh])

    useEffect(() => {
        if (selectedRobot?.intakePreferences) {
            setZoneSize(selectedRobot.intakePreferences.zoneDiameter)
            setSelectedNode(selectedRobot.intakePreferences.parentNode)
            setMaxPieces(selectedRobot.intakePreferences.maxPieces)
            setShowZoneAlways(selectedRobot.intakePreferences.showZoneAlways ?? false)
        } else {
            setSelectedNode(undefined)
            setShowZoneAlways(false)
        }
    }, [selectedRobot])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)

        // Hide the visual indicator when entering configuration mode
        if (selectedRobot) {
            selectedRobot.setIntakeVisualIndicatorVisible(false)
        }

        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)

            // Show the visual indicator when exiting configuration mode
            if (selectedRobot) {
                selectedRobot.setIntakeVisualIndicatorVisible(true)
            }
        }
    }, [selectedRobot])

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
                max={10}
                step={1}
                defaultValue={maxPieces ?? 1}
                onChange={v => setMaxPieces(v as number)}
            />

            {/* Checkbox for showing intake zone indicator at all times */}
            <Stack direction="row" justifyContent={"space-between"} alignItems={"center"} textAlign={"center"}>
                <Typography variant="h5" className="mr-12 whitespace-nowrap">
                    Show intake zone indicator always
                </Typography>
                <Switch
                    checked={showZoneAlways}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                        setShowZoneAlways(e.target.checked)
                    }}
                    slotProps={{
                        root: {
                            className: `
                                group relative inline-block 
                                w-[24px] h-[24px] m-2.5 
                                cursor-pointer transform transition-transform 
                                hover:scale-[1.03] active:scale-[1.06]
                            `,
                        },
                        input: {
                            className: `
                                cursor-inherit absolute 
                                w-full h-full top-0 left-0 
                                opacity-0 z-10 border-none
                            `,
                        },
                        track: ownerState => {
                            const baseClasses = `
                                absolute block w-full h-full 
                                transition rounded-full 
                                border border-solid outline-none 
                                border-interactive-element-right 
                                dark:border-interactive-element-right 
                                group-[.base--focusVisible]:shadow-outline-switch 
                                transform transition-transform 
                                group-hover:scale-[1.03] group-active:scale-[1.06]
                            `
                            const backgroundClasses = ownerState.checked
                                ? "bg-gradient-to-br from-interactive-element-left to-interactive-element-right"
                                : "bg-background-secondary"

                            return {
                                className: `${baseClasses} ${backgroundClasses}`,
                            }
                        },
                        thumb: {
                            className: "display-none",
                        },
                    }}
                />
            </Stack>
            {gizmoComponent}
            {Spacer(10)}
            <Button
                onClick={() => {
                    if (gizmoRef.current) {
                        const robotTransformation = convertJoltMat44ToThreeMatrix4(
                            World.physicsSystem.getBody(selectedRobot.getRootNodeId()!).GetWorldTransform()
                        )
                        gizmoRef.current.obj.position.setFromMatrixPosition(robotTransformation)
                        gizmoRef.current.obj.rotation.setFromRotationMatrix(robotTransformation)
                    }
                    setZoneSize(0.5)
                    setSelectedNode(selectedRobot?.rootNodeId)
                    setMaxPieces(selectedRobot.intakePreferences?.maxPieces ?? 1)
                }}
            >
                Reset
            </Button>
        </>
    )
}

export default ConfigureGamepiecePickupInterface
