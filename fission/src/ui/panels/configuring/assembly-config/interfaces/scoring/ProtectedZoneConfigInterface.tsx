import type Jolt from "@azaleacolburn/jolt-physics"
import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import * as THREE from "three"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import ProtectedZoneSceneObject from "@/mirabuf/ProtectedZoneSceneObject"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { Alliance, ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import type GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import World from "@/systems/World"
import SelectButton from "@/ui/components/SelectButton"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertThreeMatrix4ToArray,
} from "@/util/TypeConversions"
import { deltaFieldTransformsPhysicalProp as deltaFieldTransformsVisualProperties } from "@/util/threejs/MeshCreation"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent"
import { Button, Checkbox, FormControlLabel, Stack, TextField } from "@mui/material"
import StatefulCheckbox from "@/ui/components/StatefulCheckbox"

/**
 * Saves ejector configuration to selected field.
 *
 * Math Explanation:
 * Let W be the world transformation matrix of the gizmo.
 * Let R be the world transformation matrix of the selected field node.
 * Let L be the local transformation matrix of the gizmo, relative to the selected field node.
 *
 * We are given W and R, and want to save L with the field. This way when we create
 * the ejection point afterwards, it will be relative to the selected field node.
 *
 * W = L R
 * L = W R^(-1)
 *
 * ThreeJS sets the standard multiplication operation for matrices to be premultiply. I really
 * don't like this terminology as it's thrown me off multiple times, but I suppose it does go
 * against most other multiplication operations.
 *
 * @param name Name given to the protected zone by the user.
 * @param alliance protected zone alliance.
 * @param points Number of points to penalize.
 * @param requireRobotContact Do you need to contact a robot for the penalty to apply.
 * @param gizmo Reference to the transform gizmo object.
 * @param selectedNode Selected node that configuration is relative to.
 */
function save(
    field: MirabufSceneObject,
    zone: ProtectedZonePreferences,
    name: string,
    alliance: Alliance,
    points: number,
    requireRobotContact: boolean,
    gizmo: GizmoSceneObject,
    selectedNode?: RigidNodeId
) {
    if (!field?.fieldPreferences || !gizmo) {
        return
    }

    selectedNode ??= field.rootNodeId

    const nodeBodyId = field.mechanism.nodeToBody.get(selectedNode)
    if (!nodeBodyId) {
        return
    }

    // This step seems useless, but keeps the scale from messing up the rotation
    const translation = new THREE.Vector3(0, 0, 0)
    const rotation = new THREE.Quaternion(0, 0, 0, 1)
    const scale = new THREE.Vector3(1, 1, 1)
    gizmo.obj.matrixWorld.decompose(translation, rotation, scale)
    scale.x = Math.abs(scale.x)
    scale.y = Math.abs(scale.y)
    scale.z = Math.abs(scale.z)

    const gizmoTransformation = new THREE.Matrix4().compose(translation, rotation, scale)
    const fieldTransformation = convertJoltMat44ToThreeMatrix4(
        World.physicsSystem.getBody(nodeBodyId).GetWorldTransform()
    )
    const deltaTransformation = gizmoTransformation.premultiply(fieldTransformation.invert())

    zone.deltaTransformation = convertThreeMatrix4ToArray(deltaTransformation)
    zone.name = name
    zone.alliance = alliance
    zone.parentNode = selectedNode
    zone.penaltyPoints = points
    zone.requireRobotContact = requireRobotContact

    if (!field.fieldPreferences.protectedZones.includes(zone)) field.fieldPreferences.protectedZones.push(zone)

    PreferencesSystem.savePreferences()
}

interface ZoneConfigProps {
    selectedField: MirabufSceneObject
    selectedZone: ProtectedZonePreferences
    saveAllZones: () => void
}

const ZoneConfigInterface: React.FC<ZoneConfigProps> = ({ selectedField, selectedZone, saveAllZones }) => {
    //Official FIRST hex
    // TODO: Do we want to eventually make these editable?
    const redMaterial = useMemo(() => {
        return ProtectedZoneSceneObject.redMaterial.clone() as THREE.MeshPhongMaterial
    }, [])

    const blueMaterial = useMemo(() => {
        return ProtectedZoneSceneObject.blueMaterial.clone() as THREE.MeshPhongMaterial
    }, [])

    const [name, setName] = useState<string>(selectedZone.name)
    const [alliance, setAlliance] = useState<Alliance>(selectedZone.alliance)
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(selectedZone.parentNode)
    const [points, setPoints] = useState<number>(selectedZone.penaltyPoints)
    const [requireRobotContact, setRequireRobotContact] = useState<boolean>(selectedZone.requireRobotContact)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedField) {
            save(
                selectedField,
                selectedZone,
                name,
                alliance,
                points,
                requireRobotContact,
                gizmoRef.current,
                selectedNode
            )
            saveAllZones()
        }
    }, [selectedField, selectedZone, name, alliance, points, requireRobotContact, selectedNode, saveAllZones])

    useEffect(() => {
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
    }, [saveEvent])

    /** Holds a pause for the duration of the interface component */
    useEffect(() => {
        World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_CONFIG)

        return () => {
            World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_CONFIG)
        }
    }, [])

    /** Creates the default mesh for the gizmo */
    const defaultGizmoMesh = useMemo(() => {
        console.debug("Default Gizmo Mesh Recreation")

        if (!selectedZone) {
            console.debug("No zone selected")
            return undefined
        }

        return new THREE.Mesh(
            new THREE.BoxGeometry(1, 1, 1),
            selectedZone.alliance === "blue" ? blueMaterial : redMaterial
        )
    }, [selectedZone, selectedZone.alliance, blueMaterial, redMaterial])

    /** Creates TransformGizmoControl component and sets up target mesh. */
    const gizmoComponent = useMemo(() => {
        if (selectedField && selectedZone) {
            const postGizmoCreation = (gizmo: GizmoSceneObject) => {
                const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
                material.depthTest = false

                const deltaTransformation = convertArrayToThreeMatrix4(selectedZone.deltaTransformation)

                let nodeBodyId = selectedField.mechanism.nodeToBody.get(
                    selectedZone.parentNode ?? selectedField.rootNodeId
                )
                if (!nodeBodyId) {
                    // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
                    nodeBodyId = selectedField.mechanism.nodeToBody.get(selectedField.rootNodeId)!
                }

                /** W = L x R. See save() for math details */
                const fieldTransformation = convertJoltMat44ToThreeMatrix4(
                    World.physicsSystem.getBody(nodeBodyId).GetWorldTransform()
                )
                const props = deltaFieldTransformsVisualProperties(deltaTransformation, fieldTransformation)

                gizmo.obj.position.set(props.translation.x, props.translation.y, props.translation.z)
                gizmo.obj.rotation.setFromQuaternion(props.rotation)
                gizmo.obj.scale.set(props.scale.x, props.scale.y, props.scale.z)
                selectedField.removeProtectedZoneObject(selectedZone) // avoid rendering twice
            }

            return (
                <TransformGizmoControl
                    key="zone-transform-gizmo"
                    size={1.5}
                    gizmoRef={gizmoRef}
                    defaultMode="translate"
                    defaultMesh={defaultGizmoMesh}
                    postGizmoCreation={postGizmoCreation}
                />
            )
        } else {
            gizmoRef.current = undefined
            return <></>
        }
    }, [selectedField, selectedZone, defaultGizmoMesh])

    /** Sets the selected node if it is a part of the currently loaded field */
    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            if (!selectedField) {
                return false
            }

            const assoc = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate
            if (!assoc || assoc?.sceneObject !== selectedField) {
                return false
            }

            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [selectedField]
    )

    return (
        <Stack gap={2} className="bg-background-secondary rounded-md p-2">
            {/** Set the zone name */}
            <TextField
                label="Name"
                placeholder="Enter zone name"
                defaultValue={selectedZone.name}
                onChange={e => setName(e.target.value)}
            />

            {/** Set the alliance color */}
            <Button
                onClick={() => {
                    setAlliance(alliance === "blue" ? "red" : "blue")
                    if (gizmoRef.current)
                        (gizmoRef.current.obj as THREE.Mesh).material = alliance === "blue" ? redMaterial : blueMaterial
                }}
                style={{ background: alliance === "red" ? "#ff0000" : "#0000ff" }}
            >{`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}</Button>

            {/** Select a parent node */}
            <SelectButton
                placeholder="Select parent node"
                value={selectedNode}
                onSelect={(body: Jolt.Body) => trySetSelectedNode(body.GetID())}
            />

            {/** Set the point value */}
            <TextField
                inputProps={{ type: "number" }}
                label="Penalty Points"
                placeholder="Zone penalty points"
                defaultValue={selectedZone.penaltyPoints}
                onChange={v => setPoints(parseInt(v.target.value) || 1)}
            />

            {/** When checked, the zone will destroy gamepieces it comes in contact with */}
            {/** <Checkbox
                    label="Destroy Gamepiece"
                    defaultState={selectedZone.destroyGamepiece}
                    onClick={setDestroy}
                /> */}

            {/** When checked, points will stay even when a gamepiece leaves the zone */}
            <StatefulCheckbox
                label="Require Robot Contact"
                checked={selectedZone.requireRobotContact}
                onClick={checked => setRequireRobotContact(checked)}
            />

            {/** Switch between transform control modes */}

            {gizmoComponent}
        </Stack>
    )
}

export default ZoneConfigInterface
