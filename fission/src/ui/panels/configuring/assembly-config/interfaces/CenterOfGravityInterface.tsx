import { Stack } from "@mui/material"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import * as THREE from "three"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import StatefulSlider from "@/ui/components/StatefulSlider"
import { Spacer } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { convertJoltMat44ToThreeMatrix4 } from "@/util/TypeConversions"

interface CenterOfGravityInterfaceProps {
    selectedRobot: MirabufSceneObject
}

/**
 * Saves the center of gravity configuration to the selected robot.
 * The position is stored relative to the robot's root node so it moves with the robot.
 *
 * @param gizmo Reference to the transform gizmo object.
 * @param selectedRobot Selected robot to save data to.
 * @param effectStrength The strength of the CoG effect (0-2, where 1 is normal).
 */
function saveCenterOfGravity(gizmo: GizmoSceneObject, selectedRobot: MirabufSceneObject, effectStrength: number) {
    if (!gizmo || !selectedRobot) {
        return
    }

    const rootNodeId = selectedRobot.getRootNodeId()
    if (!rootNodeId) {
        return
    }

    const gizmoWorldPos = new THREE.Vector3()
    gizmo.obj.getWorldPosition(gizmoWorldPos)

    const robotTransform = convertJoltMat44ToThreeMatrix4(World.physicsSystem.getBody(rootNodeId).GetWorldTransform())

    const robotWorldPos = new THREE.Vector3()
    const robotWorldQuat = new THREE.Quaternion()
    const robotWorldScale = new THREE.Vector3()
    robotTransform.decompose(robotWorldPos, robotWorldQuat, robotWorldScale)

    const relativePos = gizmoWorldPos.clone().sub(robotWorldPos)
    relativePos.applyQuaternion(robotWorldQuat.clone().invert())

    selectedRobot.modifiedCenterOfGravity = relativePos
    selectedRobot.cogEffectStrength = effectStrength
    selectedRobot.updateMeshTransforms()
}

const CenterOfGravityInterface: React.FC<CenterOfGravityInterfaceProps> = ({ selectedRobot }) => {
    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)
    const [cogPosition, setCogPosition] = useState<THREE.Vector3>(new THREE.Vector3(0, 0, 0))
    const [effectStrength, setEffectStrength] = useState<number>(selectedRobot.cogEffectStrength ?? 1.0)

    // Create the center of gravity sphere mesh
    const cogSphereMesh = useMemo(() => {
        const material = new THREE.MeshBasicMaterial({
            color: 0xff00ff,
            opacity: 0.8,
            transparent: true,
            depthTest: false,
            depthWrite: false,
        })
        const sphere = new THREE.Mesh(new THREE.SphereGeometry(0.04), material) // larger than the visual sphere
        return sphere
    }, [])

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedRobot) {
            saveCenterOfGravity(gizmoRef.current, selectedRobot, effectStrength)
        }
    }, [selectedRobot, effectStrength])

    useEffect(() => {
        ConfigurationSavedEvent.listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.removeListener(saveEvent)
        }
    }, [saveEvent])

    useEffect(() => {
        const updateInterval = setInterval(() => {
            if (gizmoRef.current) {
                const newPosition = new THREE.Vector3()
                gizmoRef.current.obj.getWorldPosition(newPosition)
                setCogPosition(newPosition)
            }
        }, 100)

        return () => clearInterval(updateInterval)
    }, [])

    useEffect(() => {
        const previousVisibility = PreferencesSystem.getGlobalPreference("ShowCenterOfMassIndicators")
        PreferencesSystem.setGlobalPreference("ShowCenterOfMassIndicators", true)
        selectedRobot.updateMeshTransforms()

        return () => {
            PreferencesSystem.setGlobalPreference("ShowCenterOfMassIndicators", previousVisibility)
            selectedRobot.updateMeshTransforms()
        }
    }, [selectedRobot])

    const postGizmoCreation = useCallback(
        (gizmo: GizmoSceneObject) => {
            const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
            material.depthTest = false

            const rootNodeId = selectedRobot.getRootNodeId()
            if (!rootNodeId) {
                return
            }

            const robotTransform = convertJoltMat44ToThreeMatrix4(
                World.physicsSystem.getBody(rootNodeId).GetWorldTransform()
            )

            if (selectedRobot.modifiedCenterOfGravity) {
                const robotWorldPos = new THREE.Vector3()
                const robotWorldQuat = new THREE.Quaternion()
                const robotWorldScale = new THREE.Vector3()
                robotTransform.decompose(robotWorldPos, robotWorldQuat, robotWorldScale)

                const worldPos = selectedRobot.modifiedCenterOfGravity.clone()
                worldPos.applyQuaternion(robotWorldQuat)
                worldPos.add(robotWorldPos)

                gizmo.obj.position.copy(worldPos)
            } else {
                gizmo.obj.position.copy(selectedRobot.currentCenterOfGravity)
            }
        },
        [selectedRobot]
    )

    const handleReset = useCallback(() => {
        selectedRobot.modifiedCenterOfGravity = undefined
        selectedRobot.cogEffectStrength = 1.0 // reset
        setEffectStrength(1.0)

        if (gizmoRef.current) {
            const actualCoG = selectedRobot.currentCenterOfGravity
            gizmoRef.current.obj.position.copy(actualCoG)
            setCogPosition(actualCoG)
        }

        selectedRobot.updateMeshTransforms()
    }, [selectedRobot])

    const gizmoComponent = useMemo(() => {
        return (
            <TransformGizmoControl
                key="cog-transform-gizmo"
                size={1.2}
                gizmoRef={gizmoRef}
                defaultMode="translate"
                defaultMesh={cogSphereMesh}
                rotateDisabled={true}
                scaleDisabled={true}
                postGizmoCreation={postGizmoCreation}
            />
        )
    }, [cogSphereMesh, postGizmoCreation])

    return (
        <Stack direction="column" spacing={1}>
            <Label size="md">Center of Gravity Configuration</Label>
            <Label size="sm">Move the purple sphere to adjust the robot's center of gravity.</Label>
            {Spacer(8)}

            {gizmoComponent}

            {Spacer(8)}

            <Stack direction="row" spacing={1}>
                <Label size="sm">Current Position:</Label>
                <Label size="sm">
                    X: {cogPosition.x.toFixed(3)}m, Y: {cogPosition.y.toFixed(3)}m, Z: {cogPosition.z.toFixed(3)}m
                </Label>
            </Stack>

            {Spacer(8)}

            <StatefulSlider
                min={0}
                max={2}
                defaultValue={effectStrength}
                label={"Effect Strength"}
                onChange={value => {
                    setEffectStrength(value)
                    selectedRobot.cogEffectStrength = value
                }}
                tooltip="Adjusts how strongly the modified center of gravity affects the robot's physics (0 = no effect, 1 = normal, 2 = exaggerated)"
            />

            {Spacer(8)}

            <button
                type="button"
                onClick={handleReset}
                style={{
                    padding: "8px 16px",
                    backgroundColor: "#444",
                    color: "#fff",
                    border: "none",
                    borderRadius: "4px",
                    cursor: "pointer",
                }}
            >
                Reset to Calculated Center
            </button>
        </Stack>
    )
}

export default CenterOfGravityInterface
