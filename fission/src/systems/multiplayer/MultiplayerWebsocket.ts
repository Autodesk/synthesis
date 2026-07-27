import { consolePrefixer } from "console-prefixer"
import { MessageWithTimestamp } from "@/systems/multiplayer/MultiplayerTypes.ts"
import { decode, encode } from "@msgpack/msgpack"
import { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import { ServerMessage } from "@/systems/multiplayer/bindings/ServerMessage.ts"

// const CLIENT_PREFIX = 0b00000001
const SERVER_PREFIX = 0b00000011

const console = consolePrefixer({
    defaultPrefix: {
        text: "[Multiplayer WS]",
        style: "background: linear-gradient(90deg,rgba(255, 165, 0, 1) 0%, rgba(199, 87, 87, 1) 100%); color: white;font-weight:bold; padding:2px; border-radius:2px;",
    },
})

class MultiplayerWebsocket {
    private readonly ws: WebSocket

    public onServerMessage?: (msg: ServerMessage) => void
    public onPeerMessage?: (msg: MessageWithTimestamp) => void
    public onOpen?: ((this: MultiplayerWebsocket, ev: Event) => any) | null;
    public onClose?: ((this: MultiplayerWebsocket, ev: CloseEvent) => any) | null;
    public onError?: ((this: MultiplayerWebsocket, ev: Event) => any) | null;

    constructor(url: string) {
        this.ws = new WebSocket(url)
        console.log("Connecting to", url)
        this.ws.onopen = e => {
            console.info("Opened")
            if (this.onOpen) {
                this.onOpen.bind(this)(e)
            }
        }
        this.ws.onclose = e => {
            console.info("Closed")
            if (this.onClose) {
                this.onClose.bind(this)(e)
            }
        }

        this.ws.onerror = e => {
            console.error(e)
            if (this.onError) {
                this.onError.bind(this)(e)
            }
        }
        this.ws.onmessage = async e => {
            const msg = e.data as Blob
            const headerByte = (await msg.slice(0, 1).bytes())[0]
            const data = await msg.slice(1).arrayBuffer()
            const decoded = decode(data) as ServerMessage | MessageWithTimestamp
            const isServer = headerByte == SERVER_PREFIX
            if (isServer) {
                this.onServerMessage?.(decoded as ServerMessage)
            } else {
                this.onPeerMessage?.(decoded as MessageWithTimestamp)
            }
        }
    }

    public static init(roomId: string|null, displayName: string, ws:MultiplayerWebsocket): MultiplayerWebsocket {
        console.groupCollapsed("Multiplayer initialization")
        const initialMessage:ClientToServerMessage = {
            type:"initializeconnection",
            room_id: roomId,
            name: displayName
        }
        if (ws.ws.readyState == WebSocket.OPEN) {
            ws.send(initialMessage)
        } else {
            ws.onOpen = () => { ws.send(initialMessage) }
        }
        return ws
    }

    public send(msg: MessageWithTimestamp | ClientToServerMessage): void {
        console.log("Sending", msg)
        return this.ws.send(encode(msg))
    }

    public close(code?: number, reason?: string) {
        return this.ws.close(code, reason)
    }
}
export default MultiplayerWebsocket
