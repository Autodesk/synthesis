import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useState } from "react"
import { OnScoreChangedEvent } from "@/mirabuf/ScoringZoneSceneObject"
import MatchMode, { UpdateTimeLeft } from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import SimulationSystem from "@/systems/simulation/SimulationSystem"
import Label from "./Label"
import Draggable from "react-draggable"

const showTime = () => {
    return MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX
}

const HALF_W = "calc(50vw - 50%)"

const Scoreboard: React.FC = () => {
    const [redScore, setRedScore] = useState(SimulationSystem.redScore)
    const [blueScore, setBlueScore] = useState(SimulationSystem.redScore)
    const [time, setTime] = useState("0")

    const onScoreChange = useCallback((e: OnScoreChangedEvent) => {
        setRedScore(e.red)
        setBlueScore(e.blue)
    }, [])

    const onTimeLeftChange = useCallback((e: UpdateTimeLeft) => {
        // TODO: should this change?
        setTime(e.time)
    }, [])

    useEffect(() => {
        OnScoreChangedEvent.addListener(onScoreChange)
        UpdateTimeLeft.addListener(onTimeLeftChange)

        return () => {
            OnScoreChangedEvent.removeListener(onScoreChange)
            UpdateTimeLeft.removeListener(onTimeLeftChange)
        }
    }, [])

    return (
        <Draggable positionOffset={{ x: HALF_W, y: 0 }}>
            <Stack
                direction="column"
                sx={{ bgcolor: "background.default", position: "absolute" }}
                className="w-min p-2 justify-center align-middle rounded-3xl select-none"
            >
                {showTime() && (
                    <Stack direction="row" className="w-full justify-center">
                        <Label size="lg">{time}</Label>
                    </Stack>
                )}
                <Stack direction="row" className={`px-4 ${showTime() ? "pt-1 pb-4" : "py-4"}`} gap={1}>
                    {/* TODO: change colors? */}
                    <Stack
                        direction="column"
                        className="items-center justify-center w-20 h-20 rounded-lg"
                        sx={{ background: "#ff0000", color: "#fff" }}
                    >
                        <Label size="sm">RED</Label>
                        <Label size="lg">{redScore}</Label>
                    </Stack>
                    <Stack
                        direction="column"
                        className="items-center justify-center w-20 h-20 rounded-lg"
                        sx={{ background: "#0000ff", color: "#fff" }}
                    >
                        <Label size="sm">BLUE</Label>
                        <Label size="lg">{blueScore}</Label>
                    </Stack>
                </Stack>
            </Stack>
        </Draggable>
    )
}

export default Scoreboard
