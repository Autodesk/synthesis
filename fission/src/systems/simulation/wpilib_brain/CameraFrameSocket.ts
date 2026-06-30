/**
 * Outbound WebSocket used to stream rendered camera frames to the running robot code.
 *
 * Frame bytes are too large for the HALSim SimDevice channel (which only carries
 * numbers/booleans), so the robot process hosts a small WebSocket server (SyntheSimJava's
 * `CameraFrameServer`) and Synthesis connects out to it — mirroring how it already
 * connects to the HALSim WebSocket on `ws://localhost:3300`.
 *
 * Each message is the text `"<device>\n<base64-jpeg>"`. Connection is lazy and
 * self-healing: failures are retried quietly, and frames are dropped while disconnected.
 */

const PORT = 5808
const RETRY_MS = 2000
// Drop frames rather than queue them if the socket is backed up. Without this, a slow
// consumer causes frames to pile up in the send buffer and arrive in erratic bursts.
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
        // Swallow errors; the next send will trigger a throttled reconnect.
        ws.addEventListener("error", () => {
            if (socket === ws) socket = undefined
        })
        socket = ws
    } catch {
        socket = undefined
    }
}

/**
 * Streams a single encoded frame for a camera device. No-ops (dropping the frame) if the
 * robot's frame server isn't currently reachable.
 *
 * @param device The sim device key, e.g. `"USB Camera 0[0]"`.
 * @param base64Jpeg The frame encoded as a base64 JPEG (without the data-URL prefix).
 */
export function sendCameraFrame(device: string, base64Jpeg: string): void {
    ensureSocket()
    if (socket && socket.readyState === WebSocket.OPEN && socket.bufferedAmount < MAX_BUFFERED_BYTES) {
        socket.send(`${device}\n${base64Jpeg}`)
    }
}
