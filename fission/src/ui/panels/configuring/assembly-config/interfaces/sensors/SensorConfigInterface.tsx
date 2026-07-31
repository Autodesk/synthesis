import type Jolt from "@synthesis.adsk/jolt-physics"
import { Button, Stack, TextField } from "@mui/material"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import * as THREE from "three"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import EventSystem from "@/systems/EventSystem.ts"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsTypes"
import type { SensorPreferences, SensorType } from "@/systems/preferences/PreferenceTypes"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import SelectButton from "@/ui/components/SelectButton"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertThreeMatrix4ToArray,
} from "@/util/TypeConversions"
import { deltaFieldTransformsPhysicalProp } from "@/util/threejs/MeshCreation"

/** delta L such that gizmoWorld W = L * bodyWorld R, i.e. L = W R^(-1). See intake/zone configs. */
function computeDeltaFromGizmo(
    robot: MirabufSceneObject,
    gizmo: GizmoSceneObject,
    selectedNode?: RigidNodeId
): number[] | undefined {
    selectedNode ??= robot.rootNodeId

    const nodeBodyId = robot.mechanism.nodeToBody.get(selectedNode)
    if (!nodeBodyId) return undefined

    const translation = new THREE.Vector3()
    const rotation = new THREE.Quaternion()
    gizmo.obj.matrixWorld.decompose(translation, rotation, new THREE.Vector3())

    const gizmoTransformation = new THREE.Matrix4().compose(translation, rotation, new THREE.Vector3(1, 1, 1))
    const robotTransformation = convertJoltMat44ToThreeMatrix4(
        World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
    )
    return convertThreeMatrix4ToArray(gizmoTransformation.premultiply(robotTransformation.invert()))
}

export type SensorConfigProps = {
    selectedRobot: MirabufSceneObject
    selectedSensor: SensorPreferences
    /** Persist the sensor list after this sensor's fields are updated. */
    saveAllSensors: () => void
}

const SensorConfigInterface: React.FC<SensorConfigProps> = ({ selectedRobot, selectedSensor, saveAllSensors }) => {
    const [name, setName] = useState<string>(selectedSensor.name)
    const [sensorType, setSensorType] = useState<SensorType>(selectedSensor.sensorType)
    const [device, setDevice] = useState<string>(selectedSensor.device)
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(selectedSensor.parentNode)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (!gizmoRef.current) return
        const delta = computeDeltaFromGizmo(selectedRobot, gizmoRef.current, selectedNode)
        if (!delta) return

        selectedSensor.name = name
        selectedSensor.sensorType = sensorType
        selectedSensor.device = device
        selectedSensor.parentNode = selectedNode
        selectedSensor.deltaTransformation = delta
        saveAllSensors()
    }, [selectedRobot, selectedSensor, name, sensorType, device, selectedNode, saveAllSensors])

    useEffect(() => {
        return EventSystem.listen("ConfigurationSavedEvent", saveEvent)
    }, [saveEvent])

    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)
        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])

    const defaultGizmoMesh = useMemo(() => {
        const material = new THREE.MeshPhongMaterial({
            color: 0xffc21a,
            shininess: 0.0,
            opacity: 0.7,
            transparent: true,
        })
        return new THREE.Mesh(new THREE.BoxGeometry(0.15, 0.15, 0.15), material)
    }, [])

    const postGizmoCreation = useCallback(
        (gizmo: GizmoSceneObject) => {
            const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
            material.depthTest = false

            const delta = convertArrayToThreeMatrix4(selectedSensor.deltaTransformation)
            let nodeBodyId = selectedRobot.mechanism.nodeToBody.get(
                selectedSensor.parentNode ?? selectedRobot.rootNodeId
            )
            if (!nodeBodyId) {
                nodeBodyId = selectedRobot.mechanism.nodeToBody.get(selectedRobot.rootNodeId)!
            }

            const robotTransformation = convertJoltMat44ToThreeMatrix4(
                World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
            )
            const props = deltaFieldTransformsPhysicalProp(delta, robotTransformation)

            gizmo.obj.position.set(props.translation.x, props.translation.y, props.translation.z)
            gizmo.obj.rotation.setFromQuaternion(props.rotation)
        },
        [selectedRobot, selectedSensor]
    )

    const gizmoComponent = useMemo(
        () => (
            <TransformGizmoControl
                key="sensor-transform-gizmo"
                size={1.5}
                gizmoRef={gizmoRef}
                defaultMode="translate"
                defaultMesh={defaultGizmoMesh}
                scaleDisabled={true}
                postGizmoCreation={postGizmoCreation}
            />
        ),
        [defaultGizmoMesh, postGizmoCreation]
    )

    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            const assoc = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate
            if (!assoc || assoc?.sceneObject !== selectedRobot) return false
            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [selectedRobot]
    )

    return (
        <Stack gap={2} className="bg-background-secondary rounded-md p-2">
            <TextField
                label="Name"
                placeholder="Enter sensor name"
                defaultValue={selectedSensor.name}
                onChange={e => setName(e.target.value)}
            />
            <Button onClick={() => setSensorType(sensorType === "gyro" ? "accel" : "gyro")}>
                {sensorType === "gyro" ? "Gyro" : "Accelerometer"}
            </Button>
            <TextField
                label="Device"
                placeholder="SYN AHRS[0]"
                defaultValue={selectedSensor.device}
                onChange={e => setDevice(e.target.value)}
            />
            <SelectButton
                placeholder="Select parent node"
                value={selectedNode}
                onSelect={(body: Jolt.Body) => trySetSelectedNode(body.GetID())}
            />
            {gizmoComponent}
        </Stack>
    )
}

export default SensorConfigInterface
