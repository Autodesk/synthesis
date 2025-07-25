import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import Input from "@/components/Input"
import Button from "@/components/Button"
import NumberInput from "@/components/NumberInput"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SelectButton from "@/ui/components/SelectButton"
import Jolt from "@azaleacolburn/jolt-physics"
import * as THREE from "three"
import World from "@/systems/World"
import {
    convertArrayToThreeMatrix4,
    convertJoltMat44ToThreeMatrix4,
    convertThreeMatrix4ToArray,
} from "@/util/TypeConversions"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import { Alliance, ProtectedZonePreferences } from "@/systems/preferences/PreferenceTypes"
import { RigidNodeId } from "@/mirabuf/MirabufParser"
import { deltaFieldTransformsPhysicalProp } from "@/util/threejs/MeshCreation"
import { ConfigurationSavedEvent } from "../../ConfigurationSavedEvent"
import GizmoSceneObject from "@/systems/scene/GizmoSceneObject"
import TransformGizmoControl from "@/ui/components/TransformGizmoControl"
import { PAUSE_REF_ASSEMBLY_CONFIG } from "@/systems/physics/PhysicsSystem"
import ProtectedZoneSceneObject, { ContactType } from "@/mirabuf/ProtectedZoneSceneObject"
import Dropdown from "@/ui/components/Dropdown"
import { MatchModeType } from "@/systems/MatchMode"

const MATCH_MODE_OPTIONS: MatchModeType[] = [
    MatchModeType.SANDBOX,
    MatchModeType.AUTONOMOUS,
    MatchModeType.TELEOP,
    MatchModeType.ENDGAME,
]

const CONTACT_TYPE_OPTIONS = Object.values(ContactType)

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
 * @param activeDuring Array of match mode types during which the zone is active.
 * @param gizmo Reference to the transform gizmo object.
 * @param selectedNode Selected node that configuration is relative to.
 */
function save(
    field: MirabufSceneObject,
    zone: ProtectedZonePreferences,
    name: string,
    alliance: Alliance,
    points: number,
    contactType: ContactType,
    activeDuring: MatchModeType[],
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
    zone.contactType = contactType
    zone.activeDuring = activeDuring

    if (!field.fieldPreferences.protectedZones.includes(zone)) field.fieldPreferences.protectedZones.push(zone)

    PreferencesSystem.savePreferences()
}

interface ZoneConfigProps {
    selectedField: MirabufSceneObject
    selectedZone: ProtectedZonePreferences
    saveAllZones: () => void
}

const ZoneConfigInterface: React.FC<ZoneConfigProps> = ({ selectedField, selectedZone, saveAllZones }) => {
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
    const [contactType, setContactType] = useState<ContactType>(selectedZone.contactType || ContactType.ROBOT_ENTERS)
    const [activeDuring, setActiveDuring] = useState<MatchModeType[]>(selectedZone.activeDuring)

    const gizmoRef = useRef<GizmoSceneObject | undefined>(undefined)

    const saveEvent = useCallback(() => {
        if (gizmoRef.current && selectedField) {
            save(
                selectedField,
                selectedZone,
                name,
                alliance,
                points,
                contactType,
                activeDuring,
                gizmoRef.current,
                selectedNode
            )
            saveAllZones()
        }
    }, [selectedField, selectedZone, name, alliance, points, contactType, activeDuring, selectedNode, saveAllZones])

    useEffect(() => {
        ConfigurationSavedEvent.listen(saveEvent)

        return () => {
            ConfigurationSavedEvent.removeListener(saveEvent)
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
            selectedZone.alliance == "blue" ? blueMaterial : redMaterial
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
                const props = deltaFieldTransformsPhysicalProp(deltaTransformation, fieldTransformation)

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
            if (!assoc || assoc?.sceneObject != selectedField) {
                return false
            }

            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [selectedField]
    )

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

            {/** Set the penalty value */}
            <NumberInput
                label="Penalty Points"
                placeholder="Zone penalty points"
                defaultValue={selectedZone.penaltyPoints}
                onInput={v => setPoints(v || 1)}
            />

            {/** Determines during what game state the protected zone is active */}
            <Dropdown
                label="Active During"
                options={MATCH_MODE_OPTIONS}
                onSelect={(selectedOptions: string[]) => {
                    setActiveDuring(selectedOptions as MatchModeType[])
                }}
                defaultValue={activeDuring}
                maxWidth="15rem"
                multiSelect={true}
                textAlign="left"
            />

            {/** Determines what type of contact is required for the penalty to apply */}
            <Dropdown
                label="Contact Type"
                options={CONTACT_TYPE_OPTIONS}
                onSelect={(selectedOption: string) => {
                    setContactType(selectedOption as ContactType)
                }}
                defaultValue={contactType}
                textAlign="left"
            />

            {gizmoComponent}
        </div>
    )
}

export default ZoneConfigInterface
