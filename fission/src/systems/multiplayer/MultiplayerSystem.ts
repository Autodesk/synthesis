/** biome-ignore-all lint/correctness/noUndeclaredVariables: In Progress */
import Peer, { type DataConnection } from "peerjs"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent.ts"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import type PhysicsSystem from "../physics/PhysicsSystem"
import World from "../World"
import { peerMessageHandlers } from "./MessageHandlers"
import type { ClientInfo, EncodedAssembly, Message, MessageType } from "./types"
import Jolt from "@azaleacolburn/jolt-physics"

export const COLLISION_TIMEOUT = 500

class MultiplayerSystem {
    private readonly _client: Peer
    private readonly _connections: Map<string, DataConnection> = new Map()
    readonly roomId: string
    readonly clientId: string
    private readonly _initializationPromise: Promise<boolean>

    public readonly _clientToInfoMap: Map<string, ClientInfo> = new Map()
    public readonly _clientToObjectMap: Map<string, number[]> = new Map() // sceneObjectKey -> Jolt.BodyId.GetIndexAndSequenceNumber()
    public readonly _clientToBodyMap: Map<string, Map<number, Jolt.BodyID>> = new Map() // Each Map is: peerBodyId -> clientBodyId

    readonly info: ClientInfo
    public lastSentCollisionTimestamp: number = Date.now()

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
                this.broadcast({ type: "metadataUpdate", data: obj.multiplayerInfo }).catch(console.error)
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

    // Called by the host, initializes the world with some defined set of objects, robots can be spawned in later
    async initWorld(physicsSystem: PhysicsSystem) {
        const sceneObjects = World.sceneRenderer.mirabufSceneObjects
            .getAll()
            .map(sceneObject =>
                mirabuf.Assembly.encode(sceneObject.mirabufInstance.parser.assembly).finish()
            ) as EncodedAssembly[]

        await this.broadcast({
            type: "init",
            data: { physicsSystem, objects: sceneObjects },
        })
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
            for (const objectId of this.getOwnSceneObjectIDs()) {
                const obj = World.sceneRenderer.sceneObjects.get(objectId)
                if (!(obj instanceof MirabufSceneObject)) return
                const hash = await MirabufCachingService.hashBuffer(
                    mirabuf.Assembly.encode(obj.mirabufInstance.parser.assembly).finish()
                )
                await this.send(conn.peer, {
                    type: "newObject",
                    data: {
                        sceneObjectKey: objectId,
                        assemblyHash: hash,
                        miraType: obj.miraType,
                        initialPreferences: obj.getPreferenceData(),
                    },
                })
                await this.send(conn.peer, { type: "metadataUpdate", data: obj.multiplayerInfo })
            }
        })

        conn.on("data", async (data: unknown) => {
            await this.handlePeerMessage(data as Message, conn.peer)
        })

        conn.on("close", () => {
            this._clientToObjectMap.get(conn.peer)?.forEach(obj => {
                this.handlePeerMessage(
                    {
                        type: "deleteObject",
                        data: obj,
                    },
                    conn.peer
                ).catch(console.error) // TODO Get actual sceneObjectKey
            })

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

    async handlePeerMessage(message: Message, peerId: string) {
        const handler = peerMessageHandlers[message.type].bind(this) as (
            data: MessageType[typeof message.type],
            peerId: string
        ) => Promise<void> | void
        await handler(message.data, peerId)
    }

    async send(peer: string, message: Message) {
        const conn = this._connections.get(peer)
        if (!conn) {
            console.warn("Couldn't find peer: ", peer)
            return
        }
        await conn.send(message)
    }

    async broadcast(message: Message) {
        console.debug(`Sending Message: ${message.type}`)
        return await Promise.all(this._peers.map(peer => peer.send(message)))
    }

    getOwnSceneObjectIDs(): number[] {
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

    registerOwnSceneObject(objectId: number) {
        const list = this._clientToObjectMap.get(this.clientId)
        if (list != null) {
            list.push(objectId)
        } else {
            this._clientToObjectMap.set(this.clientId, [objectId])
        }
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
        World.setMultiplayerSystem(undefined)
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
