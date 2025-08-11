import { Tooltip, Typography } from "@mui/material"
import { Stack } from "@mui/system"
import type React from "react"
import { useEffect, useState } from "react"
import Label from "@/components/Label.tsx"
import { MultiplayerStateEvent, MultiplayerStateEventType } from "@/systems/multiplayer/MultiplayerSystem.ts"
import type { ClientInfo } from "@/systems/multiplayer/types.ts"
import World from "@/systems/World.ts"

const MultiplayerHUD: React.FC = () => {
    const [roomCode, setRoomCode] = useState("")
    const [peers, setPeers] = useState<ClientInfo[]>([])
    useEffect(() => {
        const unsubscribers: (() => void)[] = []
        unsubscribers.push(
            MultiplayerStateEvent.addEventListener(MultiplayerStateEventType.JOIN_ROOM, () => {
                if (!World.multiplayerSystem) return
                setRoomCode(World.multiplayerSystem.roomId)
                setPeers([World.multiplayerSystem.info])
            })
        )
        unsubscribers.push(
            MultiplayerStateEvent.addEventListener(MultiplayerStateEventType.PEER_CHANGE, () => {
                if (!World.multiplayerSystem) return
                setPeers([World.multiplayerSystem.info, ...World.multiplayerSystem.peerInfo])
            })
        )
        return () => {
            unsubscribers.forEach(unsubscriber => unsubscriber())
        }
    })
    return (
        roomCode && (
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
                    <Tooltip placement="right" key={peer.clientId} title={peer.clientId.split("-")[0]}>
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
