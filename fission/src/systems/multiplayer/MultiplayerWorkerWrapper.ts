import MPWorker from "./MultiplayerWorker.ts?worker"
import type {
    FromWorkerMessage,
    MessageWithTimestamp,
    ToWorkerMessage,
} from "@/systems/multiplayer/MultiplayerTypes.ts"
import type { ServerToClientMessage } from "@/systems/multiplayer/bindings/ServerToClientMessage.ts"
import type { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import type { MultiplayerCommunicationProvider } from "@/systems/multiplayer/MultiplayerCommunicationInterface.ts"

export class MultiplayerWorker implements MultiplayerCommunicationProvider {
    private _worker: Worker

    public onServerMessage?: (msg: ServerToClientMessage) => void
    public onPeerMessage?: (msg: MessageWithTimestamp) => void
    public onOpen?: (() => unknown) | null
    public onClose?: (() => unknown) | null
    public onError?: ((v?: Event) => unknown) | null
    public ready: boolean = false
    constructor(url: string) {
        this._worker = new MPWorker({ name: "MultiplayerWorker" })
        this.postMessage({ event: "connect", data: { url } })
        this._worker.onerror = e => {
            this.onError?.(e)
        }
        this._worker.onmessage = e => {
            const msg = e.data as FromWorkerMessage
            switch (msg.event) {
                case "close": {
                    this.onClose?.()
                    break
                }
                case "open": {
                    this.onOpen?.()
                    this.ready = true
                    break
                }
                case "error": {
                    this.onError?.(e)
                    break
                }
                case "peerMessage": {
                    this.onPeerMessage?.(msg.data)
                    break
                }
                case "serverMessage": {
                    this.onServerMessage?.(msg.data)
                    break
                }
                default: {
                    console.warn("Unknown message", msg)
                }
            }
        }
        this._worker.onmessageerror = e => {
            console.warn("MessageError", e)
        }
    }

    private postMessage(message: ToWorkerMessage) {
        this._worker.postMessage(message)
    }

    public init(roomId: string | null, displayName: string) {
        this.postMessage({ event: "initialize", data: { roomId, displayName } })
        return this
    }

    public sendPeer(msg: MessageWithTimestamp): void {
        this.postMessage({ event: "peerMessage", data: msg })
    }

    public sendServer(msg: ClientToServerMessage): void {
        this.postMessage({ event: "serverMessage", data: msg })
    }

    public close() {
        this.postMessage({ event: "disconnect" })
        this._worker.terminate()
    }
}
