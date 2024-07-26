import { useCallback, useEffect, useState } from "react"
import Label, { LabelSize } from "@/components/Label"
import Panel, { PanelPropsImpl } from "@/components/Panel"
import Stack, { StackDirection } from "@/components/Stack"
import { OnScoreChangedEvent } from "@/mirabuf/ScoringZoneSceneObject"
import { usePanelControlContext } from "@/ui/PanelContext"
import PreferencesSystem, { PreferenceEvent } from "@/systems/preferences/PreferencesSystem"

const ScoreboardPanel: React.FC<PanelPropsImpl> = ({ panelId, openLocation, sidePadding }) => {
    const [redScore, setRedScore] = useState<number>(0)
    const [blueScore, setBlueScore] = useState<number>(0)
    const [initialTime] = useState<number>(-1)
    const [startTime] = useState<number>(Date.now())
    const [time, setTime] = useState<number>(-1)
    const { closePanel } = usePanelControlContext()

    // probably useless code because the time left should be sent by Synthesis and not calculated here
    // const startTimer = useCallback(
    //     async (t: number) => {
    //         setInitialTime(t)
    //         setTime(t)
    //         setStartTime(Date.now())
    //     },
    //     [setInitialTime, setTime, setStartTime]
    // )

    const onScoreChange = useCallback(
        (e: OnScoreChangedEvent) => {
            setRedScore(e.red)
            setBlueScore(e.blue)
        },
        [setRedScore, setBlueScore]
    )

    const onRenderChange = useCallback(
        (e: PreferenceEvent) => {
            if (e.prefName == "RenderScoreboard" && e.prefValue == false) {
                closePanel("scoreboard")
            }
        },
        [closePanel]
    )

    useEffect(() => {
        const interval: NodeJS.Timeout = setInterval(() => {
            const elapsed = Math.round((Date.now() - startTime) / 1_000)
            if (initialTime > 0) {
                if (elapsed <= initialTime) setTime(initialTime - elapsed)
                else {
                    clearInterval(interval)
                }
            }
        })
    }, [initialTime, time, startTime])

    useEffect(() => {
        OnScoreChangedEvent.AddListener(onScoreChange)
        PreferencesSystem.addEventListener(onRenderChange)
    })

    // useEffect(() => {
    //     if (initialTime == -1) startTimer(15)
    // }, [initialTime, startTimer])

    return (
        <Panel
            panelId={panelId}
            openLocation={openLocation}
            sidePadding={sidePadding}
            cancelEnabled={false}
            acceptEnabled={false}
            contentClassName="mx-0 w-min"
        >
            {/* {time >= 0 && (
                <div className="flex flex-row justify-center pt-4">
                    <Label size={LabelSize.XL}>{time.toFixed(0)}</Label>
                </div>
            )} */}
            <Stack direction={StackDirection.Horizontal} className="px-4 pb-4 pt-4" spacing={16}>
                <div className="flex flex-col items-center text-center justify-center w-20 h-20 rounded-lg bg-match-red-alliance">
                    <Label size={LabelSize.Small}>RED</Label>
                    <Label size={LabelSize.XL}>{redScore}</Label>
                </div>
                <div className="flex flex-col items-center text-center justify-center w-20 h-20 rounded-lg bg-match-blue-alliance">
                    <Label size={LabelSize.Small}>BLUE</Label>
                    <Label size={LabelSize.XL}>{blueScore}</Label>
                </div>
            </Stack>
        </Panel>
    )
}

export default ScoreboardPanel
