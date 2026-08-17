import { Stack } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import Draggable from "react-draggable"
import { useRef } from "react"
import EventSystem from "@/systems/EventSystem.ts"
import MatchMode from "@/systems/match_mode/MatchMode"
import { MatchModeType } from "@/systems/match_mode/MatchModeTypes"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import Label from "./Label"
import World from "@/systems/World.ts"

const HALF_W = "calc(50vw - 50%)"

const Scoreboard: React.FC = () => {
    const [redScore, setRedScore] = useState(World.scoreTracker?.redScore ?? 0)
    const [blueScore, setBlueScore] = useState(World.scoreTracker?.blueScore ?? 0)
    const [time, setTime] = useState("0")
    const [alwaysOn, setAlwaysOn] = useState(() => PreferencesSystem.getUserPreference("AlwaysShowScoreboard"))
    const [inMatch, setInMatch] = useState(() => MatchMode.getInstance().getMatchModeType() !== MatchModeType.SANDBOX)

    useEffect(() => {
        const unsubs = [
            EventSystem.listen("ScoreChangedEvent", ({ red, blue }) => {
                setRedScore(red)
                setBlueScore(blue)
            }),
            EventSystem.listen("TimeChangedEvent", ({ time }) => setTime(time.toFixed())),
            EventSystem.listen("MatchStateChangedEvent", ({ mode }) => setInMatch(mode !== MatchModeType.SANDBOX)),
            PreferencesSystem.addPreferenceEventListener("AlwaysShowScoreboard", e => setAlwaysOn(e.prefValue)),
        ]
        return () => unsubs.forEach(unsub => unsub())
    }, [])

    const nodeRef = useRef<HTMLDivElement | null>(null)

    if (!alwaysOn && !inMatch) return null

    return (
        <Draggable positionOffset={{ x: HALF_W, y: "var(--top-bar-height, 0px)" }} nodeRef={nodeRef}>
            <Stack
                direction="column"
                sx={{ bgcolor: "background.paper", position: "absolute", boxShadow: 6 }}
                className="w-min p-2 justify-center align-middle rounded-3xl select-none"
                ref={nodeRef}
            >
                {inMatch && (
                    <Stack direction="row" className="w-full justify-center">
                        <Label size="lg" color="text.primary">
                            {time}
                        </Label>
                    </Stack>
                )}
                <Stack direction="row" className={`px-4 ${inMatch ? "pt-1 pb-4" : "py-4"}`} gap={1}>
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
