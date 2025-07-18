import sirv from "sirv";
import http from "node:http";
import path from "node:path"
import type { AddressInfo } from "node:net";

let server: http.Server | undefined;

interface StartStaticServerOptions {
    staticDir: string;
    port?: number;
    host?: string;
    dev?: boolean;
    single?: boolean;
    allowedOrigin?: string;
}

export async function startStaticServer(options: StartStaticServerOptions) {
    if (server) {
        return;
    }

    console.log("Starting static file server...");

    const {
        staticDir,
        port = 0,
        host = "127.0.0.1",
        dev = true,
        single = true,
        allowedOrigin = "*",
    } = options;

    const assets = sirv(staticDir, {
        dev,
        single,
    });

    server = http.createServer((req, res) => {
        res.setHeader("Access-Control-Allow-Origin", allowedOrigin);
        res.setHeader("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
        res.setHeader(
            "Access-Control-Allow-Headers",
            "Origin, X-Requested-With, Content-Type, Accept",
        );

        if (req.method === "OPTIONS") {
            res.writeHead(204);
            res.end();
            return;
        }
        console.log(req.url)
        assets(req, res);
    });

    await new Promise<void>((resolve, reject) => {
        server!.listen(port, host, () => {
            const addressInfo = server!.address() as AddressInfo;
            const serverUrl = `http://${addressInfo.address}:${addressInfo.port}`;

            console.log(
                `Static file server started on ${serverUrl} serving from ${staticDir}`,
            );

            resolve();
        });

        server!.once("error", (err) => {
            console.error("Failed to start static file server:", err);
            server = undefined;
            reject(err);
        });
    });
}

export async function teardown() {
    if (server) {
        await new Promise<void>((resolve, reject) => {
            server!.close((err) => {
                if (err) {
                    console.error("Error stopping static file server:", err);
                    reject(err);
                    return;
                }

                console.log("Static file server stopped.");
                server = undefined;
                resolve();
            });
        });
    }
}

export async function setup() {
    await startStaticServer({staticDir: path.join(process.cwd(), "public"), port:3001})
}