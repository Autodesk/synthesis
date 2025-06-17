import { Mutex } from "async-mutex"

let socket: WebSocket | undefined = undefined

const connectMutex = new Mutex()

let intervalHandle: NodeJS.Timeout | undefined = undefined
let reconnect = false
const RECONNECT_INTERVAL = 1000

function socketOpen(): boolean {
    return (socket && socket.readyState == WebSocket.OPEN) ?? false
}

function socketConnecting(): boolean {
    return (socket && socket.readyState == WebSocket.CONNECTING) ?? false
}

async function tryConnect(port?: number): Promise<void> {
    await connectMutex
        .runExclusive(() => {
            if ((socket?.readyState ?? WebSocket.CLOSED) == WebSocket.OPEN) {
                return
            }

            socket = new WebSocket(`ws://localhost:${port ?? 3300}/wpilibws`)

            socket.addEventListener("open", () => {
                console.log("WS Opened")
                self.postMessage({ status: "open" })
            })
            socket.addEventListener("error", () => {
                console.log("WS Could not open")
                self.postMessage({ status: "error" })
            })
            socket.addEventListener("close", () => {
                self.postMessage({ status: "close" })
            })

            socket.addEventListener("message", onMessage)
        })
        .then(() => console.debug("Mutex released"))
}

async function tryDisconnect(): Promise<void> {
    await connectMutex.runExclusive(() => {
        if (!socket) return

        socket.close()
        socket = undefined
    })
}

// Posts incoming messages
function onMessage(event: MessageEvent) {
    self.postMessage(event.data)
}

// Sends outgoing messages
self.addEventListener("message", e => {
    switch (e.data.command) {
        case "enable": {
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
