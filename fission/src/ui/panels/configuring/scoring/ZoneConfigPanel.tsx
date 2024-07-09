import { useState } from "react"
import Input from "@/components/Input"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import Button from "@/components/Button"
import SelectButton from "@/components/SelectButton"
import Checkbox from "@/components/Checkbox"
import NumberInput from "@/components/NumberInput"
import { SelectedZone } from "./ScoringZonesPanel"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import { usePanelControlContext } from "@/ui/PanelContext"

const ZoneConfigPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const { openPanel } = usePanelControlContext()

    const [name, setName] = useState<string>(SelectedZone.zone.name)
    const [alliance, setAlliance] = useState<"red" | "blue">(SelectedZone.zone.alliance)
    const [parent, setParent] = useState<string>(SelectedZone.zone.parent)
    const [points, setPoints] = useState<number>(SelectedZone.zone.points)
    const [destroy, setDestroy] = useState<boolean>(SelectedZone.zone.destroyGamepiece)
    const [persistent, setPersistent] = useState<boolean>(SelectedZone.zone.persistentPoints)
    const [scale, setScale] = useState<[number, number, number]>(SelectedZone.zone.scale)

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
                SelectedZone.zone.scale = scale
                
                PreferencesSystem.savePreferences()
                openPanel("scoring-zones")
            }
        }
        >
            <Input label="Name" placeholder="Enter zone name" defaultValue={SelectedZone.zone.name} onInput={setName} />
            <Button
                value={`${alliance[0].toUpperCase() + alliance.substring(1)} Alliance`}
                onClick={() => setAlliance(alliance == "blue" ? "red" : "blue")}
                colorOverrideClass={`bg-match-${alliance}-alliance`}
            />
            <SelectButton placeholder="Select zone parent" onSelect={(p: string) => setParent(p)} />
            <NumberInput label="Points" placeholder="Zone points" defaultValue={SelectedZone.zone.points} onInput={v => setPoints(v || 1)} />
            <Checkbox
                label="Destroy Gamepiece"
                defaultState={SelectedZone.zone.destroyGamepiece}
                onClick={setDestroy}
            />
            <Checkbox label="Persistent Points" defaultState={SelectedZone.zone.persistentPoints} onClick={setPersistent} />
            {/* <Slider
                label="X Scale"
                min={0}
                max={10}
                defaultValue={1}
                format={{ maximumFractionDigits: 2 }}
                onChange={(v: number) =>
                    setScale(s => {
                        s[0] = v
                        return s
                    })
                }
            />
            <Slider
                label="Y Scale"
                min={0}
                max={10}
                defaultValue={1}
                format={{ maximumFractionDigits: 2 }}
                onChange={(v: number) =>
                    setScale(s => {
                        s[1] = v
                        return s
                    })
                }
            />
            <Slider
                label="Z Scale"
                min={0}
                max={10}
                defaultValue={1}
                format={{ maximumFractionDigits: 2 }}
                onChange={(v: number) =>
                    setScale(s => {
                        s[2] = v
                        return s
                    })
                }
            /> */}
        </Panel>
    )
}

export default ZoneConfigPanel
