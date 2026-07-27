import type Jolt from "@synthesis.adsk/jolt-physics"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "../World"
import { peerMessageHandlers } from "./MessageHandlers"
import type {
    ClientAndLatencyInfo,
    ClientInfo,
    LocalSceneObjectId,
    Message,
    MessageWithTimestamp,
    RemoteSceneObjectId,
} from "./MultiplayerTypes.ts"
import EventSystem from "@/systems/EventSystem.ts"
import type { ServerMessage } from "@/systems/multiplayer/bindings/ServerMessage.ts"
import { consolePrefixer } from "console-prefixer"
import type MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"
import { hashBuffer } from "@/util/Utility.ts"
import { mirabuf } from "@/proto/mirabuf"

export const COLLISION_TIMEOUT = 500

export const multiplayerLogger = consolePrefixer({
    defaultPrefix: {
        text: "[Multiplayer]",
        style: "background: orange; color: white;font-weight:bold; padding:2px; border-radius:2px;",
    },
})
const console = multiplayerLogger

class MultiplayerSystem {
    public readonly client: MultiplayerWebsocket
    public roomId: string = ""
    public clientId: string = ""
    private _initializationPromise: Promise<boolean>

    public readonly clientToInfoMap: Map<string, ClientAndLatencyInfo> = new Map()
    public readonly clientToObjectMap: Map<string, LocalSceneObjectId[]> = new Map()
    public readonly clientToBodyMap: Map<string, Map<number, Jolt.BodyID>> = new Map() // Each Map is: peerBodyId -> clientBodyId
    public readonly clientToSceneObjectIdMap: Map<string, Map<RemoteSceneObjectId, LocalSceneObjectId>> = new Map() // Each Map is: peerObjectId -> clientObjectId

    private _info: ClientInfo = {} as ClientInfo
    private _onDestroyHooks: (() => void)[] = []

    private _lastRTT: number = -1
    private _lastPingTs: number = 0
    private _hasPendingPing: boolean = false

    public static async setup(ws: MultiplayerWebsocket, displayName: string): Promise<boolean> {
        console.groupCollapsed("Multiplayer initialization")
        const system = new MultiplayerSystem(ws, displayName)
        const initResult = await system._initializationPromise
        World.setMultiplayerSystem(system)
        console.groupEnd()
        return initResult
    }

    private constructor(ws: MultiplayerWebsocket, displayName: string) {
        this.client = ws

        this.client.onError = () => {
            globalAddToast("warning", "Multiplayer error")
        }

        this._initializationPromise = new Promise<boolean>(resolve => {
            this._info.displayName = displayName
            this.client.onServerMessage = async msg => {
                if (msg.type != "sendinfo") {
                    this.destroy()
                    console.error("Received invalid initial message")
                    resolve(false)
                    return
                }
                await this.handleServerMessage(msg)
                resolve(true)
            }
            this.client.onClose = () => {
                resolve(false)
            }
            setTimeout(() => resolve(false), 10000)
        }).then(res => {
            if (res) {
                this.client.onServerMessage = async msg => {
                    console.groupCollapsed(`Incoming server message: ${msg.type}`)
                    await this.handleServerMessage(msg)
                    console.groupEnd()
                }

                this.client.onPeerMessage = async msg => {
                    if (msg.type != "update") {
                        console.groupCollapsed(`Incoming peer message: ${msg.type}`)
                    }
                    await this.handlePeerMessage(msg)
                    if (msg.type != "update") {
                        console.groupEnd()
                    }
                }
                this.client.onClose = () => {
                    globalAddToast("error", "Multiplayer disconnected")
                    this.destroy()
                    EventSystem.dispatch("MultiplayerStateJoinRoom")
                }
            }
            return res
        })

        this._onDestroyHooks.push(
            EventSystem.listen("ConfigurationSavedEvent", () => {
                World.getOwnObjects().forEach(obj => {
                    setTimeout(() => obj.sendPreferences().catch(console.error), 100)
                })
            })
        )

        this.ping()
        const pingCallback = setInterval(() => this.ping(), 5_000)

        this._onDestroyHooks.push(() => {
            clearInterval(pingCallback)
        })
    }

    private ping() {
        if (!this._hasPendingPing) {
            this._lastPingTs = Date.now()
        }

        this._hasPendingPing = true

        if (!this.client.ready) {
            setTimeout(() => this.ping(), 200)
            return
        }

        this.client.sendServer({ type: "ping", timestamp: Date.now() })
    }

    async handleServerMessage(message: ServerMessage) {
        switch (message.type) {
            case "sendinfo":
                this.roomId = message.room_id
                this.clientId = message.client_id
                this._info.clientId = this.clientId
                this._info.creationTime = Date.now()
                globalAddToast("success", "Joined room", this.roomId)
                await this.introduceSelf(true)
                setTimeout(() => EventSystem.dispatch("MultiplayerStateJoinRoom"))
                break
            case "kick":
                this.removePeer(message.client_id)
                break
            case "pong":
                this._lastRTT = Date.now() - message.timestamp
                this._hasPendingPing = false
                this.send({
                    type: "latencyInfo",
                    data: {
                        latencyMS: this.latencyMS,
                    },
                })
                break
            default:
                console.warn(`Unhandled message from server (type ${message.type})`, message)
        }
        return message.type
    }

    async handlePeerMessage(message: MessageWithTimestamp) {
        if (message.recipientId != null && message.recipientId != this.clientId) {
            console.info("Ignoring message for", message.recipientId)
            return
        }
        if (message.type != "update") {
            console.info(`Receiving Message ${message.type}`, message)
        }

        const handler = peerMessageHandlers[message.type].bind(this) as (
            data: unknown,
            peerid: string,
            time: number
        ) => Promise<void> | void
        await handler(message.data, message.clientId, message.timestamp)
        return message.type
    }

    broadcast(message: Message) {
        return this.send(message)
    }

    send(message: Message, peerID?: string) {
        message.recipientId = peerID
        message.timestamp ??= Date.now()
        message.clientId = this.clientId
        if (message.type != "update") {
            console.groupCollapsed(`Sending Message: ${message.type}`)
            console.debug(message)
            console.groupEnd()
        }
        return this.client.sendPeer(message as MessageWithTimestamp)
    }

    async introduceSelf(requestIntroductions: boolean, peerID?: string) {
        this.send(
            {
                type: "info",
                data: {
                    info: this._info,
                    introduceSelf: requestIntroductions,
                },
            },
            peerID
        )
        for (const obj of this.getOwnObjects()) {
            this.send(
                {
                    type: "newObject",
                    data: {
                        sceneObjectKey: obj.id as RemoteSceneObjectId,
                        assemblyHash: await hashBuffer(
                            mirabuf.Assembly.encode(obj.mirabufInstance.parser.assembly).finish().buffer as ArrayBuffer
                        ),
                        miraType: obj.miraType,
                        initialPreferences: obj.getPreferenceData(),
                        bodyIds: obj.getAllBodyIds().map(id => id.GetIndexAndSequenceNumber()),
                    },
                },
                peerID
            )
        }
    }

    getOwnSceneObjectIDs() {
        return this.clientToObjectMap.get(this.clientId) ?? []
    }

    getOwnRobots(): MirabufSceneObject[] {
        return this.getOwnObjects().filter(obj => obj.miraType == MiraType.ROBOT)
    }

    getOwnObjects(): MirabufSceneObject[] {
        return (this.clientToObjectMap.get(this.clientId) ?? [])
            .map(id => World.sceneRenderer.sceneObjects.get(id))
            .filter(obj => obj instanceof MirabufSceneObject)
    }

    registerOwnSceneObject(objectId: LocalSceneObjectId) {
        const list = this.clientToObjectMap.get(this.clientId)
        this.setSceneObjectIdMapping(this.clientId, objectId as RemoteSceneObjectId, objectId)
        if (list != null) {
            list.push(objectId)
        } else {
            this.clientToObjectMap.set(this.clientId, [objectId])
        }
    }

    unregisterOwnSceneObject(objectId: LocalSceneObjectId) {
        const list = this.clientToObjectMap.get(this.clientId)
        if (!list) return
        const index = list.indexOf(objectId)
        if (index == -1) return
        list.splice(index, 1)
    }

    removePeer(clientId: string) {
        const name = this.clientToInfoMap.get(clientId)?.displayName
        this.clientToObjectMap.get(clientId)?.forEach(obj => {
            World.sceneRenderer.removeSceneObject(obj)
        })

        this.clientToSceneObjectIdMap.delete(clientId)
        this.clientToInfoMap.delete(clientId)
        this.clientToObjectMap.delete(clientId)
        this.clientToBodyMap.delete(clientId)

        EventSystem.dispatch("MultiplayerStatePeerChange")
        globalAddToast(
            "warning",
            "Multiplayer Peer Disconnected",
            name != null ? `${name} (${clientId.slice(0, 8)})` : clientId
        )
    }

    get peerIDs(): string[] {
        return [...this.clientToInfoMap.keys()]
    }

    get peerInfo(): readonly Readonly<ClientAndLatencyInfo>[] {
        return [...this.clientToInfoMap.values()]
    }

    get info(): Readonly<ClientAndLatencyInfo> {
        return { ...this._info, latency: this.latencyMS, lastUpdateTime: this._lastPingTs }
    }

    get displayName(): string {
        return this._info.displayName
    }

    get latencyMS(): number {
        if (
            !this.client.ready ||
            (this._hasPendingPing && this._lastPingTs != 0 && Date.now() - this._lastPingTs > 2000)
        ) {
            return -1
        }
        return this._lastRTT / 2
    }

    public destroy() {
        this.client.close()
        this.clientToSceneObjectIdMap.clear()
        this._onDestroyHooks.forEach(hook => {
            hook()
        })
        World.setMultiplayerSystem(undefined)
    }

    public convertSceneObjectId(peerId: string, objectId: RemoteSceneObjectId): LocalSceneObjectId {
        return this.clientToSceneObjectIdMap.get(peerId)?.get(objectId) ?? (-1 as LocalSceneObjectId)
    }

    public setSceneObjectIdMapping(peerId: string, remoteId: RemoteSceneObjectId, localId: LocalSceneObjectId) {
        let peerMap = World.multiplayerSystem?.clientToSceneObjectIdMap.get(peerId)
        if (peerMap == null) {
            peerMap = new Map()
            World.multiplayerSystem?.clientToSceneObjectIdMap.set(peerId, peerMap)
        }
        peerMap.set(remoteId, localId)
    }
}

export default MultiplayerSystem
