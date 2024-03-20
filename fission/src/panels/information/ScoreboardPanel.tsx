import { useCallback, useEffect, useState } from "react"
import Label, { LabelSize } from "../../components/Label"
import Panel, { PanelPropsImpl } from "../../components/Panel"
import Stack, { StackDirection } from "../../components/Stack"

const ScoreboardPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const [redScore] = useState<number>(0)
    const [blueScore] = useState<number>(0)
    const [initialTime, setInitialTime] = useState<number>(-1)
    const [startTime, setStartTime] = useState<number>(Date.now())
    const [time, setTime] = useState<number>(-1)

    // probably useless code because the time left should be sent by Synthesis and not calculated here
    const startTimer = useCallback(
        async (t: number) => {
            setInitialTime(t)
            setTime(t)
            setStartTime(Date.now())
        },
        [setInitialTime, setTime, setStartTime]
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
        if (initialTime == -1) startTimer(15)
    }, [initialTime, startTimer])

    return (
        <Panel
            panelId={panelId}
            cancelEnabled={false}
            acceptEnabled={false}
            contentClassName="mx-0 w-min"
        >
            {time >= 0 && (
                <div className="flex flex-row justify-center pt-4">
                    <Label size={LabelSize.XL}>{time.toFixed(0)}</Label>
                </div>
            )}
            <Stack
                direction={StackDirection.Horizontal}
                className="px-4 pb-4 pt-4"
                spacing={16}
            >
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
