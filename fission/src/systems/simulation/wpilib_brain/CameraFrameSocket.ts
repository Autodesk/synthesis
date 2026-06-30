// Frame bytes are too large for the HALSim SimDevice channel (numbers/booleans only), so
// the robot process hosts a WebSocket server (SyntheSimJava's CameraFrameServer) and we
// connect out to stream "<device>\n<base64-jpeg>" messages to it.

const PORT = 5808
const RETRY_MS = 2000
// Drop frames instead of queuing when the socket is backed up, so a slow consumer doesn't
// cause frames to pile up and arrive in erratic bursts.
const MAX_BUFFERED_BYTES = 1_000_000

let socket: WebSocket | undefined
let lastAttempt = Number.NEGATIVE_INFINITY

function ensureSocket(): void {
    if (socket && (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING)) return

    const now = performance.now()
    if (now - lastAttempt < RETRY_MS) return
    lastAttempt = now

    try {
        const ws = new WebSocket(`ws://localhost:${PORT}`)
        ws.addEventListener("close", () => {
            if (socket === ws) socket = undefined
        })
        ws.addEventListener("error", () => {
            if (socket === ws) socket = undefined
        })
        socket = ws
    } catch {
        socket = undefined
    }
}

export function sendCameraFrame(device: string, base64Jpeg: string): void {
    ensureSocket()
    if (socket && socket.readyState === WebSocket.OPEN && socket.bufferedAmount < MAX_BUFFERED_BYTES) {
        socket.send(`${device}\n${base64Jpeg}`)
    }
}
