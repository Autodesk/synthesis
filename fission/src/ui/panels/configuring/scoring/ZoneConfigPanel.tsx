import { useCallback, useEffect, useMemo, useRef, useState } from "react"
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
import TransformGizmos from "@/ui/components/TransformGizmos"
import * as THREE from "three"
import World from "@/systems/World"
import { ReactRgbaColor_ThreeColor } from "@/util/TypeConversions"
import { useTheme } from "@/ui/ThemeContext"
import MirabufSceneObject, { RigidNodeAssociate } from "@/mirabuf/MirabufSceneObject"
import { MiraType } from "@/mirabuf/MirabufLoader"
import { ToggleButton, ToggleButtonGroup } from "@/ui/components/ToggleButtonGroup"

const ZoneConfigPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const { openPanel, closePanel } = usePanelControlContext()

    const transformGizmoRef = useRef<TransformGizmos>()

    const [name, setName] = useState<string>(SelectedZone.zone.name)
    const [alliance, setAlliance] = useState<"red" | "blue">(SelectedZone.zone.alliance)
    const [selectedNode, setSelectedNode] = useState<string | undefined>(SelectedZone.zone.parentNode)
    const [points, setPoints] = useState<number>(SelectedZone.zone.points)
    const [destroy, setDestroy] = useState<boolean>(SelectedZone.zone.destroyGamepiece)
    const [persistent, setPersistent] = useState<boolean>(SelectedZone.zone.persistentPoints)

    const [transformMode, setTransformMode] = useState<"translate" | "rotate" | "scale">("translate")

    const { currentTheme, themes } = useTheme()
    const theme = useMemo(() => {
        return themes[currentTheme]
    }, [currentTheme, themes])

    const field = useMemo(() => {
        const assemblies = [...World.SceneRenderer.sceneObjects.values()]
        for (let i = 0; i < assemblies.length; i++) {
            const assembly = assemblies[i]
            if (!(assembly instanceof MirabufSceneObject)) continue

            if ((assembly as MirabufSceneObject).miraType != MiraType.FIELD) continue

            return assembly
        }

        return undefined
    }, [])

    useEffect(() => {
        closePanel("scoring-zones")
        configureGizmo()
    }, [])

    const configureGizmo = () => {
        transformGizmoRef.current = new TransformGizmos(
            new THREE.Mesh(
                new THREE.BoxGeometry(1, 1, 1),
                World.SceneRenderer.CreateToonMaterial(ReactRgbaColor_ThreeColor(theme.HighlightHover.color))
            )
        )
        transformGizmoRef.current.AddMeshToScene()

        transformGizmoRef.current.CreateGizmo("translate", 1.5)

        const position = SelectedZone.zone.position
        const rotation = SelectedZone.zone.rotation
        const scale = SelectedZone.zone.scale

        transformGizmoRef.current.mesh.position.set(position[0], position[1], position[2])
        transformGizmoRef.current.mesh.rotation.setFromQuaternion(
            new THREE.Quaternion(rotation[0], rotation[1], rotation[2], rotation[3])
        )
        transformGizmoRef.current.mesh.scale.set(scale[0], scale[1], scale[2])
    }

    const saveSettings = () => {
        SelectedZone.zone.name = name
        SelectedZone.zone.alliance = alliance
        SelectedZone.zone.parentNode = selectedNode
        SelectedZone.zone.points = points
        SelectedZone.zone.destroyGamepiece = destroy
        SelectedZone.zone.persistentPoints = persistent

        if (transformGizmoRef.current != undefined) {
            const position = transformGizmoRef.current.mesh.position
            const rotation = transformGizmoRef.current.mesh.quaternion
            const scale = transformGizmoRef.current.mesh.scale

            SelectedZone.zone.position = [position.x, position.y, position.z]
            SelectedZone.zone.rotation = [rotation.x, rotation.y, rotation.z, rotation.w]
            SelectedZone.zone.scale = [scale.x, scale.y, scale.z]
        }

        PreferencesSystem.savePreferences()
    }

    /** Sets the selected node if it is a part of the currently loaded field */
    const trySetSelectedNode = useCallback(
        (body: Jolt.BodyID) => {
            if (SelectedZone.zone == undefined || Object.keys(PreferencesSystem.getAllFieldPreferences()).length == 0)
                return false

            const assoc = World.PhysicsSystem.GetBodyAssociation<RigidNodeAssociate>(body)
            if (!assoc || assoc?.sceneObject != field) {
                return false
            }

            setSelectedNode(assoc.rigidNodeId)
            return true
        },
        [field]
    )

    return (
        <Panel
            name="Scoring Zone Config"
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                saveSettings()
                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()
                openPanel("scoring-zones")
            }}
            onCancel={() => {
                openPanel("scoring-zones")
                if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()
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
                    onClick={() => setAlliance(alliance == "blue" ? "red" : "blue")}
                    colorOverrideClass={`bg-match-${alliance}-alliance`}
                />

                {/** Select a parent node */}
                <SelectButton
                    placeholder="Select parent node"
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
                        transformGizmoRef.current?.SwitchGizmo(v, 1.5)
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
            {/* </Stack> */}
        </Panel>
    )
}

export default ZoneConfigPanel
