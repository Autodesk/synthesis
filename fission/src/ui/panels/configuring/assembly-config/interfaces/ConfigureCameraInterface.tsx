import type Jolt from "@azaleacolburn/jolt-physics"
import { Stack, TextField } from "@mui/material"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import * as THREE from "three"
import SelectButton from "@/components/SelectButton"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { type CameraPreferences, defaultCameraPreferences } from "@/systems/preferences/PreferenceTypes"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import Label from "@/ui/components/Label"
import StatefulSlider from "@/ui/components/StatefulSlider"
import { Button } from "@/ui/components/StyledComponents"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertReactRgbaColorToThreeColor,
    convertThreeMatrix4ToArray,
} from "@/util/TypeConversions"

const MIN_FOV = 10
const MAX_FOV = 170
const MIN_RES = 64
const MAX_RES = 1280
const MIN_FPS = 1
const MAX_FPS = 60

function parentBodyId(robot: MirabufSceneObject, parentNode: string | undefined): Jolt.BodyID | undefined {
    return robot.mechanism.nodeToBody.get(parentNode ?? robot.rootNodeId)
}

interface ConfigCameraProps {
    selectedRobot: MirabufSceneObject
}

const ConfigureCameraInterface: React.FC<ConfigCameraProps> = ({ selectedRobot }) => {
    const [version, setVersion] = useState(0)
    const [selectedIndex, setSelectedIndex] = useState<number>(selectedRobot.cameraPreferences.length > 0 ? 0 : -1)
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(undefined)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const cameras = selectedRobot.cameraPreferences
    const camera: CameraPreferences | undefined = cameras[selectedIndex]

    const forceRender = useCallback(() => setVersion(v => v + 1), [])

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
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])

    useEffect(() => {
        setSelectedNode(camera?.parentNode)
    }, [selectedIndex, camera])

    // selectedIndex dep so gizmo respawns for newly selected camera
    const placeholderMesh = useMemo(() => {
        return new THREE.Mesh(
            new THREE.BoxGeometry(0.12, 0.08, 0.16).translate(0, 0, 0.08),
            World.sceneRenderer.createToonMaterial(convertReactRgbaColorToThreeColor({ r: 80, g: 180, b: 255, a: 255 }))
        )
    }, [selectedIndex])

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
                key={`camera-gizmo-${selectedIndex}`}
                size={1.5}
                gizmoRef={gizmoRef}
                defaultMode="translate"
                defaultMesh={placeholderMesh}
                scaleDisabled={true}
                postGizmoCreation={postGizmoCreation}
            />
        )
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [placeholderMesh, selectedIndex, camera, selectedRobot])

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

    const addCamera = useCallback(() => {
        const nextId = cameras.reduce((max, c) => Math.max(max, c.id + 1), 0)
        cameras.push(defaultCameraPreferences(nextId))
        setSelectedIndex(cameras.length - 1)

        commit()
        forceRender()
    }, [cameras, commit, forceRender])

    const removeCamera = useCallback(() => {
        if (selectedIndex < 0) return

        cameras.splice(selectedIndex, 1)
        setSelectedIndex(cameras.length > 0 ? Math.max(0, selectedIndex - 1) : -1)

        commit()
        forceRender()
    }, [cameras, selectedIndex, commit, forceRender])

    return (
        <Stack gap={2} key={`${version}-${selectedIndex}`}>
            <Stack direction="row" gap={1} flexWrap="wrap" justifyContent="center">
                {cameras.map((c, i) => (
                    <Button
                        key={`${c.name}-${c.id}`}
                        onClick={() => setSelectedIndex(i)}
                        sx={i === selectedIndex ? { outline: "2px solid #2684ff" } : undefined}
                    >
                        {c.name}
                    </Button>
                ))}
            </Stack>

            <Stack direction="row" gap={1} justifyContent="center">
                <Button onClick={addCamera}>Add Camera</Button>
                {camera ? <Button onClick={removeCamera}>Remove</Button> : null}
            </Stack>

            {camera ? (
                <>
                    <TextField
                        label="Name"
                        size="small"
                        defaultValue={camera.name}
                        helperText={`Sim device: ${camera.name}[${camera.id}] — must match robot code`}
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

                    {gizmoComponent}
                </>
            ) : (
                <Label size="md" className="text-center">
                    No cameras configured. Add one to get started.
                </Label>
            )}
        </Stack>
    )
}

export default ConfigureCameraInterface
