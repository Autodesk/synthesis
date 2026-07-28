import { Tooltip, Typography } from "@mui/material"
import { Box, Stack } from "@mui/system"
import type React from "react"
import { useCallback, useEffect, useState } from "react"
import Label from "@/components/Label.tsx"
import { type ClientAndLatencyInfo, shortClientId } from "@/systems/multiplayer/MultiplayerTypes.ts"
import World from "@/systems/World.ts"
import EventSystem from "@/systems/EventSystem.ts"

const PING_COLOR_THRESHOLDS = [
    [200, "#c81c00"],
    [100, "#ca6a00"],
    [50, "#b3b800"],
    [5, "#359a00"],
    [0, "#00c500"],
] as [number, string][]
const FALLBACK_COLOR = "#9c9c9c"

const MultiplayerHUD: React.FC = () => {
    const [roomCode, setRoomCode] = useState<string | null>()
    const [peers, setPeers] = useState<ClientAndLatencyInfo[]>([])

    const getColor = useCallback((latency: number) => {
        return PING_COLOR_THRESHOLDS.find(v => v[0] < latency)?.[1] ?? FALLBACK_COLOR
    }, [])

    useEffect(() => {
        const unsubscribers: (() => void)[] = []
        unsubscribers.push(
            EventSystem.listen("MultiplayerStateJoinRoom", () => {
                setRoomCode(World.multiplayerSystem?.roomId ?? null)
                setPeers(World.multiplayerSystem == null ? [] : [World.multiplayerSystem.info])
            })
        )
        unsubscribers.push(
            EventSystem.listen("MultiplayerStatePeerChange", () => {
                if (!World.multiplayerSystem) return
                setPeers([World.multiplayerSystem.info, ...World.multiplayerSystem.peerInfo])
            })
        )
        return () => {
            unsubscribers.forEach(unsubscriber => unsubscriber())
        }
    })

    useEffect(() => {
        const interval = setInterval(() => {
            if (World.multiplayerSystem) {
                setPeers([World.multiplayerSystem.info, ...World.multiplayerSystem.peerInfo])
            }
        }, 1000)
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
                    let latencyColor: string = FALLBACK_COLOR
                    let latencyMessage: string | null
                    if (peer.latency == null || peer.lastUpdateTime == null) {
                        latencyMessage = "Unknown"
                    } else if (Date.now() - peer.lastUpdateTime > 7000) {
                        latencyMessage = "Timeout"
                    } else {
                        latencyColor = getColor(peer.latency)
                        latencyMessage = `${peer.latency.toFixed()}ms`
                    }

                    return (
                        <Tooltip
                            placement="right"
                            key={peer.clientId}
                            title={`${shortClientId(peer)} (${latencyMessage})`}
                        >
                            <Stack direction="row" alignItems="center" gap={0.5}>
                                <Box
                                    sx={{
                                        height: "10px",
                                        width: "10px",
                                        backgroundColor: latencyColor,
                                        borderRadius: "50%",
                                        display: "inline-block",
                                    }}
                                />
                                <Typography sx={{ flexGrow: 1 }} variant={"body1"}>
                                    {peer.displayName || shortClientId(peer)}
                                    {isSelf && " (you)"}
                                </Typography>
                            </Stack>
                        </Tooltip>
                    )
                })}
            </Stack>
        )
    )
}

export default MultiplayerHUD
