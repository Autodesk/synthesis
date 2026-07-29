import type React from "react"
import { useLayoutEffect, useState } from "react"
import type { ModalImplProps } from "@/components/Modal.tsx"
import { CloseType, useUIContext } from "../../helpers/UIProviderHelpers.ts"
import ConnectionModal from "@/modals/multiplayer/ConnectionModal.tsx"
import RoomModal from "@/modals/multiplayer/RoomModal.tsx"
import type { RoomInfo } from "@/systems/multiplayer/bindings/RoomInfo.ts"
import type MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"
import World from "@/systems/World.ts"
import { Button } from "@/components/StyledComponents.tsx"

export interface MultiplayerInitProps {
    displayName: string
    ws: MultiplayerWebsocket
}

const MultiplayerStartModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { configureScreen, closeModal } = useUIContext()
    const [roomList, setRoomList] = useState<RoomInfo[]>([])
    const [url, setUrl] = useState<string | null>(null)
    const [page, setPage] = useState<"url" | "room">("url")

    useLayoutEffect(() => {
        configureScreen(
            modal!,
            { title: "Start Multiplayer", hideAccept: true, hideCancel: false, allowClickAway: false },
            {}
        )
    }, [configureScreen, modal])

    return World.multiplayerSystem != null ? (
        <Button
            onClick={() => {
                World.multiplayerSystem!.destroy()
                closeModal(CloseType.ACCEPT)
            }}
        >
            Disconnect
        </Button>
    ) : page == "url" ? (
        <ConnectionModal setRoomList={setRoomList} setURL={setUrl} onNext={() => setPage("room")} />
    ) : (
        <RoomModal initialRoomList={roomList} url={url!} onBack={() => setPage("url")} />
    )
}

export default MultiplayerStartModal
