// frame bytes are too large for the HALSim SimDevice channel (numbers/booleans only), so
// the robot process hosts a WebSocket server (SyntheSimJava's CameraFrameServer) and we
// connect out to stream binary "<device>\n<jpeg-bytes>" messages to it

const PORT = 5808
const RETRY_MS = 2000
// drop frames instead of queuing when the socket is backed up, else a slow consumer makes
// frames pile up and arrive in erratic bursts
const MAX_BUFFERED_BYTES = 1_000_000

const ENCODER = new TextEncoder()

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

export function sendCameraFrame(device: string, jpeg: Uint8Array): void {
    ensureSocket()
    if (!socket || socket.readyState !== WebSocket.OPEN || socket.bufferedAmount >= MAX_BUFFERED_BYTES) return

    const header = ENCODER.encode(`${device}\n`)
    const msg = new Uint8Array(header.length + jpeg.length)
    msg.set(header, 0)
    msg.set(jpeg, header.length)
    socket.send(msg)
}
