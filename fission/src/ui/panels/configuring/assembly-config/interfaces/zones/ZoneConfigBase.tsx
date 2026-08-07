import type Jolt from "@synthesis.adsk/jolt-physics"
import { Button, Stack, TextField } from "@mui/material"
import { useCallback, useMemo, useRef, useState } from "react"
import * as THREE from "three"
import type { RigidNodeId } from "@/mirabuf/MirabufParser"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import type { Alliance } from "@/systems/preferences/PreferenceTypes"
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
import { useHoldPhysicsPause } from "@/util/ReactHooks.ts"
import { useConfigurationSavedListener } from "../../AssemblyConfigHooks"

/**
 * Saves zone configuration to selected field.
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
 * @param name Name given to the zone by the user.
 * @param alliance Zone alliance.
 * @param parentNode Parent node of the zone.
 * @param deltaTransformation Delta transformation of the zone.
 */

export type BaseZonePreferences = {
    name: string
    alliance: Alliance
    parentNode: string | undefined
    deltaTransformation: number[]
}

type AllianceMaterials = {
    red: THREE.Material
    blue: THREE.Material
}

export type ZoneConfigBaseProps<TZone extends BaseZonePreferences> = {
    selectedField: MirabufSceneObject
    selectedZone: TZone
    /** Called to ensure the zone exists in the correct preferences list and to persist. */
    attachAndPersistZone: (zone: TZone, field: MirabufSceneObject) => void
    /** Called by the base right before save to allow updating zone-specific fields from local state. */
    applyExtrasOnSave: (zone: TZone) => void
    /** Called after save to bubble up any UI updates, e.g. refreshing a list. */
    saveAllZones?: () => void
    /** Removes any already-rendered zone object from the field to avoid double-rendering while gizmo is active. */
    removeZoneObject: (field: MirabufSceneObject, zone: TZone) => void
    /** Materials used to visualize the gizmo by alliance. If omitted, defaults will be used. */
    materials?: AllianceMaterials
    /** Optional additional inputs to render below the common fields. */
    children?: React.ReactNode
}

const DEFAULT_RED_MATERIAL = new THREE.MeshPhongMaterial({
    color: 0xed1c24,
    shininess: 0.0,
    opacity: 0.7,
    transparent: true,
})
const DEFAULT_BLUE_MATERIAL = new THREE.MeshPhongMaterial({
    color: 0x0066b3,
    shininess: 0.0,
    opacity: 0.7,
    transparent: true,
})

function computeDeltaFromGizmo(
    field: MirabufSceneObject,
    gizmo: GizmoSceneObject,
    selectedNode?: RigidNodeId
): number[] | undefined {
    selectedNode ??= field.rootNodeId

    const nodeBodyId = field.mechanism.nodeToBody.get(selectedNode)
    if (!nodeBodyId) return undefined

    const translation = new THREE.Vector3(0, 0, 0)
    const rotation = new THREE.Quaternion(0, 0, 0, 1)
    const scale = new THREE.Vector3(1, 1, 1)
    gizmo.obj.matrixWorld.decompose(translation, rotation, scale)
    scale.x = Math.abs(scale.x)
    scale.y = Math.abs(scale.y)
    scale.z = Math.abs(scale.z)

    const gizmoTransformation = new THREE.Matrix4().compose(translation, rotation, scale)
    const fieldTransformation = convertJoltMat44ToThreeMatrix4(
        World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
    )
    const deltaTransformation = gizmoTransformation.premultiply(fieldTransformation.invert())

    return convertThreeMatrix4ToArray(deltaTransformation)
}

function getAllianceMaterial(alliance: Alliance, materials?: AllianceMaterials): THREE.Material {
    if (materials) return alliance === "blue" ? materials.blue : materials.red
    return alliance === "blue" ? DEFAULT_BLUE_MATERIAL : DEFAULT_RED_MATERIAL
}

export default function ZoneConfigBase<TZone extends BaseZonePreferences>(props: ZoneConfigBaseProps<TZone>) {
    const {
        selectedField,
        selectedZone,
        attachAndPersistZone,
        applyExtrasOnSave,
        saveAllZones,
        removeZoneObject,
        materials,
    } = props

    const [name, setName] = useState<string>(selectedZone.name)
    const [alliance, setAlliance] = useState<Alliance>(selectedZone.alliance)
    const [selectedNode, setSelectedNode] = useState<RigidNodeId | undefined>(selectedZone.parentNode)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedField && selectedZone) {
            const delta = computeDeltaFromGizmo(selectedField, gizmoRef.current, selectedNode)
            if (!delta) return

            selectedZone.deltaTransformation = delta
            selectedZone.name = name
            selectedZone.alliance = alliance
            selectedZone.parentNode = selectedNode

            applyExtrasOnSave(selectedZone)
            attachAndPersistZone(selectedZone, selectedField)
            selectedField.savePreferences()
            saveAllZones?.()
        }
    }, [
        selectedField,
        selectedZone,
        name,
        alliance,
        selectedNode,
        applyExtrasOnSave,
        attachAndPersistZone,
        saveAllZones,
    ])

    useConfigurationSavedListener(saveEvent)

    useHoldPhysicsPause()

    const defaultGizmoMesh = useMemo(() => {
        if (!selectedZone) return undefined
        return new THREE.Mesh(new THREE.BoxGeometry(1, 1, 1), getAllianceMaterial(selectedZone.alliance, materials))
    }, [selectedZone, selectedZone?.alliance, materials])

    const postGizmoCreation = useCallback(
        (gizmo: GizmoSceneObject) => {
            const material = (gizmo.obj as THREE.Mesh).material as THREE.Material
            material.depthTest = false

            const deltaTransformation = convertArrayToThreeMatrix4(selectedZone.deltaTransformation)
            let nodeBodyId = selectedField.mechanism.nodeToBody.get(selectedZone.parentNode ?? selectedField.rootNodeId)
            if (!nodeBodyId) {
                nodeBodyId = selectedField.mechanism.nodeToBody.get(selectedField.rootNodeId)!
            }

            const fieldTransformation = convertJoltMat44ToThreeMatrix4(
                World.physicsSystem.getBody(nodeBodyId)!.GetWorldTransform()
            )
            const props = deltaFieldTransformsPhysicalProp(deltaTransformation, fieldTransformation)

            gizmo.obj.position.set(props.translation.x, props.translation.y, props.translation.z)
            gizmo.obj.rotation.setFromQuaternion(props.rotation)
            gizmo.obj.scale.set(props.scale.x, props.scale.y, props.scale.z)

            removeZoneObject(selectedField, selectedZone)
        },
        [selectedField, selectedZone, removeZoneObject]
    )

    const gizmoComponent = useMemo(() => {
        if (!selectedField || !selectedZone) {
            gizmoRef.current = undefined
            return null
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
    }, [selectedField, selectedZone, defaultGizmoMesh, postGizmoCreation])

    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            if (!selectedField) return false
            const assoc = World.physicsSystem.getBodyAssociation(body) as RigidNodeAssociate
            if (!assoc || assoc?.sceneObject !== selectedField) return false
            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [selectedField]
    )

    return (
        <Stack gap={2} className="bg-background-secondary rounded-md p-2">
            <TextField
                label="Name"
                placeholder="Enter zone name"
                defaultValue={selectedZone.name}
                onChange={e => setName(e.target.value)}
            />
            <Button
                onClick={() => {
                    setAlliance(alliance === "blue" ? "red" : "blue")
                    if (gizmoRef.current)
                        (gizmoRef.current.obj as THREE.Mesh).material = getAllianceMaterial(
                            alliance === "blue" ? "red" : "blue",
                            materials
                        )
                }}
                sx={{ bgcolor: alliance === "red" ? "redAlliance.main" : "blueAlliance.main" }}
            >{`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}</Button>
            <SelectButton
                placeholder="Select parent node"
                value={selectedNode}
                onSelect={(body: Jolt.Body) => trySetSelectedNode(body.GetID())}
            />
            {props.children}
            {gizmoComponent}
        </Stack>
    )
}
