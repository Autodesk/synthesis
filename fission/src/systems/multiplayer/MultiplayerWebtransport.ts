import { consolePrefixer } from "console-prefixer"
import type { MessageWithTimestamp } from "@/systems/multiplayer/MultiplayerTypes.ts"
import { Encoder, Decoder } from "@msgpack/msgpack"
import type { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import type { ServerToClientMessage } from "@/systems/multiplayer/bindings/ServerToClientMessage.ts"
import { MultiplayerTransport, SERVER_PREFIX } from "@/systems/multiplayer/MultiplayerTransport.ts"
import type { CertificateHashes } from "@/systems/multiplayer/bindings/CertificateHashes.ts"

const console = consolePrefixer({
    defaultPrefix: {
        text: "[Multiplayer WT]",
        style: "background: linear-gradient(90deg,rgba(255, 165, 0, 1) 0%, rgba(199, 87, 87, 1) 100%); color: white;font-weight:bold; padding:2px; border-radius:2px;",
    },
})

const OUTGOING_DATAGRAM_MAX_AGE_MS = 100

/// A deliberately shallow send queue, so backpressure shows up as a dropped
/// update instead of a growing backlog
const OUTGOING_DATAGRAM_HIGH_WATER_MARK = 8

const DEFAULT_SEND_ORDER = 0
const UPDATE_SEND_ORDER_FLOOR = -Number.MAX_SAFE_INTEGER

class MultiplayerWebtransport extends MultiplayerTransport {
    private readonly _wt: WebTransport

    private readonly _encoder: Encoder = new Encoder({ forceFloat32: true })
    private readonly _decoder: Decoder = new Decoder()

    private _datagramWriter!: WritableStreamDefaultWriter<Uint8Array>
    private _ready: boolean = false
    private _closeNotified: boolean = false
    private _warnedOversizedDatagram: boolean = false

    private _updateSendOrder: number = UPDATE_SEND_ORDER_FLOOR

    override get ready() {
        return this._ready
    }

    private notifyClosed() {
        if (this._closeNotified) return
        this._closeNotified = true
        this._ready = false
        this.onClose?.bind(this)?.(null)
    }

    public static async create(url: string): Promise<MultiplayerWebtransport | null> {
        const certs = await this.getCerts(url)
        if (certs == null) return null
        return new MultiplayerWebtransport(url, certs)
    }

    public static async getCerts(url: string): Promise<WebTransportHash[] | null> {
        const certURL = new URL(url)
        certURL.protocol = "http:"
        certURL.pathname = "/cert"
        const certs = await fetch(certURL.href)
        if (!certs.ok) {
            return null
        }
        const data = (await certs.json().catch(() => null)) as CertificateHashes | null
        if (data == null) {
            return null
        }

        return data.hashes.map(v => ({
            algorithm: v.algorithm,
            value: Uint8Array.from(v.value),
        }))
    }
    constructor(url: string, certs: WebTransportHash[]) {
        super()
        this._wt = new WebTransport(url, {
            serverCertificateHashes: certs,
        })

        this._wt.closed
            .then(() => {
                console.info("Closed")
            })
            .catch((e: unknown) => {
                console.info("Closed by server", e)
            })
            .finally(() => {
                this.notifyClosed()
            })
        console.log("Connecting to", url)
        this._wt.ready.then(() => {
            this._wt.datagrams.outgoingMaxAge = OUTGOING_DATAGRAM_MAX_AGE_MS
            this._wt.datagrams.outgoingHighWaterMark = OUTGOING_DATAGRAM_HIGH_WATER_MARK

            const writableStream: WritableStream<Uint8Array> =
                "createWritable" in this._wt.datagrams && typeof this._wt.datagrams.createWritable === "function"
                    ? this._wt.datagrams.createWritable()
                    : this._wt.datagrams.writable // Deprecated and non-standard.

            this._datagramWriter = writableStream.getWriter()
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
        await this.dispatchMessage(headerByte, decoded)
    }

    private async dispatchMessage(headerByte: number, decoded: ServerToClientMessage | MessageWithTimestamp) {
        const isServer = headerByte == SERVER_PREFIX
        if (isServer) {
            this.onServerMessage?.(decoded as ServerToClientMessage)
        } else {
            this.onPeerMessage?.(decoded as MessageWithTimestamp)
        }
    }

    private async listenStreams() {
        try {
            for await (const stream of this._wt.incomingUnidirectionalStreams) {
                setTimeout(() => this.handleStream(stream))
            }
        } catch {
            console.warn("Connection closed")
            this.notifyClosed()
        }
    }

    private async handleStream(stream: ReadableStream<Uint8Array>) {
        const reader = stream.getReader()
        const { value: firstChunk } = await reader.read()
        reader.releaseLock()

        if (!firstChunk) return

        const headerByte = firstChunk[0]
        const remainder = firstChunk.subarray(1)

        async function* getPayload() {
            if (remainder.byteLength > 0) yield remainder
            for await (const chunk of stream) yield chunk
        }

        const data = await this._decoder.decodeAsync(getPayload())
        await this.dispatchMessage(headerByte, data as ServerToClientMessage | MessageWithTimestamp)
    }

    private async listenDatagrams() {
        for await (const datagram of this._wt.datagrams.readable) {
            await this.decodeMessage(datagram)
        }
    }

    override send(prefix: number, msg: MessageWithTimestamp | ClientToServerMessage, datagram: boolean): void {
        const encoded = this._encoder.encodeSharedRef(msg)
        const bytes = new Uint8Array(encoded.length + 1)
        bytes[0] = prefix
        bytes.set(encoded, 1)

        // A datagram over the path limit is silently discarded by the browser, we should send as stream
        const maxDatagramSize = this._wt.datagrams.maxDatagramSize

        let sendOrder = DEFAULT_SEND_ORDER

        if (datagram && bytes.length > maxDatagramSize) {
            this._updateSendOrder += 1
            sendOrder = this._updateSendOrder

            if (!this._warnedOversizedDatagram) {
                this._warnedOversizedDatagram = true
                console.warn(
                    `A "${msg.type}" message is ${bytes.length} bytes, past the ${maxDatagramSize} byte datagram limit, so it is going over a stream instead. Shrink the payload to keep these unreliable.`
                )
            }
        } else if (datagram) {
            const desiredSize = this._datagramWriter.desiredSize
            if (desiredSize !== null && desiredSize <= 0) return

            void this._datagramWriter.write(bytes).catch(() => {})
            return
        }

        void this.sendStream(bytes, sendOrder).catch(() => {
            console.warn("Failed to send", msg.type)
        })
    }

    private async sendStream(bytes: Uint8Array, sendOrder: number) {
        const stream = await this._wt.createUnidirectionalStream({ sendOrder })
        const writer = stream.getWriter()

        await writer.write(bytes)
        await writer.close()
    }

    override async close(code?: number, reason?: string) {
        this._wt.close?.({ closeCode: code, reason })
        await this._wt.datagrams.readable.cancel("Transport closed by client")
    }
}

export default MultiplayerWebtransport
