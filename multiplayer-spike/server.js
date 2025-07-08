import { WebSocketServer } from "ws";
import express from "express";
import { v4 as uuidv4 } from "uuid";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const GAME_CONFIG = {
    TICK_RATE: 60, // Hz - Server simulation tick rate
    WORLD_SIZE: { width: 1000, height: 1000 }, // World boundaries
    ROBOT_SIZE: { width: 50, height: 50 },
    ROBOT_SPEED: 200, // Units per second
    PHYSICS_TIMESTEP: 1 / 60, // Fixed timestep for deterministic physics
};

class GameServer {
    constructor() {
        this.clients = new Map();
        this.robots = new Map();
        this.lastTick = Date.now();
        this.tickCount = 0;

        this.app = express();
        this.httpServer = null;
        this.wss = null;

        this.setupExpress();
        this.setupWebSocket();
    }

    setupExpress() {
        this.app.use(express.static(path.join(__dirname, "client")));

        this.app.get("/api/info", (req, res) => {
            res.json({
                connectedClients: this.clients.size,
                activeRobots: this.robots.size,
                tickRate: GAME_CONFIG.TICK_RATE,
                worldSize: GAME_CONFIG.WORLD_SIZE,
            });
        });
    }

    setupWebSocket() {
        this.httpServer = this.app.listen(3000, () => {
            console.log("server running on http://localhost:3000");
        });

        this.wss = new WebSocketServer({
            server: this.httpServer,
            path: "/game",
        });

        this.wss.on("connection", (ws, req) => {
            this.handleClientConnection(ws, req);
        });

        console.log("webSocket Game Server initialized");
    }

    handleClientConnection(ws, req) {
        const clientId = uuidv4();
        const robotId = uuidv4();

        const robot = {
            id: robotId,
            clientId: clientId,
            position: {
                x:
                    Math.random() *
                    (GAME_CONFIG.WORLD_SIZE.width -
                        GAME_CONFIG.ROBOT_SIZE.width),
                y:
                    Math.random() *
                    (GAME_CONFIG.WORLD_SIZE.height -
                        GAME_CONFIG.ROBOT_SIZE.height),
            },
            velocity: { x: 0, y: 0 },
            rotation: 0,
            lastInputTime: Date.now(),
            inputs: {
                forward: false,
                backward: false,
                left: false,
                right: false,
            },
        };

        this.clients.set(clientId, {
            id: clientId,
            ws: ws,
            robotId: robotId,
            lastPing: Date.now(),
            latency: 0,
        });

        this.robots.set(robotId, robot);

        console.log(
            `client ${clientId} connected with robot ${robotId} (count: ${this.clients.size})`
        );

        this.sendToClient(clientId, {
            type: "init",
            data: {
                clientId: clientId,
                robotId: robotId,
                worldSize: GAME_CONFIG.WORLD_SIZE,
                tickRate: GAME_CONFIG.TICK_RATE,
                robots: Array.from(this.robots.values()),
            },
        });

        this.broadcastToOthers(clientId, {
            type: "robotJoined",
            data: robot,
        });

        ws.on("message", (data) => {
            this.handleClientMessage(clientId, data);
        });

        ws.on("close", () => {
            this.handleClientDisconnection(clientId);
        });

        ws.on("error", (error) => {
            console.error(`ws error on ${clientId}:`, error);
            this.handleClientDisconnection(clientId);
        });

        this.sendPing(clientId);
    }

    handleClientMessage(clientId, data) {
        const client = this.clients.get(clientId);
        if (!client) return;

        try {
            const message = JSON.parse(data.toString());

            switch (message.type) {
                case "input":
                    this.handleInputMessage(clientId, message.data);
                    break;
                case "pong":
                    this.handlePongMessage(clientId, message.data);
                    break;
                default:
                    console.warn(`unknown msg type: ${message.type}`);
            }
        } catch (error) {
            console.error(`parse error from client ${clientId}:`, error);
        }
    }

    handleInputMessage(clientId, inputData) {
        const client = this.clients.get(clientId);
        if (!client) return;

        const robot = this.robots.get(client.robotId);
        if (!robot) return;

        robot.inputs = { ...inputData };
        robot.lastInputTime = Date.now();
    }

    handlePongMessage(clientId, data) {
        const client = this.clients.get(clientId);
        if (!client) return;

        const now = Date.now();
        client.latency = now - data.timestamp;
        client.lastPing = now;
    }

    handleClientDisconnection(clientId) {
        const client = this.clients.get(clientId);
        if (!client) return;

        console.log(`client ${clientId} disconnected`);

        if (client.robotId) {
            this.robots.delete(client.robotId);
            this.broadcastToOthers(clientId, {
                type: "robotLeft",
                data: { robotId: client.robotId },
            });
        }

        this.clients.delete(clientId);

        console.log(`clients remaining: ${this.clients.size}`);
    }

    startGameLoop() {
        console.log(`starting game loop at ${GAME_CONFIG.TICK_RATE}Hz`);

        setInterval(() => {
            this.tick();
        }, 1000 / GAME_CONFIG.TICK_RATE);

        setInterval(() => {
            this.sendPingsToAllClients();
        }, 5000);
    }

    tick() {
        const now = Date.now();
        const deltaTime = GAME_CONFIG.PHYSICS_TIMESTEP;

        for (const robot of this.robots.values()) {
            this.updateRobotPhysics(robot, deltaTime);
        }

        const worldState = {
            type: "worldState",
            data: {
                tick: this.tickCount,
                timestamp: now,
                robots: Array.from(this.robots.values()).map((robot) => ({
                    id: robot.id,
                    position: robot.position,
                    rotation: robot.rotation,
                    velocity: robot.velocity,
                })),
            },
        };

        this.broadcast(worldState);

        this.tickCount++;
        this.lastTick = now;
    }

    updateRobotPhysics(robot, deltaTime) {
        let targetVelocity = { x: 0, y: 0 };

        if (robot.inputs.forward) targetVelocity.y -= GAME_CONFIG.ROBOT_SPEED;
        if (robot.inputs.backward) targetVelocity.y += GAME_CONFIG.ROBOT_SPEED;
        if (robot.inputs.left) targetVelocity.x -= GAME_CONFIG.ROBOT_SPEED;
        if (robot.inputs.right) targetVelocity.x += GAME_CONFIG.ROBOT_SPEED;

        const smoothing = 0.8;
        robot.velocity.x =
            robot.velocity.x * (1 - smoothing) + targetVelocity.x * smoothing;
        robot.velocity.y =
            robot.velocity.y * (1 - smoothing) + targetVelocity.y * smoothing;

        robot.position.x += robot.velocity.x * deltaTime;
        robot.position.y += robot.velocity.y * deltaTime;

        robot.position.x = Math.max(
            0,
            Math.min(
                GAME_CONFIG.WORLD_SIZE.width - GAME_CONFIG.ROBOT_SIZE.width,
                robot.position.x
            )
        );
        robot.position.y = Math.max(
            0,
            Math.min(
                GAME_CONFIG.WORLD_SIZE.height - GAME_CONFIG.ROBOT_SIZE.height,
                robot.position.y
            )
        );

        if (
            Math.abs(robot.velocity.x) > 10 ||
            Math.abs(robot.velocity.y) > 10
        ) {
            robot.rotation =
                (Math.atan2(robot.velocity.y, robot.velocity.x) * 180) /
                Math.PI;
        }
    }

    sendToClient(clientId, message) {
        const client = this.clients.get(clientId);
        if (!client || client.ws.readyState !== 1) return; // 1 = OPEN

        try {
            client.ws.send(JSON.stringify(message));
        } catch (error) {
            console.error(`message drop to ${clientId}:`, error);
        }
    }

    broadcast(message) {
        const messageStr = JSON.stringify(message);

        for (const [clientId, client] of this.clients) {
            if (client.ws.readyState === 1) {
                // 1 = OPEN
                try {
                    client.ws.send(messageStr);
                } catch (error) {
                    console.error(`message drop to ${clientId}:`, error);
                }
            }
        }
    }

    broadcastToOthers(excludeClientId, message) {
        const messageStr = JSON.stringify(message);

        for (const [clientId, client] of this.clients) {
            if (clientId !== excludeClientId && client.ws.readyState === 1) {
                try {
                    client.ws.send(messageStr);
                } catch (error) {
                    console.error(`broadcast drop to ${clientId}:`, error);
                }
            }
        }
    }

    sendPing(clientId) {
        this.sendToClient(clientId, {
            type: "ping",
            data: { timestamp: Date.now() },
        });
    }

    sendPingsToAllClients() {
        const now = Date.now();

        for (const [clientId, client] of this.clients) {
            this.sendToClient(clientId, {
                type: "ping",
                data: { timestamp: now },
            });
        }
    }

    getStats() {
        const clients = Array.from(this.clients.values());
        return {
            connectedClients: this.clients.size,
            activeRobots: this.robots.size,
            averageLatency:
                clients.length > 0
                    ? clients.reduce((sum, client) => sum + client.latency, 0) /
                      clients.length
                    : 0,
            tickRate: GAME_CONFIG.TICK_RATE,
            uptime: process.uptime(),
        };
    }
}

const gameServer = new GameServer();
gameServer.startGameLoop();

// Graceful shutdown
process.on("SIGINT", () => {
    console.log("\nstopping...");
    if (gameServer.httpServer) {
        gameServer.httpServer.close(() => {
            console.log("closed http server");
            process.exit(0);
        });
    }
});

// Log stats every 30 seconds
setInterval(() => {
    const stats = gameServer.getStats();
    console.log(`PERIODIC STATS:`, {
        clients: stats.connectedClients,
        robots: stats.activeRobots,
        avgLatency: Math.round(stats.averageLatency) + "ms",
        uptime: Math.round(stats.uptime) + "s",
    });
}, 30000);
