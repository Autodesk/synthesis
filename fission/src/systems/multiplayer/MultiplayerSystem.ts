import type Jolt from "@synthesis.adsk/jolt-physics"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import World from "../World"
import { peerMessageHandlers } from "./MessageHandlers"
import type {
    ClientInfo,
    LocalSceneObjectId,
    Message,
    MessageWithTimestamp,
    RemoteSceneObjectId,
} from "./MultiplayerTypes.ts"
import EventSystem from "@/systems/EventSystem.ts"
import { decode, encode } from "@msgpack/msgpack"
import type { ClientToServerMessage } from "@/systems/multiplayer/bindings/ClientToServerMessage.ts"
import type { ServerMessage } from "@/systems/multiplayer/bindings/ServerMessage.ts"
import { consolePrefixer } from "console-prefixer"

export const COLLISION_TIMEOUT = 500
export const CLIENT_PREFIX = 0b00000001
export const SERVER_PREFIX = 0b00000011

export const multiplayerLogger = consolePrefixer({
    defaultPrefix: {
        text: "[Multiplayer]",
        style: "background: orange; color: white;font-weight:bold; padding:2px; border-radius:2px;",
    },
})
const console = multiplayerLogger

class MultiplayerSystem {
    public readonly client: WebSocket
    public roomId: number = -1
    public clientId: string = ""
    private _initializationPromise: Promise<boolean>

    public readonly _clientToInfoMap: Map<string, ClientInfo> = new Map()
    public readonly _clientToObjectMap: Map<string, LocalSceneObjectId[]> = new Map()
    public readonly _clientToBodyMap: Map<string, Map<number, Jolt.BodyID>> = new Map() // Each Map is: peerBodyId -> clientBodyId
    public readonly _clientToSceneObjectIdMap: Map<string, Map<RemoteSceneObjectId, LocalSceneObjectId>> = new Map() // Each Map is: peerObjectId -> clientObjectId

    private _info: ClientInfo = {} as ClientInfo
    private _onDestroyHooks: (() => void)[] = []

    public static async setup(hostAddr: string, roomId: number | "create", displayName: string): Promise<boolean> {
        const system = new MultiplayerSystem(hostAddr, roomId, displayName)
        const initResult = await system._initializationPromise
        World.setMultiplayerSystem(system)
        return initResult
    }

    private constructor(hostAddr: string, roomId: number | "create", displayName: string) {
        this.client = new WebSocket(hostAddr)
        this.client.onopen = () => {
            const msg = encode({
                type: "initializeconnection",
                room_id: roomId == "create" ? null : roomId,
                name: displayName,
            } satisfies ClientToServerMessage)
            this.client.send(msg)
        }
        this.client.onclose = ev => {
            console.log(ev, this.client)
            globalAddToast("error", "Multiplayer connection closed")
            this.destroy()
            EventSystem.dispatch("MultiplayerStateJoinRoom")
        }

        this.client.onerror = ev => {
            console.error(ev)
            globalAddToast("warning", "Multiplayer error")
        }

        this._initializationPromise = new Promise<boolean>(resolve => {
            this._info.displayName = displayName
            this.client.onmessage = async ev => {
                const { from, type } = await this.onMessage(ev.data)
                if (from != "server" || type != "sendinfo") {
                    this.destroy()
                    console.error("Received invalid initial message")
                    resolve(false)
                    return
                }

                resolve(true)
            }
            setTimeout(() => resolve(false), 10000)
        }).then(res => {
            if (res) {
                this.client.onmessage = async ev => {
                    await this.onMessage(ev.data as Blob)
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
    }

    async onMessage(msg: Blob) {
        const headerByte = (await msg.slice(0, 1).bytes())[0]
        const data = await msg.slice(1).arrayBuffer()
        const decoded = decode(data)
        const isServer = headerByte == SERVER_PREFIX

        if (isServer) {
            return {
                from: "server",
                type: await this.handleServerMessage(decoded as ServerMessage),
            }
        } else {
            return {
                from: "client",
                type: await this.handlePeerMessage(decoded as MessageWithTimestamp),
            }
        }
    }

    async handleServerMessage(message: ServerMessage) {
        switch (message.type) {
            case "sendinfo":
                this.roomId = message.room_id
                this.clientId = message.client_id
                this._info.clientId = this.clientId
                this._info.creationTime = Date.now()
                globalAddToast("success", "Joined room", this.roomId)
                await this.sendHello(true)
                multiplayerLogger.log("Dispatchy")
                setTimeout(() => EventSystem.dispatch("MultiplayerStateJoinRoom"))
                break
            case "kick":
                this.removePeer(message.client_id)
                break
            default:
                console.warn(`Unhandled message from server (type ${message.type})`, message)
        }
        return message.type
    }

    async handlePeerMessage(message: MessageWithTimestamp) {
        if (message.recipientId != null && message.recipientId != this.clientId) {
            console.info("Ignoring message for", message.recipientId)
        }
        if (message.type != "update") {
            console.debug(`Receiving Message ${message.type}`, message)
        }

        const handler = peerMessageHandlers[message.type].bind(this) as (
            data: unknown,
            peerid: string,
            time: number
        ) => Promise<void> | void
        await handler(message.data, message.client_id, message.timestamp)
        return message.type
    }

    async broadcast(message: Message) {
        return this.send(message)
    }

    async send(message: Message, peerID?: string) {
        message.recipientId = peerID
        message.timestamp ??= Date.now()
        message.client_id = this.clientId
        if (message.type != "update") {
            console.debug(`Sending Message: ${message.type}`, message)
        }
        const encoded = encode(message)
        return this.client.send(encoded)
    }

    async sendHello(requestIntroductions: boolean, peerID?: string) {
        await this.send(
            {
                type: "info",
                data: {
                    info: this._info,
                    introduceSelf: requestIntroductions,
                },
            },
            peerID
        )
    }

    getOwnSceneObjectIDs() {
        return this._clientToObjectMap.get(this.clientId) ?? []
    }

    getOwnRobots(): MirabufSceneObject[] {
        return this.getOwnObjects().filter(obj => obj.miraType == MiraType.ROBOT)
    }

    getOwnObjects(): MirabufSceneObject[] {
        return (this._clientToObjectMap.get(this.clientId) ?? [])
            .map(id => World.sceneRenderer.sceneObjects.get(id))
            .filter(obj => obj instanceof MirabufSceneObject)
    }

    registerOwnSceneObject(objectId: LocalSceneObjectId) {
        const list = this._clientToObjectMap.get(this.clientId)
        this.setSceneObjectIdMapping(this.clientId, objectId as RemoteSceneObjectId, objectId)
        if (list != null) {
            list.push(objectId)
        } else {
            this._clientToObjectMap.set(this.clientId, [objectId])
        }
    }

    unregisterOwnSceneObject(objectId: LocalSceneObjectId) {
        const list = this._clientToObjectMap.get(this.clientId)
        if (!list) return
        const index = list.indexOf(objectId)
        if (index == -1) return
        list.splice(index, 1)
    }

    removePeer(clientId: string) {
        const name = this._clientToInfoMap.get(clientId)?.displayName
        this._clientToObjectMap.get(clientId)?.forEach(obj => {
            World.sceneRenderer.removeSceneObject(obj)
        })

        this._clientToSceneObjectIdMap.delete(clientId)
        this._clientToInfoMap.delete(clientId)
        this._clientToObjectMap.delete(clientId)
        this._clientToBodyMap.delete(clientId)

        EventSystem.dispatch("MultiplayerStatePeerChange")
        globalAddToast(
            "warning",
            "Multiplayer Peer Disconnected",
            name != null ? `${name} (${clientId.slice(0, 8)})` : clientId
        )
    }

    get peerIDs(): string[] {
        return [...this._clientToInfoMap.keys()]
    }

    get peerInfo(): readonly Readonly<ClientInfo>[] {
        return [...this._clientToInfoMap.values()]
    }

    get info(): Readonly<ClientInfo> {
        return this._info
    }

    get displayName(): string {
        return this._info.displayName
    }

    public destroy() {
        this.client.close()
        this._clientToSceneObjectIdMap.clear()
        this._onDestroyHooks.forEach(hook => {
            hook()
        })
        World.setMultiplayerSystem(undefined)
    }

    public convertSceneObjectId(peerId: string, objectId: RemoteSceneObjectId): LocalSceneObjectId {
        return this._clientToSceneObjectIdMap.get(peerId)?.get(objectId) ?? (-1 as LocalSceneObjectId)
    }

    public convertSceneObjectIdReverse(peerId: string, objectId: LocalSceneObjectId): RemoteSceneObjectId | undefined {
        return [...this._clientToSceneObjectIdMap.get(peerId)!.entries()].find(([_, l]) => objectId == l)?.[0]
    }

    public setSceneObjectIdMapping(peerId: string, remoteId: RemoteSceneObjectId, localId: LocalSceneObjectId) {
        let peerMap = World.multiplayerSystem?._clientToSceneObjectIdMap.get(peerId)
        if (peerMap == null) {
            peerMap = new Map()
            World.multiplayerSystem?._clientToSceneObjectIdMap.set(peerId, peerMap)
        }
        peerMap.set(remoteId, localId)
    }
}

export default MultiplayerSystem
