import { Mutex } from "async-mutex"

let socket: WebSocket | undefined = undefined

const connectMutex = new Mutex()

async function tryConnect(port: number | undefined): Promise<void> {
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
        case "connect":
            tryConnect(e.data.port)
            break
        case "disconnect":
            tryDisconnect()
            break
        case "update":
            console.log(`update in worker ${JSON.stringify(e.data)}`)
            if (socket) socket.send(JSON.stringify(e.data.data))
            break
        default:
            console.warn(`Unrecognized command '${e.data.command}'`)
            break
    }
})
