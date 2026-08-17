/// <reference lib="webworker" />

import type { FromWorkerMessage, ToWorkerMessage } from "@/systems/multiplayer/MultiplayerTypes.ts"

import { Mutex } from "async-mutex"
import MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"

let socket: MultiplayerWebsocket | undefined = undefined

const connectMutex = new Mutex()

async function tryConnect(url: string): Promise<void> {
    await connectMutex.runExclusive(() => {
        if (socket?.ready) {
            return
        }

        socket = new MultiplayerWebsocket(url)

        socket.addEventListener("open", () => {
            postMessage({ event: "open" })
        })
        socket.addEventListener("close", () => {
            postMessage({ event: "close" })
        })
        socket.addEventListener("error", e => {
            postMessage({ event: "error", data: e.type })
        })
        socket.onPeerMessage = v => {
            postMessage({ event: "peerMessage", data: v })
        }
        socket.onServerMessage = v => {
            postMessage({ event: "serverMessage", data: v })
        }
    })
}

async function tryDisconnect(): Promise<void> {
    await connectMutex.runExclusive(() => {
        if (!socket) return
        socket.close()
        socket = undefined
    })
}

async function postMessage(message: FromWorkerMessage) {
    self.postMessage(message)
}

async function handleMessage({ event, data }: ToWorkerMessage) {
    switch (event) {
        case "connect": {
            await tryConnect(data.url)
            break
        }
        case "initialize": {
            socket!.init(data.roomId, data.displayName)
            break
        }
        case "serverMessage": {
            socket!.sendServer(data)
            break
        }
        case "peerMessage": {
            socket!.sendPeer(data)
            break
        }
        case "disconnect": {
            await tryDisconnect()
            break
        }
        default: {
            console.warn(`Unrecognized command '${event}'`)
            break
        }
    }
}

self.addEventListener("message", e => {
    void handleMessage(e.data)
})
