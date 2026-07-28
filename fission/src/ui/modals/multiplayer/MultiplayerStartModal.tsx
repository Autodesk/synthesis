import type React from "react"
import { useLayoutEffect, useState } from "react"
import type { ModalImplProps } from "@/components/Modal.tsx"
import { useUIContext } from "../../helpers/UIProviderHelpers.ts"
import ConnectionModal from "@/modals/multiplayer/ConnectionModal.tsx"
import RoomModal from "@/modals/multiplayer/RoomModal.tsx"
import type { RoomInfo } from "@/systems/multiplayer/bindings/RoomInfo.ts"
import type MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"

export interface MultiplayerInitProps {
    displayName: string
    ws: MultiplayerWebsocket
}
interface MultiplayerStartMenuCustomProps {
    startWorldCallback: (initData: MultiplayerInitProps) => Promise<boolean>
}

const MultiplayerStartModal: React.FC<ModalImplProps<void, MultiplayerStartMenuCustomProps>> = ({ modal }) => {
    const { configureScreen } = useUIContext()
    const [roomList, setRoomList] = useState<RoomInfo[]>([])
    const [url, setUrl] = useState<string | null>(null)
    const [page, setPage] = useState<"url" | "room">("url")

    const { startWorldCallback } = modal!.props.custom

    useLayoutEffect(() => {
        configureScreen(
            modal!,
            { title: "Start Multiplayer", hideAccept: true, hideCancel: true, allowClickAway: false },
            {}
        )
    }, [configureScreen, modal])

    return page == "url" ? (
        <ConnectionModal setRoomList={setRoomList} setURL={setUrl} onNext={() => setPage("room")} />
    ) : (
        <RoomModal
            initialRoomList={roomList}
            url={url!}
            startWorldCallback={startWorldCallback}
            onBack={() => setPage("url")}
        />
    )
}

export default MultiplayerStartModal
