import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import * as THREE from "three"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { CameraPreferences } from "@/systems/preferences/PreferenceTypes"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import SelectButton from "@/ui/components/SelectButton"
import StatefulSlider from "@/ui/components/StatefulSlider"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertReactRgbaColorToThreeColor,
    convertThreeMatrix4ToArray,
} from "@/util/TypeConversions"
import { TextField } from "@mui/material"
import type Jolt from "@synthesis.adsk/jolt-physics"
import type React from "react"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import EventSystem from "@/systems/EventSystem"

const MIN_FOV = 10
const MAX_FOV = 170
const MIN_RES = 64
const MAX_RES = 1280
const MIN_FPS = 1
const MAX_FPS = 60

function parentBodyId(robot: MirabufSceneObject, parentNode: string | undefined): Jolt.BodyID | undefined {
    return robot.mechanism.nodeToBody.get(parentNode ?? robot.rootNodeId)
}

interface CameraConfigInterfaceProps {
    selectedRobot: MirabufSceneObject
    camera: CameraPreferences
}

const CameraConfigInterface: React.FC<CameraConfigInterfaceProps> = ({ selectedRobot, camera }) => {
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(undefined)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const commit = useCallback(() => {
        if (camera && gizmoRef.current) {
            const nodeBodyId = parentBodyId(selectedRobot, camera.parentNode)
            if (nodeBodyId) {
                const gizmoWorld = gizmoRef.current.obj.matrixWorld.clone()
                const robotWorld = convertJoltMat44ToThreeMatrix4(
                    World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
                )
                const delta = gizmoWorld.premultiply(robotWorld.invert())
                camera.deltaTransformation = convertThreeMatrix4ToArray(delta)
            }
        }
        PreferencesSystem.setRobotPreferences(selectedRobot.assemblyName, selectedRobot.robotPreferences)
        PreferencesSystem.savePreferences()
        selectedRobot.updateCameras()
    }, [camera, selectedRobot])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", commit)
    }, [commit])

    useEffect(() => {
        setSelectedNode(camera?.parentNode)
    }, [camera])

    // selectedIndex dep so gizmo respawns for newly selected camera
    const placeholderMesh = useMemo(() => {
        return new THREE.Mesh(
            new THREE.BoxGeometry(0.12, 0.08, 0.16).translate(0, 0, 0.08),
            World.sceneRenderer.createToonMaterial(convertReactRgbaColorToThreeColor({ r: 80, g: 180, b: 255, a: 255 }))
        )
    }, [camera])

    const gizmoComponent = useMemo(() => {
        if (!camera) {
            gizmoRef.current = undefined
            return <></>
        }

        const postGizmoCreation = (gizmo: GizmoSceneObject) => {
            const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
            material.depthTest = false

            const delta = convertArrayToThreeMatrix4(camera.deltaTransformation)
            const nodeBodyId = parentBodyId(selectedRobot, camera.parentNode)
            if (!nodeBodyId) return

            const robotWorld = convertJoltMat44ToThreeMatrix4(
                World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
            )
            const gizmoWorld = delta.premultiply(robotWorld)
            gizmo.obj.position.setFromMatrixPosition(gizmoWorld)
            gizmo.obj.rotation.setFromRotationMatrix(gizmoWorld)
        }

        return (
            <TransformGizmoControl
                key={`camera-gizmo-${camera.id}`}
                size={1.5}
                gizmoRef={gizmoRef}
                defaultMode="translate"
                defaultMesh={placeholderMesh}
                scaleDisabled={true}
                postGizmoCreation={postGizmoCreation}
            />
        )
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [placeholderMesh, camera, selectedRobot])

    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            const assoc = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate
            if (!assoc || assoc.sceneObject !== selectedRobot) return false

            setSelectedNode(assoc.rigidNodeId)
            if (camera) camera.parentNode = assoc.rigidNodeId

            commit()

            return true
        },
        [selectedRobot, camera, commit]
    )

    return (
        <>
            <TextField
                label="Name"
                size="small"
                defaultValue={camera.name}
                helperText={`Sim device: ${camera.name}[${camera.id}] (must match robot code)`}
                onChange={e => {
                    camera.name = e.target.value
                }}
            />

            <SelectButton
                placeholder="Select parent node"
                value={selectedNode}
                onSelect={(body: Jolt.Body) => trySetSelectedNode(body.GetID())}
            />

            <StatefulSlider
                label="Field of View (°)"
                min={MIN_FOV}
                max={MAX_FOV}
                step={1}
                defaultValue={camera.fovDegrees}
                onChange={v => {
                    camera.fovDegrees = v
                }}
            />
            {/* NOTE: in code sim mode, these are only configurable through robot code */}
            {selectedRobot.brain?.brainType === "synthesis" && (
                <>
                    <StatefulSlider
                        label="Width (px)"
                        min={MIN_RES}
                        max={MAX_RES}
                        step={16}
                        defaultValue={camera.resolutionWidth}
                        onChange={v => {
                            camera.resolutionWidth = v
                        }}
                    />
                    <StatefulSlider
                        label="Height (px)"
                        min={MIN_RES}
                        max={MAX_RES}
                        step={16}
                        defaultValue={camera.resolutionHeight}
                        onChange={v => {
                            camera.resolutionHeight = v
                        }}
                    />
                    <StatefulSlider
                        label="Frame Rate (fps)"
                        min={MIN_FPS}
                        max={MAX_FPS}
                        step={1}
                        defaultValue={camera.fps}
                        onChange={v => {
                            camera.fps = v
                        }}
                    />
                </>
            )}

            {gizmoComponent}
        </>
    )
}

export default CameraConfigInterface
