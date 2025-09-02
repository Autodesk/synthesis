import type Jolt from "@azaleacolburn/jolt-physics"
import Peer, { type DataConnection } from "peerjs"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent.ts"
import { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import World from "../World"
import { peerMessageHandlers } from "./MessageHandlers"
import type { ClientInfo, LocalSceneObjectId, Message, MessageWithTimestamp, RemoteSceneObjectId } from "./types"
import { hashBuffer } from "@/util/Utility"

export const COLLISION_TIMEOUT = 500

class MultiplayerSystem {
    private readonly _client: Peer
    private readonly _connections: Map<string, DataConnection> = new Map()
    readonly roomId: string
    readonly clientId: string
    private readonly _initializationPromise: Promise<boolean>

    public readonly _clientToInfoMap: Map<string, ClientInfo> = new Map()

    public readonly _clientToObjectMap: Map<string, LocalSceneObjectId[]> = new Map()
    public readonly _clientToBodyMap: Map<string, Map<number, Jolt.BodyID>> = new Map() // Each Map is: peerBodyId -> clientBodyId
    public readonly _clientToSceneObjectIdMap: Map<string, Map<RemoteSceneObjectId, LocalSceneObjectId>> = new Map() // Each Map is: peerObjectId -> clientObjectId

    readonly info: ClientInfo

    public static async setup(roomId: string, displayName: string, isHost: boolean): Promise<boolean> {
        const clientId = await generateId(roomId)
        const system = new MultiplayerSystem(roomId, clientId, displayName, isHost)
        World.setMultiplayerSystem(system)
        return await system._initializationPromise
    }

    getClient() {
        return this._client
    }

    private constructor(roomId: string, clientId: string, displayName: string, isHost: boolean = false) {
        this.roomId = roomId
        this.clientId = clientId
        this.info = { clientId: this.clientId, displayName: displayName, isHost, creationTime: Date.now() }

        this._client = new Peer(this.clientId, {
            host: window.location.hostname,
            port: parseInt(import.meta.env.VITE_MULTIPLAYER_PORT) ?? 9002,
            path: "/",
        })

        this._client.on("call", e => console.debug("peerjs call", e))
        this._client.on("close", () => {
            console.debug("peerjs close")
        })

        this._initializationPromise = new Promise<boolean>(resolve => {
            this._client.on("open", async (id: string) => {
                console.debug(`Broker connection opened: ID - ${id}`)
                const peerCount = await this.connectToRoom()
                if (peerCount == 0 && !isHost) {
                    globalAddToast("warning", `Could not find room`, this.roomId)
                    this.destroy()
                    World.setMultiplayerSystem(undefined)
                    MultiplayerStateEvent.dispatch(MultiplayerStateEventType.JOIN_ROOM)
                    resolve(false)
                }
                resolve(true)
            })

            this._client.on("error", e => {
                console.error("PeerJS Error:", e)
                switch (e.type) {
                    case "unavailable-id":
                        globalAddToast("warning", "Reused Client ID", "Try Joining Again")
                        PreferencesSystem.setGlobalPreference("MultiplayerClientID", "")
                        break
                    case "network":
                        globalAddToast("error", "Network Issue", "Could not connect to server")
                        break
                    default:
                        console.warn("Unknown PeerJS Error Type", e.type)
                        globalAddToast("warning", "Unknown PeerJS Error")
                        break
                }
                resolve(false)
            })

            this._client.on("disconnected", peer => {
                console.log("PeerJS Disconnect:", peer, this._clientToInfoMap.get(peer)?.displayName ?? "")
            })
        })

        this._client.on("connection", async conn => {
            console.debug("Receiving Connection: ", conn.peer)
            if (
                conn.metadata.authHash !=
                (await createSha256Hash({
                    roomId: this.roomId,
                    establishedClientId: this.clientId,
                    newClientId: conn.peer,
                }))
            ) {
                conn.close()
                console.warn("Blocking unauthorized connection from " + conn.peer)
                return
            }
            this.setupConnectionHandlers(conn)
        })

        ConfigurationSavedEvent.listen(() => {
            World.getOwnObjects().forEach(obj => {
                setTimeout(() => obj.sendPreferences().catch(console.error), 100)
            })
        })
    }

    async connectToRoom() {
        const roomHash = await createSha256Hash({ roomId: this.roomId })

        const peersPromise = new Promise<string[]>(resolve => this._client.listAllPeers(resolve))
        const peers = await peersPromise

        console.debug(`Peers: ${peers}`)

        const peerCount = await Promise.all(
            peers
                .filter(peer => peer !== this.clientId)
                .map(async peer => {
                    const idParts = peer.split("-")
                    if (idParts[1] != roomHash) return false
                    if (
                        idParts[2] == (await createSha256Hash({ roomId: this.roomId, establishedClientId: idParts[1] }))
                    )
                        return false

                    const conn = this._client.connect(peer, {
                        metadata: {
                            authHash: await createSha256Hash({
                                roomId: this.roomId,
                                establishedClientId: peer,
                                newClientId: this.clientId,
                            }),
                        },
                    })
                    this.setupConnectionHandlers(conn)

                    console.debug(`Initiating Connection: ${peer}`)
                    return true
                })
        ).then(res => res.filter(success => success).length)

        MultiplayerStateEvent.dispatch(MultiplayerStateEventType.JOIN_ROOM)
        return peerCount
    }

    setupConnectionHandlers(conn: DataConnection) {
        if (this._connections.has(conn.peer)) {
            console.warn("Setting up connection for", conn.peer, "again")
            return
        }
        conn.on("open", async () => {
            console.debug("Connection opened", conn.peer)
            this._connections.set(conn.peer, conn)
            MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
            await this.send(conn.peer, { type: "info", data: this.info })

            for (const obj of this.getOwnObjects()) {
                await this.send(conn.peer, {
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
                })
            }
        })

        conn.on("data", async (data: unknown) => {
            await this.handlePeerMessage(data as MessageWithTimestamp, conn.peer)
        })

        conn.on("close", () => {
            this._clientToObjectMap.get(conn.peer)?.forEach(obj => {
                this.handlePeerMessage(
                    {
                        type: "deleteObject",
                        data: this.convertSceneObjectIdReverse(conn.peer, obj)!,
                        timestamp: Date.now(),
                    },
                    conn.peer
                ).catch(console.error) // TODO Get actual sceneObjectKey
            })
            this._clientToSceneObjectIdMap.delete(conn.peer)

            this._connections.delete(conn.peer)
            // TODO: handle host transition

            MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
            globalAddToast(
                "warning",
                "Multiplayer Peer Disconnected",
                this._clientToInfoMap.get(conn.peer)?.displayName ?? "Unknown"
            )
            console.debug("Connection closed:", conn.peer)
        })
        conn.on("iceStateChanged", e => console.debug("ice change", e))

        conn.on("error", (err: Error) => {
            console.error("Connection error:", err)
        })
    }

    async handlePeerMessage(message: MessageWithTimestamp, peerId: string) {
        if (message.type != "update") {
            console.debug(`Recieving Message ${message.type}`)
        }
        const handler = peerMessageHandlers[message.type].bind(this) as (
            data: unknown,
            peerid: string,
            time: number
        ) => Promise<void> | void
        await handler(message.data, peerId, message.timestamp)
    }

    async send(peer: string, message: Message) {
        const conn = this._connections.get(peer)
        if (!conn) {
            console.warn("Couldn't find peer: ", peer)
            return
        }
        message.timestamp ??= Date.now()
        await conn.send(message)
    }

    async broadcast(message: Message) {
        if (message.type != "update") {
            console.debug(`Sending Message: ${message.type}`)
        }
        message.timestamp ??= Date.now()
        return await Promise.all(this._peers.map(peer => peer.send(message)))
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

    get peerIDs(): string[] {
        return [...this._connections.keys()]
    }

    private get _peers() {
        return [...this._connections.values()]
    }

    get peerInfo(): ClientInfo[] {
        return this.peerIDs.map(
            peerId =>
                this._clientToInfoMap.get(peerId) ?? {
                    clientId: peerId,
                    displayName: peerId,
                    isHost: false,
                    creationTime: Infinity,
                }
        )
    }

    get displayName(): string {
        return this.info.displayName
    }

    public destroy() {
        this._connections.forEach(conn => conn.close())
        this._connections.clear()
        this._client.destroy()
        this._clientToSceneObjectIdMap.clear()
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

async function generateId(roomId: string, forceRegen: boolean = false): Promise<string> {
    let id =
        import.meta.env.DEV && new URLSearchParams(window.location.search).get("randomId")
            ? undefined
            : PreferencesSystem.getGlobalPreference("MultiplayerClientID")
    if (!id || forceRegen) {
        id = `client_${Math.random().toString(36).substring(2, 9)}`
        PreferencesSystem.setGlobalPreference("MultiplayerClientID", id)
        PreferencesSystem.savePreferences()
    }
    PreferencesSystem.savePreferences()
    return `${id}-${await createSha256Hash({ roomId })}-${await createSha256Hash({ roomId, establishedClientId: id })}`
}

interface HashableData {
    roomId?: string
    establishedClientId?: string
    newClientId?: string
}

async function createSha256Hash({ roomId, establishedClientId, newClientId }: HashableData) {
    const msgBuffer = new TextEncoder().encode(`${roomId}${establishedClientId}${newClientId}`)
    const hashBuffer = await crypto.subtle.digest("SHA-256", msgBuffer)
    const hashArray = Array.from(new Uint8Array(hashBuffer))
    return hashArray
        .slice(0, 8)
        .map(b => b.toString(16).padStart(2, "0"))
        .join("")
}

export enum MultiplayerStateEventType {
    INIT,
    JOIN_ROOM,
    PEER_CHANGE,
}

export class MultiplayerStateEvent extends Event {
    private constructor(event: MultiplayerStateEventType) {
        super(`MultiplayerStateChange${event}`)
    }

    public static dispatch(eventType: MultiplayerStateEventType) {
        const event = new MultiplayerStateEvent(eventType)
        window.dispatchEvent(event)
    }

    public static addEventListener(eventType: MultiplayerStateEventType, cb: EventListenerOrEventListenerObject) {
        window.addEventListener(`MultiplayerStateChange${eventType}`, cb)
        return () => {
            window.removeEventListener(`MultiplayerStateChange${eventType}`, cb)
        }
    }
}

export default MultiplayerSystem
