import { useCallback, useEffect, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import Stack, { StackDirection } from "@/components/Stack"
import { OnScoreChangedEvent } from "@/mirabuf/ScoringZoneSceneObject"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import PreferencesSystem, { PreferenceEvent } from "@/systems/preferences/PreferencesSystem"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import MatchMode, { MatchModeType, UpdateTimeLeft } from "@/systems/match_mode/MatchMode"
import { Spacer } from "@/components/StyledComponents"

function showTime(): boolean {
    return MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX
}

const ScoreboardPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [redScore, setRedScore] = useState<number>(SimulationSystem.redScore)
    const [blueScore, setBlueScore] = useState<number>(SimulationSystem.blueScore)
    const [time, setTime] = useState<string>("0")
    const { closePanel } = usePanelControlContext()

    const onScoreChange = useCallback(
        (e: OnScoreChangedEvent) => {
            setRedScore(e.red)
            setBlueScore(e.blue)
        },
        [setRedScore, setBlueScore]
    )

    const onTimeLeftChange = useCallback(
        (e: UpdateTimeLeft) => {
            setTime(e.autonomousTime)
        },
        [setTime]
    )

    const onRenderChange = useCallback(
        (e: PreferenceEvent<"RenderScoreboard">) => {
            if (!e.prefValue) {
                closePanel("scoreboard")
            }
        },
        [closePanel]
    )

    useEffect(() => {
        OnScoreChangedEvent.addListener(onScoreChange)
        UpdateTimeLeft.addListener(onTimeLeftChange)
        const removeListener = PreferencesSystem.addPreferenceEventListener("RenderScoreboard", onRenderChange)
        return () => {
            removeListener()
        }
    })

    return (
        <Panel
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            cancelEnabled={false}
            acceptEnabled={false}
            contentClassName="mx-0 w-min"
        >
            {showTime() ? (
                <div className="flex flex-row justify-center pt-4">
                    <Label size={LabelSize.XL}>{time}</Label>
                </div>
            ) : (
                Spacer(0)
            )}
            <Stack direction={StackDirection.HORIZONTAL} className="px-4 pb-4 pt-1" spacing={16}>
                <div className="flex flex-col items-center text-center justify-center w-20 h-20 rounded-lg bg-match-red-alliance">
                    <Label size={LabelSize.SMALL}>RED</Label>
                    <Label size={LabelSize.XL}>{redScore}</Label>
                </div>
                <div className="flex flex-col items-center text-center justify-center w-20 h-20 rounded-lg bg-match-blue-alliance">
                    <Label size={LabelSize.SMALL}>BLUE</Label>
                    <Label size={LabelSize.XL}>{blueScore}</Label>
                </div>
            </Stack>
        </Panel>
    )
}

export default ScoreboardPanel
