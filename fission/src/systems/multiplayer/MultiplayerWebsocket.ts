import { consolePrefixer } from "console-prefixer"
import { MessageWithTimestamp } from "@/systems/multiplayer/MultiplayerTypes.ts"
import {Encoder, Decoder} from "@msgpack/msgpack"
import { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import { ServerMessage } from "@/systems/multiplayer/bindings/ServerMessage.ts"

const CLIENT_PREFIX = 0b00000001
const SERVER_PREFIX = 0b00000011


const console = consolePrefixer({
    defaultPrefix: {
        text: "[Multiplayer WS]",
        style: "background: linear-gradient(90deg,rgba(255, 165, 0, 1) 0%, rgba(199, 87, 87, 1) 100%); color: white;font-weight:bold; padding:2px; border-radius:2px;",
    },
})

class MultiplayerWebsocket {
    private readonly ws: WebSocket

    private readonly encoder: Encoder<never> = new Encoder()
    private readonly decoder: Decoder<never> = new Decoder()
    private prefixBuf = new Uint8Array(1)

    public onServerMessage?: (msg: ServerMessage) => void
    public onPeerMessage?: (msg: MessageWithTimestamp) => void
    public onOpen?: ((this: MultiplayerWebsocket, ev: Event) => any) | null;
    public onClose?: ((this: MultiplayerWebsocket, ev: CloseEvent) => any) | null;
    public onError?: ((this: MultiplayerWebsocket, ev: Event) => any) | null;


    public get ready() {
        return this.ws.readyState === WebSocket.OPEN
    }

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
            const data = msg.slice(1).stream()
            const decoded = await this.decoder.decodeAsync(data) as ServerMessage | MessageWithTimestamp
            const isServer = headerByte == SERVER_PREFIX
            if (isServer) {
                this.onServerMessage?.(decoded as ServerMessage)
            } else {
                this.onPeerMessage?.(decoded as MessageWithTimestamp)
            }
        }
    }

    public static init(roomId: string|null, displayName: string, ws:MultiplayerWebsocket): MultiplayerWebsocket {
        const initialMessage:ClientToServerMessage = {
            type:"initializeconnection",
            room_id: roomId,
            name: displayName
        }
        if (ws.ws.readyState == WebSocket.OPEN) {
            ws.sendServer(initialMessage)
        } else {
            ws.onOpen = () => { ws.sendServer(initialMessage) }
        }
        return ws
    }

    private send(prefix:number, msg: MessageWithTimestamp | ClientToServerMessage): void {
        console.log("Sending", msg)
        const encoded = this.encoder.encodeSharedRef(msg)
        this.prefixBuf[0] = prefix
        return this.ws.send(new Blob([this.prefixBuf, encoded]))
    }

    public sendPeer(msg: MessageWithTimestamp): void {
        return this.send(CLIENT_PREFIX, msg)
    }

    public sendServer(msg: ClientToServerMessage): void {
        return this.send(SERVER_PREFIX, msg)
    }

    public close(code?: number, reason?: string) {
        return this.ws.close(code, reason)
    }
}
export default MultiplayerWebsocket
