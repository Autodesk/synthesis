import type { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import type { MessageWithTimestamp } from "@/systems/multiplayer/MultiplayerTypes.ts"
import type { ServerToClientMessage } from "@/systems/multiplayer/bindings/ServerToClientMessage.ts"

export interface MultiplayerCommunicationProvider {
    onServerMessage?: (msg: ServerToClientMessage) => void
    onPeerMessage?: (msg: MessageWithTimestamp) => void
    onOpen?: ((v?: Event) => unknown) | null
    onClose?: ((v?: Event) => unknown) | null
    onError?: ((v?: Event) => unknown) | null
    ready: boolean
    sendServer(msg: ClientToServerMessage): void
    sendPeer(msg: MessageWithTimestamp): void
    init(roomId: string | null, displayName: string): MultiplayerCommunicationProvider
    close(): void
}
