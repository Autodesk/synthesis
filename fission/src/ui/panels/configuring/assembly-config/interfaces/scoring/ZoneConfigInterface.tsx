import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import Input from "@/components/Input"
import Button from "@/components/Button"
import Checkbox from "@/components/Checkbox"
import NumberInput from "@/components/NumberInput"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SelectButton from "@/ui/components/SelectButton"
import Jolt from "@barclah/jolt-physics"
import * as THREE from "three"
import World from "@/systems/World"
import { Array_ThreeMatrix4, JoltMat44_ThreeMatrix4, ThreeMatrix4_Array } from "@/util/TypeConversions"
import { useTheme } from "@/ui/ThemeContext"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
// import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { Alliance, ScoringZonePreferences } from "@/systems/preferences/PreferenceTypes"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import { DeltaFieldTransforms_PhysicalProp as DeltaFieldTransforms_VisualProperties } from "@/util/threejs/MeshCreation"
import { ConfigurationSavedEvent } from "../../ConfigurationSavedEvent"
import GizmoSceneObject/*, { GizmoMode } */ from "@/systems/scene/GizmoSceneObject"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"

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
 * @param name Name given to the scoring zone by the user.
 * @param alliance Scoring zone alliance.
 * @param points Number of points the zone is worth.
 * @param destroy Destroy gamepiece setting.
 * @param persistent Persistent points setting.
 * @param gizmo Reference to the transform gizmo object.
 * @param selectedNode Selected node that configuration is relative to.
 */
function save(
    field: MirabufSceneObject,
    zone: ScoringZonePreferences,
    name: string,
    alliance: Alliance,
    points: number,
    destroy: boolean,
    persistent: boolean,
    gizmo: GizmoSceneObject,
    selectedNode?: RigidNodeId
) {
    console.log("save")
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

    const gizmoTransformation = new THREE.Matrix4().compose(translation, rotation, scale)
    const fieldTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
    const deltaTransformation = gizmoTransformation.premultiply(fieldTransformation.invert())

    zone.deltaTransformation = ThreeMatrix4_Array(deltaTransformation)
    zone.name = name
    zone.alliance = alliance
    zone.parentNode = selectedNode
    zone.points = points
    zone.destroyGamepiece = destroy
    zone.persistentPoints = persistent

    if (!field.fieldPreferences.scoringZones.includes(zone)) field.fieldPreferences.scoringZones.push(zone)

    PreferencesSystem.savePreferences()
}

interface ZoneConfigProps {
    selectedField: MirabufSceneObject
    selectedZone: ScoringZonePreferences
    saveAllZones: () => void
}

const ZoneConfigInterface: React.FC<ZoneConfigProps> = ({ selectedField, selectedZone, saveAllZones }) => {
    //Official FIRST hex
    // TODO: Do we want to eventually make these editable?
    const redMaterial = new THREE.MeshPhongMaterial({
        color: 0xed1c24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    const blueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0066b3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    }) //0x0000ff

    const [name, setName] = useState<string>(selectedZone.name)
    const [alliance, setAlliance] = useState<Alliance>(selectedZone.alliance)
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(selectedZone.parentNode)
    const [points, setPoints] = useState<number>(selectedZone.points)
    const [destroy] = useState<boolean>(selectedZone.destroyGamepiece)
    const [persistent, setPersistent] = useState<boolean>(selectedZone.persistentPoints)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const { currentTheme, themes } = useTheme()
    const theme = useMemo(() => {
        return themes[currentTheme]
    }, [currentTheme, themes])

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedField) {
            save(selectedField, selectedZone, name, alliance, points, destroy, persistent, gizmoRef.current, selectedNode)
            saveAllZones()
        }
    }, [
        selectedField,
        selectedZone,
        name,
        alliance,
        points,
        destroy,
        persistent,
        selectedNode,
        saveAllZones,
    ])

    useEffect(() => {
        ConfigurationSavedEvent.Listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.RemoveListener(saveEvent)
        }
    }, [saveEvent])

    useEffect(() => {
        World.PhysicsSystem.HoldPause()

        return () => {
            World.PhysicsSystem.ReleasePause()
        }
    }, [])

    useEffect(() => {
        const field = selectedField
        const zone = selectedZone

        if (!field || !zone) {
            return
        }

        const gizmo = new GizmoSceneObject(
            new THREE.Mesh(new THREE.BoxGeometry(1, 1, 1), zone.alliance == "blue" ? blueMaterial : redMaterial),
            "translate",
            1.5
        )

        ;((gizmo.obj as THREE.Mesh).material as THREE.Material).depthTest = false

        const deltaTransformation = Array_ThreeMatrix4(zone.deltaTransformation)

        let nodeBodyId = field.mechanism.nodeToBody.get(zone.parentNode ?? field.rootNodeId)
        if (!nodeBodyId) {
            // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
            nodeBodyId = field.mechanism.nodeToBody.get(field.rootNodeId)!
        }

        /** W = L x R. See save() for math details */
        const fieldTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
        const props = DeltaFieldTransforms_VisualProperties(deltaTransformation, fieldTransformation)

        gizmo.obj.position.set(props.translation.x, props.translation.y, props.translation.z)
        gizmo.obj.rotation.setFromQuaternion(props.rotation)
        gizmo.obj.scale.set(props.scale.x, props.scale.y, props.scale.z)

        return () => {
            World.SceneRenderer.RemoveSceneObject(gizmo.id)
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [theme, gizmoRef.current])

    /** Sets the selected node if it is a part of the currently loaded field */
    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            if (!selectedField) {
                return false
            }

            const assoc = World.PhysicsSystem.GetBodyAssociation(body) as RigidNodeAssociate
            if (!assoc || assoc?.sceneObject != selectedField) {
                return false
            }

            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [selectedField]
    )
    
    const gizmoComponent = useMemo(() => {
        return (selectedField && selectedZone) ? (<TransformGizmoControl
            key="zone-transform-gizmo"
            size={1.5}
            gizmoRef={gizmoRef}
            defaultMode="translate"
        />) : (<></>);
    }, [selectedField, selectedZone])

    return (
        <div className="flex flex-col gap-2 bg-background-secondary rounded-md p-2">
            {/** Set the zone name */}
            <Input label="Name" placeholder="Enter zone name" defaultValue={selectedZone.name} onInput={setName} />

            {/** Set the alliance color */}
            <Button
                value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                onClick={() => {
                    setAlliance(alliance == "blue" ? "red" : "blue")
                    if (gizmoRef.current)
                        (gizmoRef.current.obj as THREE.Mesh).material = alliance == "blue" ? redMaterial : blueMaterial
                }}
                colorOverrideClass={`bg-match-${alliance}-alliance`}
            />

            {/** Select a parent node */}
            <SelectButton
                placeholder="Select parent node"
                value={selectedNode}
                onSelect={(body: Jolt.Body) => trySetSelectedNode(body.GetID())}
            />

            {/** Set the point value */}
            <NumberInput
                label="Points"
                placeholder="Zone points"
                defaultValue={selectedZone.points}
                onInput={v => setPoints(v || 1)}
            />

            {/** When checked, the zone will destroy gamepieces it comes in contact with */}
            {/** <Checkbox
                    label="Destroy Gamepiece"
                    defaultState={selectedZone.destroyGamepiece}
                    onClick={setDestroy}
                /> */}

            {/** When checked, points will stay even when a gamepiece leaves the zone */}
            <Checkbox label="Persistent Points" defaultState={selectedZone.persistentPoints} onClick={setPersistent} />

            {/** Switch between transform control modes */}

            {gizmoComponent}
        </div>
    )
}

export default ZoneConfigInterface
