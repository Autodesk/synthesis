import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "../World"
import { peerMessageHandlers } from "./MessageHandlers"
import type { ClientAndLatencyInfo, ClientInfo, Message, MessageWithTimestamp } from "./MultiplayerTypes.ts"
import EventSystem from "@/systems/EventSystem.ts"
import type { ServerToClientMessage } from "@/systems/multiplayer/bindings/ServerToClientMessage.ts"
import { consolePrefixer } from "console-prefixer"
import type MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"
import { hashBuffer } from "@/util/Utility.ts"
import { mirabuf } from "@/proto/mirabuf"
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"
import MatchMode from "../match_mode/MatchMode.ts"

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
    public readonly clientToObjectMap: Map<string, SceneObjectId[]> = new Map() // indicates ownership over objects

    private _info: ClientInfo = {} as ClientInfo
    private _onDestroyHooks: (() => void)[] = []

    private _lastRTT: number = -1
    // Positive if client clock ahead, negative if server clock ahead
    private _clientTimeDeltaMS: number = 0
    private _lastPingTs: number = 0
    private _hasPendingPing: boolean = false

    public fieldTransferLock?: { ts: number; id: SceneObjectId }

    public isHost: boolean

    public static async setup(ws: MultiplayerWebsocket, displayName: string, isHost: boolean): Promise<boolean> {
        MatchMode.getInstance().sandboxModeStart()

        console.group("Multiplayer initialization")

        const system = new MultiplayerSystem(ws, displayName, isHost)
        const initResult = await system._initializationPromise
        World.setMultiplayerSystem(system)

        console.groupEnd()

        return initResult
    }

    private constructor(ws: MultiplayerWebsocket, displayName: string, isHost: boolean) {
        this.isHost = isHost
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
                    await this.handleServerMessage(msg)
                }

                this.client.onPeerMessage = async msg => {
                    await this.handlePeerMessage(msg)
                }
                this.client.onClose = () => {
                    globalAddToast("error", "Multiplayer disconnected")
                    this.destroy()
                    EventSystem.dispatch("MultiplayerStateJoinRoom")
                }
                this.registerExistingSceneObjects()
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

    async handleServerMessage(message: ServerToClientMessage) {
        console.debug(`Incoming server message ${message.type}`)
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
                this._lastRTT = Date.now() - message.client_send_ts
                this._clientTimeDeltaMS = Date.now() - this._lastRTT / 2 - message.server_ts
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
        if (message.type == "newObject" && message.data.miraType == MiraType.FIELD) {
            this.fieldTransferLock = { ts: message.timestamp, id: message.data.sceneObjectId }
        }
        this.client.sendPeer(message as MessageWithTimestamp)
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
        console.warn("INTRODUCING SELF", this.getOwnObjects())
        for (const obj of this.getOwnObjects()) {
            this.send(
                {
                    type: "newObject",
                    data: {
                        sceneObjectId: obj.id,
                        assemblyHash: await hashBuffer(
                            mirabuf.Assembly.encode(obj.mirabufInstance.parser.assembly).finish().buffer as ArrayBuffer
                        ),
                        miraType: obj.miraType,
                        initialPreferences: obj.getPreferenceData(),
                        initialPhysicsData: obj.miraType === MiraType.FIELD ? obj.getUpdateData().bodies : undefined,
                    },
                },
                peerID
            )
        }
    }

    sendDimensionPenalty() {}

    sendOngoingMatchModeInfo() {
        const matchMode = MatchMode.getInstance()
        if (!matchMode.isMatchEnabled) return

        this.broadcast({
            type: "matchModeState",
            data: {
                event: "ongoing",
                config: matchMode.matchModeConfig,
                startTime: this.toServerTime(matchMode.startTime),
            },
        })
    }

    getOwnSceneObjectIDs(): SceneObjectId[] {
        return this.clientToObjectMap.get(this.clientId) ?? []
    }

    getOwnRobots(): MirabufSceneObject[] {
        return this.getOwnObjects().filter(obj => obj.miraType == MiraType.ROBOT)
    }

    getRemoteRobots(): MirabufSceneObject[] {
        return this.getRemoteObjects().filter(obj => obj.miraType == MiraType.ROBOT)
    }

    getOwnObjects(): MirabufSceneObject[] {
        return (this.clientToObjectMap.get(this.clientId) ?? [])
            .map(id => World.sceneRenderer.sceneObjects.get(id))
            .filter(obj => obj instanceof MirabufSceneObject)
    }

    getRemoteObjects(): MirabufSceneObject[] {
        return [...this.clientToObjectMap]
            .filter(([clientId, _]) => clientId != this.clientId)
            .map(([_, objects]) => objects)
            .flat()
            .map(id => World.sceneRenderer.sceneObjects.get(id))
            .filter(obj => obj instanceof MirabufSceneObject)
    }

    registerOwnSceneObject(objectId: SceneObjectId) {
        const list = this.clientToObjectMap.get(this.clientId)
        if (list?.includes(objectId)) {
            console.warn("Already has", objectId)
            return
        }
        if (list != null) {
            list.push(objectId)
        } else {
            this.clientToObjectMap.set(this.clientId, [objectId])
        }
        console.warn(this.getOwnObjects())
    }

    registerExistingSceneObjects() {
        console.log("testing", JSON.stringify(World.sceneRenderer.mirabufSceneObjects))
        const objects = World.sceneRenderer.mirabufSceneObjects.getAll()
        console.warn("EXISTING", objects)
        objects.forEach(object => {
            console.warn("Checking", object.id)
            if (object.isOwnObject) {
                this.registerOwnSceneObject(object.id)
            }
        })
    }

    unregisterOwnSceneObject(objectId: SceneObjectId) {
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
        this.clientToInfoMap.delete(clientId)
        this.clientToObjectMap.delete(clientId)

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

    public toServerTime(time: number) {
        return time - this._clientTimeDeltaMS
    }
    public fromServerTime(time: number) {
        return time + this._clientTimeDeltaMS
    }

    public destroy() {
        this.client.close()
        World.setMultiplayerSystem(undefined)
        World.reset("own")
        this._onDestroyHooks.forEach(hook => {
            hook()
        })
    }
}

export default MultiplayerSystem
