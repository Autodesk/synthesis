import Peer, { DataConnection } from "peerjs"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PhysicsSystem from "../physics/PhysicsSystem"
import World from "../World"
import type {
    ClientInfo,
    CollisionData,
    InitData,
    InitObjectData,
    Message,
    UpdateObjectData as UpdateObjectData,
} from "./types"

const COLLISION_TIMEOUT = 500


class MultiplayerSystem {
    private readonly client: Peer
    private readonly connections: Map<string, DataConnection> = new Map()
    readonly roomId: string
    readonly clientId: string

    private readonly clientToInfoMap:Map<string, ClientInfo> = new Map()
    private readonly clientToRobotMap: Map<string, number | null> = new Map() // clientId -> sceneObjectKey

    readonly info: ClientInfo
    lastSentCollisionTimestamp: number = Date.now()

    public static async create(roomId: string, displayName:string, isHost:boolean): Promise<MultiplayerSystem> {
        const clientId = await generateId(roomId)
        return new MultiplayerSystem(roomId, clientId, displayName, isHost)
    }

    private constructor(roomId: string, clientId: string, displayName:string, isHost: boolean = false) {
        this.roomId = roomId
        this.clientId = clientId
        this.info = { clientId: this.clientId, displayName: displayName, isHost, creationTime: Date.now() }

        this.client = new Peer(this.clientId, {
            host: window.location.hostname,
            port: 9000,
            path: "/",
        })
        this.client.on("error", console.log)
        this.client.on("disconnected", console.log)
        this.client.on("call", console.log)
        this.client.on("close", console.log)

        this.client.on("open", async (id: string) => {
            console.log(`Broker connection opened: ID - ${id}`)
            await this.connectToRoom()
        })

        this.client.on("connection", async conn => {
            console.log("Receiving Connection: ", conn.peer)
            if (conn.metadata.authHash != (await createSha256Hash(this.roomId + conn.peer + this.clientId))) {
                conn.close()
                console.warn("Blocking unauthorized connection from " + conn.peer)
                return
            }
            this.setupConnectionHandlers(conn)
        })
    }

    async connectToRoom() {
        const roomHash = await createSha256Hash(this.roomId)

        const peersPromise = new Promise<string[]>(resolve => this.client.listAllPeers(resolve))
        const peers = await peersPromise

        console.log(`Peers: ${peers}`)

        await Promise.all(
            peers
                .filter(peer => peer !== this.clientId && peer.split("-")[1] == roomHash)
                .map(async peer => {
                    const conn = this.client.connect(peer, {
                        metadata: {
                            authHash: await createSha256Hash(this.roomId + this.clientId + peer),
                        },
                    })
                    this.setupConnectionHandlers(conn)

                    console.log(`Initiating Connection: ${peer}`)
                })
        )
        MultiplayerStateEvent.dispatch(MultiplayerStateEventType.JOIN_ROOM)
    }

    // Called by the host, initializes the world with some defined set of objects, robots can be spawned in later
    async initWorld(physicsSystem: PhysicsSystem) {
        const sceneObjects: InitObjectData[] = [...World.sceneRenderer.sceneObjects.entries()]
            .filter(
                (sceneObjectPair): sceneObjectPair is [number, MirabufSceneObject] =>
                    sceneObjectPair[1] instanceof MirabufSceneObject
            )
            .map(([key, sceneObject]) => {
                return {
                    key,
                    sceneObject,
                }
            })
        await this.broadcast({
            type: "init",
            data: { physicsSystem, objects: sceneObjects },
        })
    }

    setupConnectionHandlers(conn: DataConnection) {
        if (this.connections.has(conn.peer)) {
            console.warn("Setting up connection for", conn.peer, "again")
            return
        }
        conn.on("open", async () => {
            console.log("Connection opened", conn.peer)
            this.connections.set(conn.peer, conn)
            MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
            await this.send(conn.peer, { type: "info", data: this.info })
        })

        conn.on("data", (data: unknown) => {
            this.handlePeerMessage(data as Message)
        })

        conn.on("close", () => {
            this.handlePeerMessage({
                type: "robotLeft",
                data: { sceneObjectKey: 0 },
            }) // TODO Get actual sceneObjectKey

            this.connections.delete(conn.peer)
            // TODO handle host transition

            if (this._host == null) {
                const newHost = this._peers.reduce((prev, current) => (this.clientToInfoMap.get(prev.peer)?.creationTime ?? Infinity) < (this.clientToInfoMap.get(current.peer)?.creationTime ?? Infinity) ? prev : current)
                this.clientToInfoMap.get(newHost.peer)!.isHost = true // TODO: enforce that everybody agrees
            }

            MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
            console.log("Connection closed:", conn.peer)
        })
        conn.on("iceStateChanged", console.log)

        conn.on("error", (err: Error) => {
            console.error("Connection error:", err)
        })
    }

    handlePeerMessage(message: Message) {
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
                this.handleNewObject(message.data)
                break
        }
    }

    handlePeerInfo(data: ClientInfo) {
        this.clientToRobotMap.set(data.clientId, null)
        this.clientToInfoMap.set(data.clientId, data)
        MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
    }

    handleWorldInitialization(data: InitData) {
        World.physicsSystem = data.physicsSystem
        World.sceneRenderer.sceneObjects = this.initObjectDataToSceneObjectMap(data.objects)
    }
    initObjectDataToSceneObjectMap(objects: InitObjectData[]): Map<number, MirabufSceneObject> {
        return new Map(objects.map(object => [object.key, object.sceneObject]))
    }

    handlePeerUpdate(data: UpdateObjectData[]) {
        data.forEach(({ sceneObjectKey, mechanism, instance }) => {
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
            sceneObject.mechanism = mechanism
            sceneObject.mirabufInstance = instance
        })
    }

    handleCollision(data: CollisionData) {
        // TODO Expand on this logic
        if (this.lastSentCollisionTimestamp < COLLISION_TIMEOUT) return

        World.physicsSystem = data.physicsSystem
        World.sceneRenderer.sceneObjects = data.sceneObjects
    }

    handleNewObject(data: InitObjectData) {
        World.sceneRenderer.sceneObjects.set(data.key, data.sceneObject)
    }

    async send(peer: string, message: Message) {
        const conn = this.connections.get(peer)
        if (!conn) {
            console.warn("Couldn't find peer: ", peer)
            return
        }
        await conn.send(message)
    }

    async broadcast(message: Message) {
        return await Promise.all(this._peers.map((peer) => peer.send(message)))
    }

    getClientSceneObjectId(): number | null {
        return this.clientToRobotMap.get(this.clientId) ?? null
    }

    get peerIDs(): string[] {
        return Array.from(this.connections.keys())
    }

    private get _peers() {
        return [...this.connections.values()]
    }
    private get _host() {
        return this._peers.find((conn) => this.clientToInfoMap.get(conn.peer)?.isHost)
    }

    get peerInfo(): ClientInfo[] {
        return this.peerIDs.map((peerId) => (this.clientToInfoMap.get(peerId) ?? {clientId:peerId, displayName:peerId, isHost:false, creationTime:Infinity}))
    }

    get displayName():string {
        return this.info.displayName
    }

    public destroy() {
        this.connections.forEach((conn) => conn.close())
        this.connections.clear()
        this.client.destroy()
    }
}

const localStorageKey = "multiplayer_clientid"

async function generateId(roomId: string): Promise<string> {
    let id =
        (import.meta.env.DEV ? new URLSearchParams(window.location.search).get("uid") : undefined) ??
        window.localStorage.getItem(localStorageKey)
    if (id == null) {
        id = `client_${Math.random().toString(36).substring(2, 9)}`
        window.localStorage.setItem(localStorageKey, id)
    }
    return `${id}-${await createSha256Hash(roomId)}`
}

async function createSha256Hash(msg: string) {
    const msgBuffer = new TextEncoder().encode(msg)
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
    PEER_CHANGE
}

export class MultiplayerStateEvent extends Event {

    private constructor(event:MultiplayerStateEventType) {
        super(`MultiplayerStateChange${event}`);
    }

    public static dispatch(eventType:MultiplayerStateEventType) {
        const event = new MultiplayerStateEvent(eventType)
        window.dispatchEvent(event)
    }

    public static addEventListener(eventType:MultiplayerStateEventType, cb:EventListenerOrEventListenerObject) {
        window.addEventListener(`MultiplayerStateChange${eventType}`, cb)
        return () => {
            window.removeEventListener(`MultiplayerStateChange${eventType}`, cb)
        }
    }

}

export default MultiplayerSystem
