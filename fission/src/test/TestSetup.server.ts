import http from "node:http"
import path from "node:path"
import sirv from "sirv"

let server: http.Server | undefined
const PORT = 3001
const serveDirectory = path.join(process.cwd(), "public")
export async function setup() {
    if (server) {
        return
    }

    console.log("Starting static file server...")

    const assets = sirv(serveDirectory)

    server = http.createServer((req, res) => {
        res.setHeader("Access-Control-Allow-Origin", "*")
        res.setHeader("Access-Control-Allow-Methods", "GET")
        assets(req, res)
    })

    await new Promise<void>((resolve, reject) => {
        if (!server) {
            console.warn("no server")
            return
        }
        server.listen(PORT, "127.0.0.1", () => {
            console.log(`Serving files from ${serveDirectory} on port ${PORT} `)
            resolve()
        })

        server.once("error", err => {
            console.error("Failed to start static file server:", err)
            server = undefined
            reject(err)
        })
    })
}

export async function teardown() {
    if (server) {
        await new Promise<void>((resolve, reject) => {
            server!.close(err => {
                if (err) {
                    console.error("Error stopping static file server:", err)
                    reject(err)
                    return
                }

                console.log("Static file server stopped.")
                server = undefined
                resolve()
                process.exit(0)
            })
        })
    }
}
