import { useState } from "react"
import Input from "@/components/Input"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import Button from "@/components/Button"
import SelectButton from "@/components/SelectButton"
import Checkbox from "@/components/Checkbox"
import Slider from "@/components/Slider"
import NumberInput from "@/components/NumberInput"

export type ScoringZone = {
    name: string
    alliance: "red" | "blue"
    parent: string
    points: number
    destroyGamepiece: boolean
    persistentPoints: boolean
    scale: [number, number, number]
}

const ZoneConfigPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    // somehow get and store which zone is being edited
    // maybe a global ConfigProvider in App.tsx?
    // then set all default values to the state of the zone
    const [, setName] = useState<string>("")
    const [alliance, setAlliance] = useState<"red" | "blue">("blue")
    const [, setParent] = useState<string>("")
    const [, setPoints] = useState<number>(1)
    const [, setDestroy] = useState<boolean>(false)
    const [, setPersistent] = useState<boolean>(false)
    const [, setScale] = useState<[number, number, number]>([1, 1, 1])

    return (
        <Panel name="Scoring Zone Config" panelId={panelId}>
            <Input
                label="Name"
                placeholder="Enter zone name"
                onInput={setName}
            />
            <Button
                value={`${
                    alliance[0].toUpperCase() + alliance.substring(1)
                } Alliance`}
                onClick={() => setAlliance(alliance == "blue" ? "red" : "blue")}
                colorOverrideClass={`bg-match-${alliance}-alliance`}
            />
            <SelectButton
                placeholder="Select zone parent"
                onSelect={(p: string) => setParent(p)}
            />
            <NumberInput
                label="Points"
                placeholder="Zone points"
                defaultValue={1}
                onInput={v => setPoints(v || 1)}
            />
            <Checkbox
                label="Destroy Gamepiece"
                defaultState={false}
                onClick={setDestroy}
            />
            <Checkbox
                label="Persistent Points"
                defaultState={false}
                onClick={setPersistent}
            />
            <Slider
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
            />
        </Panel>
    )
}

export default ZoneConfigPanel
