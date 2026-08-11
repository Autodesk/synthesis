import { consolePrefixer } from "console-prefixer"
import type { MessageWithTimestamp } from "@/systems/multiplayer/MultiplayerTypes.ts"
import { Encoder, Decoder } from "@msgpack/msgpack"
import type { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import type { ServerToClientMessage } from "@/systems/multiplayer/bindings/ServerToClientMessage.ts"
import { MultiplayerTransport, SERVER_PREFIX } from "@/systems/multiplayer/MultiplayerTransport.ts"

const console = consolePrefixer({
    defaultPrefix: {
        text: "[Multiplayer WT]",
        style: "background: linear-gradient(90deg,rgba(255, 165, 0, 1) 0%, rgba(199, 87, 87, 1) 100%); color: white;font-weight:bold; padding:2px; border-radius:2px;",
    },
})

type StreamReadWrite = {
    writer: WritableStreamDefaultWriter<Uint8Array>
    reader: ReadableStream<Uint8Array>
}
class MultiplayerWebtransport extends MultiplayerTransport {
    private readonly _wt: WebTransport

    private readonly _encoder: Encoder<never> = new Encoder()
    private readonly _decoder: Decoder<never> = new Decoder()
    private _stream!: StreamReadWrite
    private _datagrams!: StreamReadWrite
    private _prefixBuf = new Uint8Array(1)
    private _ready: boolean = false

    override get ready() {
        return this._ready
    }

    constructor(url: string) {
        super()
        this._wt = new WebTransport(url)

        this._wt.closed.then(() => {
            console.info("Closed")
            if (this.onClose) {
                this.onClose.bind(this)(null)
            }
        })
        console.log("Connecting to", url)
        this._wt.ready.then(async () => {
            const stream = await this._wt.createBidirectionalStream()
            this._stream = {
                writer: stream.writable.getWriter(),
                reader: stream.readable,
            }
            this._datagrams = {
                writer: this._wt.datagrams.writable.getWriter(),
                reader: this._wt.datagrams.readable,
            }
            this._ready = true
            this.onOpen?.bind(this)?.(null)
            setTimeout(() => this.listenDatagrams())
            setTimeout(() => this.listenStreams())
        })
    }

    private async decodeMessage(body: Uint8Array) {
        const headerByte = body[0]
        const data = body.slice(1)
        const decoded = this._decoder.decode(data) as ServerToClientMessage | MessageWithTimestamp
        const isServer = headerByte == SERVER_PREFIX
        if (decoded.type != "update") console.debug("Recieving", isServer ? "server" : "client", decoded)
        if (isServer) {
            this.onServerMessage?.(decoded as ServerToClientMessage)
        } else {
            this.onPeerMessage?.(decoded as MessageWithTimestamp)
        }
    }

    private async listenStreams() {
        for await (const message of this._stream.reader) {
            console.log("[TCP]", message)
            await this.decodeMessage(message)
        }
    }

    private async listenDatagrams() {
        for await (const datagram of this._datagrams.reader) {
            console.log("[UDP]", datagram)
            await this.decodeMessage(datagram)
        }
    }

    override async send(prefix: number, msg: MessageWithTimestamp | ClientToServerMessage, datagram: boolean) {
        if (msg.type != "update") console.debug("Sending", msg)
        const encoded = this._encoder.encodeSharedRef(msg)
        this._prefixBuf[0] = prefix
        const data = new Blob([this._prefixBuf, encoded])
        if (datagram) {
            await this._datagrams.writer.write(await data.bytes())
        } else {
            await this._stream.writer.write(await data.bytes())
        }
    }

    override async close(code?: number, reason?: string) {
        this._wt.close?.({ closeCode: code, reason })
        await this._stream.reader.cancel("Transport closed by client")
        await this._datagrams.reader.cancel("Transport closed by client")
        await this._stream.writer.close()
        await this._datagrams.writer.close()
    }
}
export default MultiplayerWebtransport
