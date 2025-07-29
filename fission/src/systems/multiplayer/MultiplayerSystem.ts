import Peer, { DataConnection } from "peerjs"
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PhysicsSystem from "../physics/PhysicsSystem"
import World from "../World"
import type { ClientInfo, InitMultiplayerObjectData, Message } from "./types"

class PeerConnection {
    readonly client: Peer
    readonly roomId: string
    readonly connections: DataConnection[] = []
    readonly clientId: string
    isHost: boolean = false
    info: ClientInfo
    readonly handlePeerMessage: (peer: string, data: Message) => void

    public static async create(
        handlePeerMessage: (peer: string, data: Message) => void,
        roomId: string
    ): Promise<PeerConnection> {
        const clientId = await generateId(roomId)
        return new PeerConnection(handlePeerMessage, roomId, clientId)
    }

    public static async createHost(handlePeerMessage: (peer: string, data: Message) => void): Promise<PeerConnection> {
        const room = Math.random().toString(10).substring(2, 9)
        return this.create(handlePeerMessage, room)
    }

    private constructor(handlePeerMessage: (peer: string, data: Message) => void, roomId: string, clientId: string) {
        this.roomId = roomId
        this.clientId = clientId
        this.info = { clientId: this.clientId, displayName: this.clientId }
        this.client = new Peer(this.clientId, {
            host: window.location.hostname,
            port: 9000,
            path: "/",
        })
        this.client.on("error", console.log)
        this.client.on("disconnected", console.log)
        this.client.on("call", console.log)
        this.client.on("close", console.log)
        this.handlePeerMessage = handlePeerMessage

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
    initWorld(physicsSystem: PhysicsSystem) {
        const sceneObjects: InitMultiplayerObjectData[] = [...World.sceneRenderer.sceneObjects.entries()]
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
        const message: Message = {
            type: "init",
            data: { physicsSystem, objects: sceneObjects },
        }
        this.connections.forEach(c => c.send(message))
    }

    setupConnectionHandlers(conn: DataConnection) {
        if (this.connections.includes(conn)) {
            console.warn("Setting up connection for", conn.peer, "again")
            return
        }
        conn.on("open", async () => {
            console.log("Connection opened")
            this.connections.push(conn)
            await this.send(conn.peer, { type: "info", data: this.info })
        })

        conn.on("data", (data: unknown) => {
            this.handlePeerMessage(conn.peer, data as Message)
        })

        conn.on("close", () => {
            this.handlePeerMessage(conn.peer, { type: "robotLeft", data: { sceneObjectKey: 0 } }) // TODO Get actual sceneObjectKey
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

    async send(peer: string, message: Message) {
        const conn = this.connections.find(c => c.peer == peer)
        if (!conn) {
            console.warn("Can't find peer", peer)
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

export default PeerConnection
