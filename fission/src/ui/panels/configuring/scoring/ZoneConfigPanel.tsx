import { useCallback, useEffect, useMemo, useState } from "react"
import Input from "@/components/Input"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import Button from "@/components/Button"
import Checkbox from "@/components/Checkbox"
import NumberInput from "@/components/NumberInput"
import { SelectedZone } from "./ScoringZonesPanel"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { usePanelControlContext } from "@/ui/PanelContext"
import SelectButton from "@/ui/components/SelectButton"
import Jolt from "@barclah/jolt-physics"
import TransformGizmos, { GizmoTransformMode } from "@/ui/components/TransformGizmos"
import * as THREE from "three"
import World from "@/systems/World"
import { Array_ThreeMatrix4, JoltMat44_ThreeMatrix4, ThreeMatrix4_Array } from "@/util/TypeConversions"
import { useTheme } from "@/ui/ThemeContext"
import { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"
import { Alliance } from "@/systems/preferences/PreferenceTypes"
import { RgbaColor } from "react-colorful"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import { DeltaFieldTransforms_PhysicalProp as DeltaFieldTransforms_VisualProperties } from "@/util/threejs/MeshCreation"

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
    name: string,
    alliance: Alliance,
    points: number,
    destroy: boolean,
    persistent: boolean,
    gizmo: TransformGizmos,
    selectedNode?: RigidNodeId
) {
    const field = SelectedZone.field
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
    gizmo.mesh.matrixWorld.decompose(translation, rotation, scale)

    const gizmoTransformation = new THREE.Matrix4().compose(translation, rotation, scale)
    const fieldTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
    const deltaTransformation = gizmoTransformation.premultiply(fieldTransformation.invert())

    const zone = SelectedZone.zone

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

const ZoneConfigPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    //Official FIRST hex
    const redMaterial =  new THREE.MeshPhongMaterial({
        color: 0xED1C24,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })
    const blueMaterial = new THREE.MeshPhongMaterial({
        color: 0x0066B3,
        shininess: 0.0,
        opacity: 0.7,
        transparent: true,
    })  //0x0000ff
    const { openPanel, closePanel } = usePanelControlContext()

    const [name, setName] = useState<string>(SelectedZone.zone.name)
    const [alliance, setAlliance] = useState<Alliance>(SelectedZone.zone.alliance)
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(SelectedZone.zone.parentNode)
    const [points, setPoints] = useState<number>(SelectedZone.zone.points)
    const [destroy, setDestroy] = useState<boolean>(SelectedZone.zone.destroyGamepiece)
    const [persistent, setPersistent] = useState<boolean>(SelectedZone.zone.persistentPoints)

    const [transformGizmo, setTransformGizmo] = useState<TransformGizmos | undefined>(undefined)
    const [transformMode, setTransformMode] = useState<GizmoTransformMode>("translate")

    const { currentTheme, themes } = useTheme()
    const theme = useMemo(() => {
        return themes[currentTheme]
    }, [currentTheme, themes])

    useEffect(() => {
        closePanel("scoring-zones")

        World.PhysicsSystem.HoldPause()

        return () => {
            World.PhysicsSystem.ReleasePause()
        }
    }, [])

    useEffect(() => {
        const field = SelectedZone.field
        const zone = SelectedZone.zone

        if (!field || !zone) {
            setTransformGizmo(undefined)
            return
        }

        const gizmo = new TransformGizmos(
            new THREE.Mesh(
                new THREE.BoxGeometry(1, 1, 1),
                zone.alliance == "blue" ? blueMaterial : redMaterial
            )
        );

        (gizmo.mesh.material as THREE.Material).depthTest = false
        gizmo.AddMeshToScene()
        gizmo.CreateGizmo("translate", 1.5)

        const deltaTransformation = Array_ThreeMatrix4(zone.deltaTransformation)

        let nodeBodyId = field.mechanism.nodeToBody.get(zone.parentNode ?? field.rootNodeId)
        if (!nodeBodyId) {
            // In the event that something about the id generation for the rigid nodes changes and parent node id is no longer in use
            nodeBodyId = field.mechanism.nodeToBody.get(field.rootNodeId)!
        }

        /** W = L x R. See save() for math details */
        const fieldTransformation = JoltMat44_ThreeMatrix4(World.PhysicsSystem.GetBody(nodeBodyId).GetWorldTransform())
        const props = DeltaFieldTransforms_VisualProperties(deltaTransformation, fieldTransformation)


        gizmo.mesh.position.set(props.translation.x, props.translation.y, props.translation.z)
        gizmo.mesh.rotation.setFromQuaternion(props.rotation)
        gizmo.mesh.scale.set(props.scale.x, props.scale.y, props.scale.z)

        setTransformGizmo(gizmo)

        return () => {
            gizmo.RemoveGizmos()
            setTransformGizmo(undefined)
        }
    }, [theme])

    /** Sets the selected node if it is a part of the currently loaded field */
    const trySetSelectedNode = useCallback((body: Jolt.BodyID) => {
        if (!SelectedZone.field) {
            return false
        }

        const assoc = World.PhysicsSystem.GetBodyAssociation(body) as RigidNodeAssociate
        if (!assoc || assoc?.sceneObject != SelectedZone.field) {
            return false
        }

        setSelectedNode(assoc.rigidNodeId)
        return true
    }, [])

    return (
        <Panel
            name="Scoring Zone Config"
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                if (transformGizmo && SelectedZone.field) {
                    save(name, alliance, points, destroy, persistent, transformGizmo, selectedNode)
                }
                openPanel("scoring-zones")
            }}
            onCancel={() => {
                openPanel("scoring-zones")
            }}
        >
            <div className="flex flex-col gap-2 bg-background-secondary rounded-md p-2">
                {/** Set the zone name */}
                <Input
                    label="Name"
                    placeholder="Enter zone name"
                    defaultValue={SelectedZone.zone.name}
                    onInput={setName}
                />

                {/** Set the alliance color */}
                <Button
                    value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                    onClick={() => {
                        setAlliance(alliance == "blue" ? "red" : "blue")
                        if (transformGizmo) {
                            transformGizmo.mesh.material = alliance == "blue" ? redMaterial : blueMaterial
                        }
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
                    defaultValue={SelectedZone.zone.points}
                    onInput={v => setPoints(v || 1)}
                />

                {/** When checked, the zone will destroy gamepieces it comes in contact with */}
                <Checkbox
                    label="Destroy Gamepiece"
                    defaultState={SelectedZone.zone.destroyGamepiece}
                    onClick={setDestroy}
                />

                {/** When checked, points will stay even when a gamepiece leaves the zone */}
                <Checkbox
                    label="Persistent Points"
                    defaultState={SelectedZone.zone.persistentPoints}
                    onClick={setPersistent}
                />

                {/** Switch between transform control modes */}

                <ToggleButtonGroup
                    value={transformMode}
                    exclusive
                    onChange={(_, v) => {
                        if (v == undefined) return

                        setTransformMode(v)
                        transformGizmo?.SwitchGizmo(v, 1.5)
                    }}
                    sx={{
                        alignSelf: "center",
                    }}
                >
                    <ToggleButton value={"translate"}>Move</ToggleButton>
                    <ToggleButton value={"scale"}>Scale</ToggleButton>
                    <ToggleButton value={"rotate"}>Rotate</ToggleButton>
                </ToggleButtonGroup>
            </div>
        </Panel>
    )
}

const colorToNumber = (color: RgbaColor) => {
    return color.r * 10000 + color.g * 100 + color.b
}

export default ZoneConfigPanel
