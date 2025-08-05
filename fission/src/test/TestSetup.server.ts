import type http from "node:http"
import path from "node:path"
import express from "express"
import { ExpressPeerServer } from "peer"

let server: http.Server | undefined
const ASSET_PORT = 3001
const serveDirectory = path.join(process.cwd(), "public/Downloadables")
export async function setup() {
    if (server) {
        return
    }
    console.log("Starting testing server...")
    const expressServer = express()

    await new Promise<void>((resolve, reject) => {
        if (!expressServer) {
            console.warn("no server")
            return
        }

        server = expressServer.listen(ASSET_PORT, "127.0.0.1", () => {
            console.log(`Started testing server on port ${ASSET_PORT}`)
            resolve()
        })
        const peerjsServer = ExpressPeerServer(server, {
            allow_discovery: true,
            path: "/",
        })
        expressServer.use("/Downloadables/", express.static(serveDirectory))
        expressServer.use("/", peerjsServer)

        server.once("error", err => {
            console.error("Failed to start testing server:", err)
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
                    console.error("Error stopping testing server:", err)
                    reject(err)
                    return
                }

                console.log("testing server stopped.")
                server = undefined
                resolve()
            })
        })
    }
}
