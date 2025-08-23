import { Stack } from "@mui/material"
import type React from "react"
import { useCallback, useEffect, useState } from "react"
import Draggable from "react-draggable"
import { useRef } from "react"
import { OnScoreChangedEvent } from "@/mirabuf/ScoringZoneSceneObject"
import MatchMode, { UpdateTimeLeft } from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import Label from "./Label"

const showTime = () => {
    return MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX
}

const HALF_W = "calc(50vw - 50%)"

const Scoreboard: React.FC = () => {
    const [redScore, setRedScore] = useState(ScoreTracker.redScore)
    const [blueScore, setBlueScore] = useState(ScoreTracker.blueScore)
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

    const nodeRef = useRef<HTMLDivElement | null>(null)

    return (
        <Draggable positionOffset={{ x: HALF_W, y: 0 }} nodeRef={nodeRef}>
            <Stack
                direction="column"
                sx={{ bgcolor: "background.paper", position: "absolute", boxShadow: 6 }}
                className="w-min p-2 justify-center align-middle rounded-3xl select-none"
                ref={nodeRef}
            >
                {showTime() && (
                    <Stack direction="row" className="w-full justify-center">
                        <Label size="lg" color="text.primary">
                            {time}
                        </Label>
                    </Stack>
                )}
                <Stack direction="row" className={`px-4 ${showTime() ? "pt-1 pb-4" : "py-4"}`} gap={1}>
                    {/* TODO: change colors? */}
                    <Stack
                        direction="column"
                        className="items-center justify-center w-20 h-20 rounded-lg"
                        sx={{ bgcolor: "redAlliance.main", color: "#fff" }}
                    >
                        <Label size="sm">RED</Label>
                        <Label size="lg">{redScore}</Label>
                    </Stack>
                    <Stack
                        direction="column"
                        className="items-center justify-center w-20 h-20 rounded-lg"
                        sx={{ bgcolor: "blueAlliance.main", color: "#fff" }}
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
