import { Mutex } from "async-mutex"

// Generic WebSocket worker shared by WPILib and FTC codesim connections.

let socket: WebSocket | undefined = undefined

const connectMutex = new Mutex()

let intervalHandle: NodeJS.Timeout | undefined = undefined
let reconnect = false
let connectionUrl: string | undefined = undefined
const RECONNECT_INTERVAL = 1000

function socketOpen(): boolean {
    return (socket && socket.readyState == WebSocket.OPEN) ?? false
}

function socketConnecting(): boolean {
    return (socket && socket.readyState == WebSocket.CONNECTING) ?? false
}

async function tryConnect(): Promise<void> {
    await connectMutex.runExclusive(() => {
        if (!connectionUrl) return
        if ((socket?.readyState ?? WebSocket.CLOSED) == WebSocket.OPEN) {
            return
        }

        socket = new WebSocket(connectionUrl)

        socket.addEventListener("open", () => {
            self.postMessage({ status: "open" })
        })
        socket.addEventListener("error", () => {
            self.postMessage({ status: "error" })
        })
        socket.addEventListener("close", () => {
            self.postMessage({ status: "close" })
        })

        socket.addEventListener("message", onMessage)
    })
}

async function tryDisconnect(): Promise<void> {
    await connectMutex.runExclusive(() => {
        if (!socket) return

        socket.close()
        socket = undefined
    })
}

function onMessage(event: MessageEvent) {
    self.postMessage(event.data)
}

self.addEventListener("message", e => {
    switch (e.data.command) {
        case "enable": {
            connectionUrl = e.data.url ?? connectionUrl
            reconnect = e.data.reconnect ?? false
            const intervalFunc = () => {
                if (intervalHandle != undefined && !socketOpen() && !socketConnecting()) {
                    tryConnect()
                }

                if (!reconnect) {
                    clearInterval(intervalHandle)
                    intervalHandle = undefined
                }
            }
            if (intervalHandle != undefined) {
                clearInterval(intervalHandle)
            }
            intervalHandle = setInterval(intervalFunc, RECONNECT_INTERVAL)
            break
        }
        case "disable": {
            clearInterval(intervalHandle)
            intervalHandle = undefined
            tryDisconnect()
            break
        }
        case "update": {
            if (socketOpen()) {
                socket!.send(JSON.stringify(e.data.data))
            }
            break
        }
        default: {
            console.warn(`Unrecognized command '${e.data.command}'`)
            break
        }
    }
})
