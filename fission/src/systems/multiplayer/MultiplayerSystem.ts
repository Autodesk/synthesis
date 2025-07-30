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
    UpdateMultiplayerObjectData as UpdateObjectData,
} from "./types"

const COLLISION_TIMEOUT = 500

class MultiplayerSystem {
    readonly client: Peer
    readonly roomId: string
    readonly connections: DataConnection[] = []
    readonly clientId: string

    info: ClientInfo
    lastSentCollisionTimestamp: number = Date.now()
    connected: boolean = false
    otherPeers: string[] = []

    public static async create(roomId: string, isHost: boolean = false): Promise<MultiplayerSystem> {
        const clientId = await generateId(roomId)
        return new MultiplayerSystem(roomId, clientId, isHost)
    }

    public static async createHost(): Promise<MultiplayerSystem> {
        const room = Math.random().toString(10).substring(2, 9)
        return this.create(room, true)
    }

    private constructor(roomId: string, clientId: string, isHost: boolean = false) {
        this.roomId = roomId
        this.clientId = clientId
        this.info = { clientId: this.clientId, displayName: this.clientId, isHost }

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
        if (this.connections.includes(conn)) {
            console.warn("Setting up connection for", conn.peer, "again")
            return
        }
        conn.on("open", async () => {
            console.log("Connection opened")
            this.connections.push(conn)
            this.send(conn.peer, { type: "info", data: this.info })
        })

        conn.on("data", (data: unknown) => {
            this.handlePeerMessage(data as Message)
        })

        conn.on("close", () => {
            this.handlePeerMessage({
                type: "robotLeft",
                data: { sceneObjectKey: 0 },
            }) // TODO Get actual sceneObjectKey
            this.connections.splice(
                this.connections.findIndex(c => c == conn),
                1
            )
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

    handlePeerInfo(data: ClientInfo) {}
    handleWorldInitialization(data: InitData) {}
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
        if (this.lastSentCollisionTimestamp < COLLISION_TIMEOUT) return

        World.physicsSystem = data.physicsSystem
        World.sceneRenderer.sceneObjects = data.sceneObject
    }

    handleNewObject(data: InitObjectData) {
        World.sceneRenderer.sceneObjects.set(data.key, data.sceneObject)
    }

    async send(peer: string, message: Message) {
        const conn = this.connections.find(c => c.peer == peer)
        if (!conn) {
            console.warn("Couldn't find peer: ", peer)
            return
        }
        await conn.send(message)
    }

    async broadcast(message: Message) {
        return await Promise.all(this.connections.map(connection => connection.send(message)))
    }

    getOtherPeerIds(): string[] {
        return this.connections.map(c => c.peer)
    }
}

const localStorageKey = "multiplayer_clientid"

async function generateId(roomId?: string): Promise<string> {
    let id =
        (import.meta.env.DEV ? new URLSearchParams(window.location.search).get("uid") : undefined) ??
        window.localStorage.getItem(localStorageKey)
    if (id == null) {
        id = `client_${Math.random().toString(36).substring(2, 9)}`
        window.localStorage.setItem(localStorageKey, id)
    }
    if (roomId) {
        return `${id}-${await createSha256Hash(roomId)}`
    }
    return id
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

export default MultiplayerSystem
