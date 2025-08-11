/** biome-ignore-all lint/correctness/noUndeclaredVariables: In Progress */
import Peer, { type DataConnection } from "peerjs"
import { globalAddToast } from "@/components/GlobalUIControls.ts"
import { ConfigurationSavedEvent } from "@/events/ConfigurationSavedEvent.ts"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import MatchMode from "@/systems/match_mode/MatchMode.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import JOLT from "@/util/loading/JoltSyncLoader"
import type PhysicsSystem from "../physics/PhysicsSystem"
import World from "../World"
import type {
    AssemblyRequestData,
    ClientInfo,
    EncodedAssembly,
    InitData,
    InitObjectData,
    MatchModeStateData,
    Message,
    MessageType,
    MetadataUpdateData,
    ObjectPreferences,
    UpdateObjectData,
} from "./types"

const COLLISION_TIMEOUT = 500

class MultiplayerSystem {
    private readonly _client: Peer
    private readonly _connections: Map<string, DataConnection> = new Map()
    readonly roomId: string
    readonly clientId: string
    private readonly _initializationPromise: Promise<boolean>

    private readonly _clientToInfoMap: Map<string, ClientInfo> = new Map()
    private readonly _clientToObjectMap: Map<string, number[]> = new Map() // sceneObjectKey -> Jolt.BodyId.GetIndexAndSequenceNumber()

    readonly info: ClientInfo
    lastSentCollisionTimestamp: number = Date.now()

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
            World.sceneRenderer.mirabufSceneObjects.getAll().forEach(obj => {
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
        })

        conn.on("data", async (data: unknown) => {
            await this.handlePeerMessage(data as Message, conn.peer)
        })

        conn.on("close", () => {
            this.handlePeerMessage(
                {
                    type: "robotLeft",
                    data: { sceneObjectKey: 0 },
                },
                conn.peer
            ).catch(console.error) // TODO Get actual sceneObjectKey

            this._connections.delete(conn.peer)
            // TODO: handle host transition

            MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
            console.debug("Connection closed:", conn.peer)
        })
        conn.on("iceStateChanged", e => console.debug("ice change", e))

        conn.on("error", (err: Error) => {
            console.error("Connection error:", err)
        })
    }

    peerMessageHandlers = {
        info: this.handlePeerInfo,
        init: this.handleWorldInitialization,
        update: this.handlePeerUpdate,
        collision: this.handleCollision,
        newObject: this.handleNewObject,
        needAssembly: this.handleAssemblyRequest,
        deleteObject: this.handleDeleteObject,
        configureObject: this.handleObjectConfiguration,
        disableObjectPhysics: this.disableObjectPhysics,
        enableObjectPhysics: this.enableObjectPhysics,
        metadataUpdate: this.handleMetadataUpdate,
        matchModeState: this.handleMatchModeState,
        robotLeft: () => {
            console.warn("unhandled event")
        },
        ping: () => {
            console.warn("unhandled event")
        },
        pong: () => {
            console.warn("unhandled event")
        },
    } as const satisfies { [K in keyof MessageType]: (data: MessageType[K], peerId: string) => Promise<void> | void }

    async handlePeerMessage(message: Message, peerId: string) {
        const handler = this.peerMessageHandlers[message.type].bind(this) as (
            data: MessageType[typeof message.type],
            peerId: string
        ) => Promise<void> | void
        await handler(message.data, peerId)
    }

    async handleMatchModeState(data: MatchModeStateData) {
        console.log(data)
        if (data.event == "start") {
            MatchMode.getInstance().setMatchModeConfig(data.config)
            await MatchMode.getInstance().start(false)
        }
        if (data.event == "cancel") {
            MatchMode.getInstance().sandboxModeStart()
            globalAddToast("info", "Match Mode Cancelled")
        }
    }

    handlePeerInfo(data: ClientInfo) {
        this._clientToObjectMap.set(data.clientId, [])
        this._clientToInfoMap.set(data.clientId, data)
        MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
    }

    async handleWorldInitialization(data: InitData) {
        World.physicsSystem = data.physicsSystem
        World.sceneRenderer.sceneObjects = await this.encodedAssemblyToSceneObjectMap(data.objects)
    }

    async encodedAssemblyToSceneObjectMap(assemblies: EncodedAssembly[]): Promise<Map<number, MirabufSceneObject>> {
        return new Map(
            await Promise.all(
                assemblies.map(async assembly => {
                    const object = await createMirabuf(mirabuf.Assembly.decode(assembly))
                    if (object == null) return

                    World.sceneRenderer.registerSceneObject(object)

                    return [object.id, object] as [number, MirabufSceneObject]
                })
            ).then(objects => objects.filter(n => n != null))
        )
    }

    handlePeerUpdate(data: UpdateObjectData[]) {
        data.forEach(({ sceneObjectKey, gamePiecesControlled, bodies }) => {
            const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey)
            if (sceneObject == null) {
                console.warn(
                    `Multiplayer SceneObject: ${sceneObjectKey} not found in sceneObjects map. Multiplayer SceneObjects must be initialized before being updated.`
                )
                return
            } else if (!(sceneObject instanceof MirabufSceneObject)) {
                console.error(`Multiplayer SceneObject: ${sceneObjectKey} not MirabufSceneObject`)
                return
            }

            // Set all the ejectables that are in activeEjectables but not gamePiecesControlled
            sceneObject.activeEjectables
                .filter(id => !gamePiecesControlled.includes(id.GetIndexAndSequenceNumber()))
                // We're not ejecting the actual game piece here, but the robots should be configured to eject in the same order so it's fine
                .forEach(_ => sceneObject.eject())

            // Set all the ejectables that are in gamePiecesControlled but not activeEjectables
            gamePiecesControlled
                .filter(id => !sceneObject.activeEjectables.map(n => n.GetIndexAndSequenceNumber()).includes(id))
                .forEach(id => {
                    const bodyId = new JOLT.BodyID(id)
                    return sceneObject.setEjectable(bodyId)
                })

            // Sets the physics data for each body in the assembly
            bodies.forEach(({ bodyId, linearVelocityStr, angularVelocityStr, positionStr, rotationStr }) => {
                const lin: { x: number; y: number; z: number } = JSON.parse(linearVelocityStr)
                const ang: { x: number; y: number; z: number } = JSON.parse(angularVelocityStr)
                const pos: { x: number; y: number; z: number } = JSON.parse(positionStr)
                const rot: { x: number; y: number; z: number; w: number } = JSON.parse(rotationStr)

                const linearVelocity = new JOLT.Vec3(lin.x, lin.y, lin.z)
                const angularVelocity = new JOLT.Vec3(ang.x, ang.y, ang.z)
                const position = new JOLT.RVec3(pos.x, pos.y, pos.z)
                const rotation = new JOLT.Quat(rot.x, rot.y, rot.z, rot.w)

                const joltBodyId = new JOLT.BodyID(bodyId)

                const clientBody = World.physicsSystem.getBody(joltBodyId)
                if (!clientBody) {
                    console.error(`Body ${bodyId} on Scene Object ${sceneObject.assemblyName} not found`)
                    return
                }

                clientBody.SetLinearVelocity(linearVelocity)
                clientBody.SetAngularVelocity(angularVelocity)
                World.physicsSystem.setBodyPosition(joltBodyId, position)
                World.physicsSystem.setBodyRotation(joltBodyId, rotation)
            })
        })
    }

    handleCollision(data: UpdateObjectData[]) {
        // TODO Expand on this logic
        if (this.lastSentCollisionTimestamp < COLLISION_TIMEOUT) return

        this.handlePeerUpdate(data)
    }

    async handleNewObject(data: InitObjectData, peerId: string) {
        let assembly: mirabuf.Assembly | undefined
        if (data.assembly) {
            const returnedInfo = await MirabufCachingService.cacheLocalAndReturn(
                data.assembly.buffer as ArrayBuffer,
                data.miraType
            )
            if (!returnedInfo) {
                console.warn("nothing returned from caching function")
                return
            }
            assembly = returnedInfo?.assembly
        } else {
            assembly = await MirabufCachingService.get(data.assemblyHash)
        }
        if (!assembly) {
            console.log("needAssembly")
            await this.send(peerId, {
                type: "needAssembly",
                data: { assemblyHash: data.assemblyHash, sceneObjectKey: data.sceneObjectKey },
            })
            return
        }

        const object = await createMirabuf(assembly)
        if (object == null) return

        object.setPreferenceData(data.initialPreferences)
        object.nameOverride =
            (this._clientToInfoMap.get(peerId)?.displayName ?? peerId) +
            " " +
            (this._clientToObjectMap.get(peerId)?.length ?? "0")

        console.log("Registering object", object, data)
        World.sceneRenderer.registerSceneObject(object, data.sceneObjectKey)

        this._clientToObjectMap.get(peerId)?.push(object.id) || this._clientToObjectMap.set(peerId, [object.id])
    }

    async handleAssemblyRequest(data: AssemblyRequestData, peerId: string) {
        const sceneObjectKey = data.sceneObjectKey

        const assembly = await MirabufCachingService.getEncoded(data.assemblyHash)
        if (!assembly || !assembly.info) {
            console.error(`Failed to get assembly: ${data.assemblyHash} from cache`)
            return
        }
        const { buffer, info } = assembly

        const encodedAssembly = new Uint8Array(buffer) as EncodedAssembly

        const message: Message = {
            type: "newObject",
            data: {
                sceneObjectKey,
                assembly: encodedAssembly,
                assemblyHash: info.hash,
                miraType: info.miraType,
                initialPreferences: (
                    World.sceneRenderer.sceneObjects.get(data.sceneObjectKey)! as MirabufSceneObject
                ).getPreferenceData(),
            },
        }

        await this.send(peerId, message)
    }

    handleDeleteObject(sceneObjectKey: number, peerId: string) {
        this._clientToObjectMap.delete(peerId)

        const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey)
        if (!sceneObject || !(sceneObject instanceof MirabufSceneObject)) return

        sceneObject.dispose()
        World.sceneRenderer.removeSceneObject(sceneObjectKey)
    }

    handleObjectConfiguration(data: ObjectPreferences) {
        const sceneObject = World.sceneRenderer.sceneObjects.get(data.sceneObjectKey) as MirabufSceneObject
        sceneObject.setPreferenceData(data.objectConfigurationData)
    }

    disableObjectPhysics(sceneObjectKey: number) {
        const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey) as MirabufSceneObject
        sceneObject.disablePhysics()
    }

    enableObjectPhysics(sceneObjectKey: number) {
        const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey) as MirabufSceneObject
        sceneObject.enablePhysics()
    }

    handleMetadataUpdate(data: MetadataUpdateData) {
        const sceneObject = World.sceneRenderer.sceneObjects.get(data.sceneObjectKey)
        if (!sceneObject || !(sceneObject instanceof MirabufSceneObject)) return

        sceneObject.multiplayerInfo = data
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

    getOwnSceneObjects(): number[] {
        return this._clientToObjectMap.get(this.clientId) ?? []
    }

    getOwnRobots(): MirabufSceneObject[] {
        return (this._clientToObjectMap.get(this.clientId) ?? [])
            .map(id => World.sceneRenderer.sceneObjects.get(id))
            .filter(obj => obj instanceof MirabufSceneObject)
            .filter(obj => obj.miraType == MiraType.ROBOT)
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
