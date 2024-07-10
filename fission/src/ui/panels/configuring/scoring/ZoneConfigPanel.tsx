import { useEffect, useState } from "react"
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

const ZoneConfigPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const { openPanel } = usePanelControlContext()

    const [name, setName] = useState<string>(SelectedZone.zone.name)
    const [alliance, setAlliance] = useState<"red" | "blue">(SelectedZone.zone.alliance)
    const [parent, setParent] = useState<Jolt.Body | undefined>(SelectedZone.zone.parent)
    const [points, setPoints] = useState<number>(SelectedZone.zone.points)
    const [destroy, setDestroy] = useState<boolean>(SelectedZone.zone.destroyGamepiece)
    const [persistent, setPersistent] = useState<boolean>(SelectedZone.zone.persistentPoints)

    const [transformMode, setTransformMode] = useState<"translate" | "rotate" | "scale">("translate")

    useEffect(() => {
        // TODO: create transform gizmo
    })

    return (
        <Panel
            name="Scoring Zone Config"
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            onAccept={() => {
                SelectedZone.zone.name = name
                SelectedZone.zone.alliance = alliance
                SelectedZone.zone.parent = parent
                SelectedZone.zone.points = points
                SelectedZone.zone.destroyGamepiece = destroy
                SelectedZone.zone.persistentPoints = persistent

                // TODO: Yoink transform info from the transform gizmo

                PreferencesSystem.savePreferences()
                openPanel("scoring-zones")
            }}
        >
            <Input label="Name" placeholder="Enter zone name" defaultValue={SelectedZone.zone.name} onInput={setName} />
            <Button
                value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                onClick={() => setAlliance(alliance == "blue" ? "red" : "blue")}
                colorOverrideClass={`bg-match-${alliance}-alliance`}
            />
            <SelectButton placeholder="Select zone parent" onSelect={(p: Jolt.Body) => setParent(p)} />
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
                                // TODO: Switch the transform gizmo to translate mode
                            }}
                        />
                        <Button
                            value="Scale"
                            colorOverrideClass={transformMode != "scale" ? "bg-interactive-background" : undefined}
                            onClick={() => {
                                setTransformMode("scale")
                                // TODO: Switch the transform gizmo to translate mode
                            }}
                        />
                        <Button
                            value="Rotate"
                            colorOverrideClass={transformMode != "rotate" ? "bg-interactive-background" : undefined}
                            onClick={() => {
                                setTransformMode("rotate")
                                // TODO: Switch the transform gizmo to translate mode
                            }}
                        />
                    </>
            </Stack>
        </Panel>
    )
}

export default ZoneConfigPanel
