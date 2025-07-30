import React, {useEffect, useState} from "react"
import {Stack} from "@mui/system";
import World from "@/systems/World.ts";
import {MultiplayerStateEvent, MultiplayerStateEventType} from "@/systems/multiplayer/MultiplayerSystem.ts";
import {ClientInfo} from "@/systems/multiplayer/types.ts";


const MultiplayerHUD: React.FC = () => {
    const [roomCode, setRoomCode] = useState("")
    const [peers, setPeers] = useState<ClientInfo[]>([])
    useEffect(() => {
        const unsubscribers:(() => void)[] = []
        unsubscribers.push(MultiplayerStateEvent.addEventListener(MultiplayerStateEventType.JOIN_ROOM, () => {
            setRoomCode(World.multiplayerSystem?.roomId ?? "")
        }))
        unsubscribers.push(MultiplayerStateEvent.addEventListener(MultiplayerStateEventType.PEER_CHANGE, () => {
            setPeers(World.multiplayerSystem?.peerInfo ?? [])
        }))
        return () => {
            unsubscribers.forEach((unsubscriber) => unsubscriber())
        }
    })
    return (
        <Stack direction="column" position="fixed" left={0} bottom={0}>
            <div>
            Room: {roomCode}
            </div>
            {peers.map((peer) => (
                <div key={peer.clientId}>{peer.isHost&&"* "}{peer.displayName}</div>
            ))}
        </Stack>
    )
}

export default MultiplayerHUD
