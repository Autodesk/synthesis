import * as THREE from "three"
import { useCallback, useEffect, useMemo, useState } from "react"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import SelectButton from "@/components/SelectButton"
import Slider from "@/ui/components/Slider"
import Jolt from "@barclah/jolt-physics"
import Label from "@/ui/components/Label"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Button from "@/ui/components/Button"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import World from "@/systems/World"
import { MiraType } from "@/mirabuf/MirabufLoader"
import {
    Array_ThreeMatrix4,
    JoltMat44_ThreeMatrix4,
    ReactRgbaColor_ThreeColor,
    ThreeMatrix4_Array,
} from "@/util/TypeConversions"
import { useTheme } from "@/ui/ThemeContext"
import LabeledButton, { LabelPlacement } from "@/ui/components/LabeledButton"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import { SynthesisIcons } from "@/ui/components/StyledComponents"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"

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
    mesh: THREE.Mesh,
    selectedRobot: MirabufSceneObject,
    selectedNode?: RigidNodeId
) {
    if (!selectedRobot?.ejectorPreferences || !mesh) {
        return
    }

    selectedNode ??= selectedRobot.rootNodeId

    const nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedNode)
    if (!nodeBodyId) {
        return
    }

    const gizmoTransformation = mesh.matrixWorld
    const robotTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
    const deltaTransformation = gizmoTransformation.premultiply(robotTransformation.invert())

    selectedRobot.ejectorPreferences.deltaTransformation = ThreeMatrix4_Array(deltaTransformation)
    selectedRobot.ejectorPreferences.parentNode = selectedNode
    selectedRobot.ejectorPreferences.ejectorVelocity = ejectorVelocity

    PreferencesSystem.savePreferences()
}

const ConfigureShotTrajectoryPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const { currentTheme, themes } = useTheme()
    const theme = useMemo(() => {
        return themes[currentTheme]
    }, [currentTheme, themes])

    const [selectedRobot, setSelectedRobot] = useState<MirabufSceneObject | undefined>(undefined)
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(undefined)
    const [ejectorVelocity, setEjectorVelocity] = useState<number>((MIN_VELOCITY + MAX_VELOCITY) / 2.0)
    const [mesh, setMesh] = useState<THREE.Mesh | undefined>(undefined)
    const robots = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()].filter(x => {
            if (x instanceof MirabufSceneObject) {
                return x.miraType === MiraType.ROBOT
            }
            return false
        }) as MirabufSceneObject[]
        return assemblies
    }, [])

    // Not sure I like this, but made it a state and effect rather than a memo to add the cleanup to the end
    useEffect(() => {
        if (!selectedRobot?.ejectorPreferences) {
            // setMesh(undefined)
            return
        }

        setMesh(
            new THREE.Mesh(
                new THREE.ConeGeometry(0.1, 0.4, 4).rotateX(Math.PI / 2.0).translate(0, 0, 0.2),
                World.SceneRenderer.CreateToonMaterial(ReactRgbaColor_ThreeColor(theme.HighlightSelect.color))
            )
        )
        if (!mesh) return
        ;(mesh.material as THREE.Material).depthTest = false
        const translationGizmo = new GizmoSceneObject(mesh, "translate", 1.5)
        const rotationGizmo = new GizmoSceneObject(mesh, "rotate", 2.0)

        const deltaTransformation = Array_ThreeMatrix4(selectedRobot.ejectorPreferences.deltaTransformation)

        let nodeBodyId = selectedRobot.mechanism.nodeToBody.get(
            selectedRobot.ejectorPreferences.parentNode ?? selectedRobot.rootNodeId
        )
        if (!nodeBodyId) {
            // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
            nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedRobot.rootNodeId)!
        }

        /** W = L x R. See save() for math details */
        const robotTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
        const gizmoTransformation = deltaTransformation.premultiply(robotTransformation)

        mesh.position.setFromMatrixPosition(gizmoTransformation)
        mesh.rotation.setFromRotationMatrix(gizmoTransformation)

        return () => {
            World.SceneRenderer.RemoveSceneObject(translationGizmo.id)
            World.SceneRenderer.RemoveSceneObject(rotationGizmo.id)
            // setMesh(undefined)
        }
    }, [mesh, selectedRobot, theme])

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
        <Panel
            name="Configure Ejector"
            icon={SynthesisIcons.Gear}
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                if (mesh && selectedRobot) {
                    save(ejectorVelocity, mesh, selectedRobot, selectedNode)
                    const currentGp = selectedRobot.activeEjectable
                    selectedRobot.SetEjectable(undefined, true)
                    selectedRobot.SetEjectable(currentGp)
                }
            }}
            onCancel={() => {}}
            acceptEnabled={selectedRobot?.ejectorPreferences != undefined}
        >
            {selectedRobot?.ejectorPreferences == undefined ? (
                <>
                    <Label>Select a robot</Label>
                    {/** Scroll view for selecting a robot to configure */}
                    <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[40vh] bg-background-secondary rounded-md p-2">
                        {robots.map(mirabufSceneObject => {
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
                    <LabeledButton
                        placement={LabelPlacement.Left}
                        label="Reset"
                        value="Reset"
                        buttonClassName="w-min"
                        onClick={() => {
                            if (mesh) {
                                const robotTransformation = JoltMat44_ThreeMatrix4(
                                    World.PhysicsSystem.GetBody(selectedRobot.GetRootNodeId()!).GetWorldTransform()
                                )
                                mesh.position.setFromMatrixPosition(robotTransformation)
                                mesh.rotation.setFromRotationMatrix(robotTransformation)
                            }
                            setEjectorVelocity((MIN_VELOCITY + MAX_VELOCITY) / 2.0)
                            setSelectedNode(selectedRobot?.rootNodeId)
                        }}
                    />
                </>
            )}
        </Panel>
    )
}

export default ConfigureShotTrajectoryPanel
