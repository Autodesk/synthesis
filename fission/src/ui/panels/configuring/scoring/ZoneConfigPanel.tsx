import { useEffect, useRef, useState } from "react"
import Input from "@/components/Input"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import Button from "@/components/Button"
import Checkbox from "@/components/Checkbox"
import NumberInput from "@/components/NumberInput"
import { SelectedZone } from "./ScoringZonesPanel"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { usePanelControlContext } from "@/ui/PanelContext"
import Stack, { StackDirection } from "@/ui/components/Stack"
import SelectButton from "@/ui/components/SelectButton"
import Jolt from "@barclah/jolt-physics"
import TransformGizmos from "@/ui/components/TransformGizmos"
import * as THREE from "three"

const ZoneConfigPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const { openPanel, closePanel } = usePanelControlContext()

    const transformGizmoRef = useRef<TransformGizmos>()

    const [name, setName] = useState<string>(SelectedZone.zone.name)
    const [alliance, setAlliance] = useState<"red" | "blue">(SelectedZone.zone.alliance)
    const [parent, setParent] = useState<Jolt.Body | undefined>(SelectedZone.zone.parent)
    const [points, setPoints] = useState<number>(SelectedZone.zone.points)
    const [destroy, setDestroy] = useState<boolean>(SelectedZone.zone.destroyGamepiece)
    const [persistent, setPersistent] = useState<boolean>(SelectedZone.zone.persistentPoints)

    const [transformMode, setTransformMode] = useState<"translate" | "rotate" | "scale">("translate")

    useEffect(() => {
        closePanel("scoring-zones")

        configureGizmo("translate")
    },[])

    const configureGizmo = (mode: "translate" | "rotate" | "scale") => {
        // Remove the old transform gizmo
        if (transformGizmoRef.current) transformGizmoRef.current.RemoveGizmos()

        transformGizmoRef.current = new TransformGizmos(
            new THREE.Mesh(new THREE.BoxGeometry(1, 1, 1), new THREE.MeshBasicMaterial({ color: 0xffffff }))
        )
        transformGizmoRef.current.AddMeshToScene()

        transformGizmoRef.current.CreateGizmo(mode, 1.5)

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
        SelectedZone.zone.parent = parent
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
            <Input label="Name" placeholder="Enter zone name" defaultValue={SelectedZone.zone.name} onInput={setName} />
            <Button
                value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                onClick={() => setAlliance(alliance == "blue" ? "red" : "blue")}
                colorOverrideClass={`bg-match-${alliance}-alliance`}
            />
            <SelectButton
                placeholder="Select pickup node"
                onSelect={(body: Jolt.Body) => (setParent(body))}
            />
            <NumberInput
                label="Points"
                placeholder="Zone points"
                defaultValue={SelectedZone.zone.points}
                onInput={v => setPoints(v || 1)}
            />
            <Checkbox
                label="Destroy Gamepiece"
                defaultState={SelectedZone.zone.destroyGamepiece}
                onClick={setDestroy}
            />
            <Checkbox
                label="Persistent Points"
                defaultState={SelectedZone.zone.persistentPoints}
                onClick={setPersistent}
            />
            <Stack direction={StackDirection.Horizontal} spacing={8}>
                <>
                    <Button
                        value="Move"
                        colorOverrideClass={transformMode != "translate" ? "bg-interactive-background" : undefined}
                        onClick={() => {
                            setTransformMode("translate")

                            transformGizmoRef.current?.SwitchGizmo("translate", 1.5)
                        }}
                    />
                    <Button
                        value="Scale"
                        colorOverrideClass={transformMode != "scale" ? "bg-interactive-background" : undefined}
                        onClick={() => {
                            setTransformMode("scale")

                            transformGizmoRef.current?.SwitchGizmo("scale", 1.5)
                        }}
                    />
                    <Button
                        value="Rotate"
                        colorOverrideClass={transformMode != "rotate" ? "bg-interactive-background" : undefined}
                        onClick={() => {
                            setTransformMode("rotate")

                            transformGizmoRef.current?.SwitchGizmo("rotate", 1.5)
                        }}
                    />
                </>
            </Stack>
        </Panel>
    )
}

export default ZoneConfigPanel
