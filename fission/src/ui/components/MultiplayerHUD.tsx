import { Tooltip, Typography } from "@mui/material"
import { Stack } from "@mui/system"
import type React from "react"
import { useEffect, useState } from "react"
import Label from "@/components/Label.tsx"
import type { ClientInfo } from "@/systems/multiplayer/MultiplayerTypes.ts"
import World from "@/systems/World.ts"
import EventSystem from "@/systems/EventSystem.ts"

const MultiplayerHUD: React.FC = () => {
    const [roomCode, setRoomCode] = useState<number | null>()
    const [peers, setPeers] = useState<ClientInfo[]>([])
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
                {peers.map(peer => (
                    <Tooltip placement="right" key={peer.clientId} title={peer.clientId.slice(0, 8)}>
                        <Typography variant={"body1"} key={peer.clientId}>
                            {peer.displayName}
                            {peer.clientId == World.multiplayerSystem?.clientId && " (you)"}
                        </Typography>
                    </Tooltip>
                ))}
            </Stack>
        )
    )
}

export default MultiplayerHUD
