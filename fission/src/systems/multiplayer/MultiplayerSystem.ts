/** biome-ignore-all lint/correctness/noUndeclaredVariables: <explanation> */
import Peer, { DataConnection } from "peerjs"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import PhysicsSystem from "../physics/PhysicsSystem"
import World from "../World"
import type {
    ClientInfo,
    CollisionData,
    EncodedAssembly,
    InitData,
    InitObjectData,
    Message,
    UpdateObjectData,
} from "./types"
import { mirabuf } from "@/proto/mirabuf"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import { globalAddToast, globalOpenModal } from "@/components/GlobalUIControls.ts"
import JOLT from "@/util/loading/JoltSyncLoader"

const COLLISION_TIMEOUT = 500

class MultiplayerSystem {
    private readonly _client: Peer
    private readonly _connections: Map<string, DataConnection> = new Map()
    readonly roomId: string
    readonly clientId: string

    private readonly _clientToInfoMap: Map<string, ClientInfo> = new Map()
    // TODO Update this system to be one-to-many
    private readonly _clientToRobotMap: Map<string, number | null> = new Map() // clientId -> sceneObjectKey

    readonly info: ClientInfo
    lastSentCollisionTimestamp: number = Date.now()

    public static async create(roomId: string, displayName: string, isHost: boolean): Promise<MultiplayerSystem> {
        const clientId = await generateId(roomId)
        return new MultiplayerSystem(roomId, clientId, displayName, isHost)
    }

    private constructor(roomId: string, clientId: string, displayName: string, isHost: boolean = false) {
        this.roomId = roomId
        this.clientId = clientId
        this.info = { clientId: this.clientId, displayName: displayName, isHost, creationTime: Date.now() }

        this._client = new Peer(this.clientId, {
            host: window.location.hostname,
            port: 9000,
            path: "/",
        })
        this._client.on("error", console.log)
        this._client.on("disconnected", console.log)
        this._client.on("call", console.log)
        this._client.on("close", console.log)

        this._client.on("open", async (id: string) => {
            console.log(`Broker connection opened: ID - ${id}`)
            const peerCount = await this.connectToRoom()

            if (peerCount == 0 && !isHost) {
                globalAddToast("warning", `Could not find room`, this.roomId)
                this.destroy()
                World.setMultiplayerSystem(undefined)
                MultiplayerStateEvent.dispatch(MultiplayerStateEventType.JOIN_ROOM)
                globalOpenModal("multiplayer-lobby")
            }
        })

        this._client.on("connection", async conn => {
            console.log("Receiving Connection: ", conn.peer)
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
        window.multiplayer = this
    }

    async connectToRoom() {
        const roomHash = await createSha256Hash({ roomId: this.roomId })

        const peersPromise = new Promise<string[]>(resolve => this._client.listAllPeers(resolve))
        const peers = await peersPromise

        console.log(`Peers: ${peers}`)

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

                    console.log(`Initiating Connection: ${peer}`)
                    return true
                })
        ).then(res => res.filter(success => success).length)

        MultiplayerStateEvent.dispatch(MultiplayerStateEventType.JOIN_ROOM)
        return peerCount
    }

    // Called by the host, initializes the world with some defined set of objects, robots can be spawned in later
    async initWorld(physicsSystem: PhysicsSystem) {
        const sceneObjects = [...World.sceneRenderer.sceneObjects.values()]
            .filter((sceneObject): sceneObject is MirabufSceneObject => sceneObject instanceof MirabufSceneObject)
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
            console.log("Connection opened", conn.peer)
            this._connections.set(conn.peer, conn)
            MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
            await this.send(conn.peer, { type: "info", data: this.info })
        })

        conn.on("data", (data: unknown) => {
            this.handlePeerMessage(data as Message, conn.peer)
        })

        conn.on("close", () => {
            this.handlePeerMessage(
                {
                    type: "robotLeft",
                    data: { sceneObjectKey: 0 },
                },
                conn.peer
            ) // TODO Get actual sceneObjectKey

            this._connections.delete(conn.peer)
            // TODO handle host transition

            if (this._host == null) {
                const newHost = this._peers.reduce((prev, current) =>
                    (this._clientToInfoMap.get(prev.peer)?.creationTime ?? Infinity) <
                    (this._clientToInfoMap.get(current.peer)?.creationTime ?? Infinity)
                        ? prev
                        : current
                )
                this._clientToInfoMap.get(newHost.peer)!.isHost = true // TODO: enforce that everybody agrees
            }

            MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
            console.log("Connection closed:", conn.peer)
        })
        conn.on("iceStateChanged", console.log)

        conn.on("error", (err: Error) => {
            console.error("Connection error:", err)
        })
    }

    handlePeerMessage(message: Message, peerId: string) {
        console.log(`Received Message of Type: ${message.type}`)
        switch (message.type) {
            case "info":
                this.handlePeerInfo(message.data)
                break
            case "init":
                this.handleWorldInitialization(message.data)
                break
            case "update":
                this.handlePeerUpdate(message.data)
                break
            case "collision":
                this.handleCollision(message.data)
                break
            case "newObject":
                this.handleNewObject(message.data, peerId)
                break
        }
    }

    handlePeerInfo(data: ClientInfo) {
        this._clientToRobotMap.set(data.clientId, null)
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
        data.forEach(({ sceneObjectKey, linearVelocityStr, angularVelocityStr, positionStr, rotationStr }) => {
            // const rootBody: Jolt.Body = JSON.parse(encodedRootBody)
            const lin: { x: number; y: number; z: number } = JSON.parse(linearVelocityStr)
            const ang: { x: number; y: number; z: number } = JSON.parse(angularVelocityStr)
            const pos: { x: number; y: number; z: number } = JSON.parse(positionStr)
            const rot: { x: number; y: number; z: number; w: number } = JSON.parse(rotationStr)

            const linearVelocity = new JOLT.Vec3(lin.x, lin.y, lin.z)
            const angularVelocity = new JOLT.Vec3(ang.x, ang.y, ang.z)
            const position = new JOLT.RVec3(pos.x, pos.y, pos.z)
            const rotation = new JOLT.Quat(rot.x, rot.y, rot.z, rot.w)

            const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey)
            if (sceneObject == null) {
                console.error(
                    `Multiplayer SceneObject: ${sceneObjectKey} not found in sceneObjects map. Multiplayer SceneObjects must be initialized before being updated.`
                )
                return
            } else if (!(sceneObject instanceof MirabufSceneObject)) {
                console.error(`Multiplayer SceneObject: ${sceneObjectKey} not MirabufSceneObject`)
                return
            }

            const clientMechanism = sceneObject.mechanism
            const clientBodyId = clientMechanism.nodeToBody.get(clientMechanism.rootBody)
            if (!clientBodyId) {
                console.error(`Body not found`)
                return
            }

            const clientBody = World.physicsSystem.getBody(clientBodyId)!
            clientBody.SetLinearVelocity(linearVelocity)
            clientBody.SetAngularVelocity(angularVelocity)
            World.physicsSystem.setBodyPosition(clientBodyId, position)
            World.physicsSystem.setBodyRotation(clientBodyId, rotation)
        })
    }

    handleCollision(data: CollisionData) {
        // TODO Expand on this logic
        if (this.lastSentCollisionTimestamp < COLLISION_TIMEOUT) return

        World.physicsSystem = data.physicsSystem
        World.sceneRenderer.sceneObjects = data.sceneObjects
    }

    async handleNewObject(data: InitObjectData, peerId: string) {
        const assembly = mirabuf.Assembly.decode(data.assembly)
        const object = await createMirabuf(assembly)
        if (object == null) return

        object.id = data.sceneObjectKey
        World.sceneRenderer.registerSceneObject(object)

        this._clientToRobotMap.set(peerId, object.id)
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
        console.log(`New Message: ${message.type}`)
        return await Promise.all(this._peers.map(peer => peer.send(message)))
    }

    getClientSceneObjectId(): number | null {
        return this._clientToRobotMap.get(this.clientId) ?? null
    }
    newClientSceneObject(objectId: number) {
        this._clientToRobotMap.set(this.clientId, objectId)
    }

    get peerIDs(): string[] {
        return [...this._connections.keys()]
    }

    private get _peers() {
        return [...this._connections.values()]
    }
    private get _host() {
        return this._peers.find(conn => this._clientToInfoMap.get(conn.peer)?.isHost)
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
