import {Tooltip, Typography} from "@mui/material"
import {Box, Stack} from "@mui/system"
import React, {useMemo} from "react"
import {useEffect, useState} from "react"
import Label from "@/components/Label.tsx"
import {type ClientInfo, shortClientId} from "@/systems/multiplayer/MultiplayerTypes.ts"
import World from "@/systems/World.ts"
import EventSystem from "@/systems/EventSystem.ts"

const PING_COLOR_THRESHOLDS = [
    [200, "#7b1200"],
    [100, "#7b3e00"],
    [50, "#777b00"],
    [0, "#007b00"],
] as [number, string][]

const MultiplayerHUD: React.FC = () => {
    const [roomCode, setRoomCode] = useState<string | null>()
    const [peers, setPeers] = useState<ClientInfo[]>([])
    const [latency, setLatency] = useState<number>(World.multiplayerSystem?.latencyMS ?? -1)
    const latencyColor = useMemo(() => PING_COLOR_THRESHOLDS.find((v) => v[0] < latency)?.[1] ?? "#9c9c9c", [latency])
    useEffect(() => {
        const unsubscribers: (() => void)[] = []
        unsubscribers.push(
            EventSystem.listen("MultiplayerStateJoinRoom", () => {
                setRoomCode(World.multiplayerSystem?.roomId ?? null)
                setPeers(World.multiplayerSystem == null ? [] : [World.multiplayerSystem.info])
                setLatency(World.multiplayerSystem?.latencyMS ?? -1)
            })
        )
        unsubscribers.push(
            EventSystem.listen("MultiplayerStatePeerChange", () => {
                if (!World.multiplayerSystem) return
                setPeers([World.multiplayerSystem.info, ...World.multiplayerSystem.peerInfo])
                setLatency(World.multiplayerSystem?.latencyMS ?? -1)
            })
        )
        return () => {
            unsubscribers.forEach(unsubscriber => unsubscriber())
        }
    })

    useEffect(() => {
        const interval = setInterval(() => {
            setLatency(World.multiplayerSystem?.latencyMS ?? -1)
        }, 500)
        return () => clearInterval(interval)
    })
    return (
        roomCode != null && (
            <Stack
                direction="column"
                position="fixed"
                left={0}
                bottom={0}
                sx={theme => ({
                    bgcolor: `color-mix(in srgb, ${theme.palette.background.paper}, transparent 40%)`,
                    color: theme.palette.text.primary,
                    borderTopRightRadius: "1rem",
                    pl: "0.4rem",
                    pb: "0.4rem",
                    pt: "0.5rem",
                    pr: "0.7rem",
                })}
            >
                <Label fontWeight={"700"} size={"sm"}>
                    Room {roomCode}
                </Label>
                {peers.map(peer => {
                    const isSelf = peer.clientId == World.multiplayerSystem?.clientId
                    return (
                    <Tooltip placement="right" key={peer.clientId} title={`${shortClientId(peer)} ${isSelf ? `(${latency.toFixed()}ms)` : ""}`}>
                        <Stack direction="row" alignItems="center" gap={0.5}>
                            <Box sx={{
                                height: "10px",
                                width: "10px",
                                backgroundColor: isSelf ? latencyColor : "transparent",
                                borderRadius: "50%",
                                display: "inline-block"
                            }}/>
                            <Typography sx={{flexGrow:1}} variant={"body1"}>
                                {peer.displayName || shortClientId(peer)}
                                {isSelf && " (you)"}
                            </Typography>
                        </Stack>
                    </Tooltip>
                )})}
            </Stack>
        )
    )
}

export default MultiplayerHUD
