import type { MessageWithTimestamp } from "@/systems/multiplayer/MultiplayerTypes.ts"
import type { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import type { ServerToClientMessage } from "@/systems/multiplayer/bindings/ServerToClientMessage.ts"

export const CLIENT_PREFIX = 0b00000001
export const SERVER_PREFIX = 0b00000011

export abstract class MultiplayerTransport {
    public onServerMessage?: (msg: ServerToClientMessage) => void
    public onPeerMessage?: (msg: MessageWithTimestamp) => void
    public onOpen?: ((this: MultiplayerTransport, ev: Event | null) => unknown) | null
    public onClose?: ((this: MultiplayerTransport, ev: CloseEvent | null) => unknown) | null
    public onError?: ((this: MultiplayerTransport, ev: Event) => unknown) | null

    public abstract get ready(): boolean

    public static init(roomId: string | null, displayName: string, ws: MultiplayerTransport): MultiplayerTransport {
        const initialMessage: ClientToServerMessage = {
            type: "initializeconnection",
            room_id: roomId,
            name: displayName,
        }

        if (ws.ready) {
            ws.sendServer(initialMessage)
        } else {
            ws.onOpen = () => {
                ws.sendServer(initialMessage)
            }
        }

        return ws
    }

    protected abstract send(prefix: number, msg: MessageWithTimestamp | ClientToServerMessage, datagram: boolean): void

    public sendPeer(msg: MessageWithTimestamp, datagram: boolean = false): void {
        return this.send(CLIENT_PREFIX, msg, datagram)
    }

    public sendServer(msg: ClientToServerMessage, datagram: boolean = false): void {
        return this.send(SERVER_PREFIX, msg, datagram)
    }

    public abstract close(code?: number, reason?: string): void
}
