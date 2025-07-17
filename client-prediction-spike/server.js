import { WebSocketServer } from "ws";
import express from "express";
import { v4 as uuidv4 } from "uuid";
import path from "path";
import { fileURLToPath } from "url";
import helmet from "helmet";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const GAME_CONFIG = {
    TICK_RATE: 60, // Hz - Server simulation tick rate
    WORLD_SIZE: { width: 1000, height: 1000 }, // World boundaries
    ROBOT_SIZE: { width: 50, height: 50 },
    ROBOT_SPEED: 200, // Units per second
    PHYSICS_TIMESTEP: 1 / 60, // Fixed timestep for deterministic physics
    PREDICTION_BUFFER_SIZE: 120, // Keep 2 seconds of history at 60fps
    RECONCILIATION_THRESHOLD: 4, // Pixels - correction threshold
};

class GameServer {
    constructor() {
        this.clients = new Map();
        this.robots = new Map();
        this.lastTick = Date.now();
        this.tickCount = 0;
        this.sequenceNumber = 0;
        this.inputHistory = new Map();
        this.stateHistory = new Map();

        // Enhanced metrics collection
        this.serverMetrics = {
            startTime: Date.now(),
            totalMessages: 0,
            totalInputs: 0,
            totalCorrections: 0,
            totalDivergence: 0,
            frameTimeSamples: [],
            clientLatencies: new Map(),
            inputRates: new Map(),
            correctionHistory: [],
            networkStats: {
                bytesReceived: 0,
                bytesSent: 0,
                messagesPerSecond: 0,
                lastSecondMessages: 0,
                lastSecondStart: Date.now()
            },
            performanceStats: {
                avgTickTime: 0,
                maxTickTime: 0,
                tickTimeSamples: [],
                memoryUsage: process.memoryUsage(),
                cpuUsage: 0
            }
        };

        this.app = express();
        this.httpServer = null;
        this.wss = null;

        this.setupExpress();
        this.setupWebSocket();
        this.startMetricsCollection();
    }

    setupExpress() {
        this.app.use(express.static(path.join(__dirname, "client")));
        this.app.use(helmet());

        this.app.get("/", (req, res) => {
            res.sendFile(path.join(__dirname, "client", "index.html"));
        });

        this.app.get("/dashboard", (req, res) => {
            res.sendFile(path.join(__dirname, "client", "dashboard.html"));
        });

        this.app.get("/api/info", (req, res) => {
            res.json({
                connectedClients: this.clients.size,
                activeRobots: this.robots.size,
                tickRate: GAME_CONFIG.TICK_RATE,
                worldSize: GAME_CONFIG.WORLD_SIZE,
                predictionEnabled: true,
                reconciliationThreshold: GAME_CONFIG.RECONCILIATION_THRESHOLD,
                serverMetrics: this.getDetailedStats(),
                uptime: Date.now() - this.serverMetrics.startTime
            });
        });

        this.app.get("/api/metrics", (req, res) => {
            res.json(this.getDetailedStats());
        });
    }

    setupWebSocket() {
        this.httpServer = this.app.listen(3000, () => {
            console.log(`Prediction Server running on http://localhost:3000`);
            console.log(`Tick Rate: ${GAME_CONFIG.TICK_RATE} Hz`);
            console.log(
                `Reconciliation Threshold: ${GAME_CONFIG.RECONCILIATION_THRESHOLD}px`
            );
        });

        this.wss = new WebSocketServer({ server: this.httpServer });

        this.wss.on("connection", (ws, req) => {
            this.handleClientConnection(ws, req);
        });

        this.startGameLoop();
    }

    startGameLoop() {
        const targetDelta = 1000 / GAME_CONFIG.TICK_RATE;

        const gameLoop = () => {
            const tickStart = Date.now();
            const now = Date.now();
            const deltaTime = (now - this.lastTick) / 1000;

            this.updateWorld(deltaTime);
            this.sendStateUpdates();
            this.sendPingsToAllClients();

            // Collect performance metrics
            const tickTime = Date.now() - tickStart;
            this.collectPerformanceMetrics(tickTime);

            this.lastTick = now;
            this.tickCount++;

            setTimeout(gameLoop, targetDelta);
        };

        gameLoop();
    }

    startMetricsCollection() {
        // Collect metrics every second
        setInterval(() => {
            this.updateNetworkStats();
            this.collectSystemMetrics();
            this.broadcastMetricsToClients();
        }, 1000);
    }

    collectPerformanceMetrics(tickTime) {
        this.serverMetrics.performanceStats.tickTimeSamples.push(tickTime);
        
        if (this.serverMetrics.performanceStats.tickTimeSamples.length > 60) {
            this.serverMetrics.performanceStats.tickTimeSamples.shift();
        }

        this.serverMetrics.performanceStats.avgTickTime = 
            this.serverMetrics.performanceStats.tickTimeSamples.reduce((a, b) => a + b, 0) / 
            this.serverMetrics.performanceStats.tickTimeSamples.length;

        this.serverMetrics.performanceStats.maxTickTime = Math.max(
            this.serverMetrics.performanceStats.maxTickTime, 
            tickTime
        );
    }

    updateNetworkStats() {
        const now = Date.now();
        if (now - this.serverMetrics.networkStats.lastSecondStart >= 1000) {
            this.serverMetrics.networkStats.messagesPerSecond = 
                this.serverMetrics.networkStats.lastSecondMessages;
            this.serverMetrics.networkStats.lastSecondMessages = 0;
            this.serverMetrics.networkStats.lastSecondStart = now;
        }
    }

    collectSystemMetrics() {
        this.serverMetrics.performanceStats.memoryUsage = process.memoryUsage();
        
        // Collect CPU usage (simplified)
        const usage = process.cpuUsage();
        this.serverMetrics.performanceStats.cpuUsage = 
            (usage.user + usage.system) / 1000000; // Convert to seconds
    }

    broadcastMetricsToClients() {
        const metricsData = {
            type: "serverMetrics",
            data: {
                tickRate: GAME_CONFIG.TICK_RATE,
                actualTickRate: this.calculateActualTickRate(),
                connectedClients: this.clients.size,
                totalMessages: this.serverMetrics.totalMessages,
                messagesPerSecond: this.serverMetrics.networkStats.messagesPerSecond,
                avgTickTime: Math.round(this.serverMetrics.performanceStats.avgTickTime * 100) / 100,
                memoryUsageMB: Math.round(this.serverMetrics.performanceStats.memoryUsage.heapUsed / 1024 / 1024),
                totalCorrections: this.serverMetrics.totalCorrections,
                uptime: Date.now() - this.serverMetrics.startTime
            }
        };

        this.broadcast(metricsData);
    }

    calculateActualTickRate() {
        if (this.serverMetrics.performanceStats.tickTimeSamples.length === 0) return 0;
        
        const avgTickTime = this.serverMetrics.performanceStats.avgTickTime;
        return avgTickTime > 0 ? Math.round(1000 / avgTickTime) : 0;
    }

    updateWorld(deltaTime) {
        this.sequenceNumber++;

        this.storeStateSnapshot();

        for (const [robotId, robot] of this.robots) {
            this.updateRobotPhysics(robot, deltaTime);
        }
    }

    storeStateSnapshot() {
        const snapshot = {
            sequence: this.sequenceNumber,
            timestamp: Date.now(),
            robots: new Map(),
        };

        for (const [robotId, robot] of this.robots) {
            snapshot.robots.set(robotId, {
                position: { ...robot.position },
                velocity: { ...robot.velocity },
                rotation: robot.rotation,
                inputs: { ...robot.inputs },
            });
        }

        this.stateHistory.set(this.sequenceNumber, snapshot);

        if (this.stateHistory.size > GAME_CONFIG.PREDICTION_BUFFER_SIZE) {
            const oldestKey = Math.min(...this.stateHistory.keys());
            this.stateHistory.delete(oldestKey);
        }
    }

    updateRobotPhysics(robot, deltaTime) {
        const inputs = robot.inputs || {};

        const targetVelocity = { x: 0, y: 0 };

        if (inputs.w) targetVelocity.y -= GAME_CONFIG.ROBOT_SPEED;
        if (inputs.s) targetVelocity.y += GAME_CONFIG.ROBOT_SPEED;
        if (inputs.a) targetVelocity.x -= GAME_CONFIG.ROBOT_SPEED;
        if (inputs.d) targetVelocity.x += GAME_CONFIG.ROBOT_SPEED;

        const smoothing = 0.15;
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

    sendStateUpdates() {
        const gameState = {
            sequence: this.sequenceNumber,
            timestamp: Date.now(),
            robots: Array.from(this.robots.values()).map((robot) => ({
                ...robot,
                reconciliation: robot.reconciliation || {},
            })),
        };

        this.broadcast({
            type: "gameState",
            data: gameState,
        });
    }

    handleClientConnection(ws, req) {
        const clientId = uuidv4();
        const robotId = uuidv4();

        const client = {
            id: clientId,
            ws: ws,
            robotId: robotId,
            latency: 0,
            lastPing: Date.now(),
            inputSequence: 0,
            lastProcessedInput: 0,
        };

        this.clients.set(clientId, client);
        this.inputHistory.set(clientId, new Map());

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
            inputs: {},
            lastInputTime: Date.now(),
            reconciliation: {
                corrections: 0,
                totalDivergence: 0,
                lastCorrectionTime: 0,
            },
        };

        this.robots.set(robotId, robot);

        console.log(
            `Client ${clientId} connected with robot ${robotId} (count: ${this.clients.size})`
        );

        this.sendToClient(clientId, {
            type: "init",
            data: {
                clientId: clientId,
                robotId: robotId,
                worldSize: GAME_CONFIG.WORLD_SIZE,
                tickRate: GAME_CONFIG.TICK_RATE,
                robots: Array.from(this.robots.values()),
                predictionConfig: {
                    enabled: true,
                    bufferSize: GAME_CONFIG.PREDICTION_BUFFER_SIZE,
                    reconciliationThreshold:
                        GAME_CONFIG.RECONCILIATION_THRESHOLD,
                },
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
            console.error(`WebSocket error on ${clientId}:`, error);
            this.handleClientDisconnection(clientId);
        });

        this.sendPing(clientId);
    }

    handleClientMessage(clientId, data) {
        const client = this.clients.get(clientId);
        if (!client) return;

        // Track message metrics
        this.serverMetrics.totalMessages++;
        this.serverMetrics.networkStats.lastSecondMessages++;
        this.serverMetrics.networkStats.bytesReceived += data.length;

        try {
            const message = JSON.parse(data.toString());

            switch (message.type) {
                case "input":
                    this.handleInputMessage(clientId, message.data);
                    break;
                case "pong":
                    this.handlePongMessage(clientId, message.data);
                    break;
                case "predictionState":
                    this.handlePredictionState(clientId, message.data);
                    break;
                default:
                    console.warn(`Unknown message type: ${message.type}`);
            }
        } catch (error) {
            console.error(`Parse error from client ${clientId}:`, error);
        }
    }

    handleInputMessage(clientId, inputData) {
        const client = this.clients.get(clientId);
        if (!client) return;

        const robot = this.robots.get(client.robotId);
        if (!robot) return;

        // Track input metrics
        this.serverMetrics.totalInputs++;
        if (!this.serverMetrics.inputRates.has(clientId)) {
            this.serverMetrics.inputRates.set(clientId, { count: 0, lastReset: Date.now() });
        }
        
        const inputRate = this.serverMetrics.inputRates.get(clientId);
        inputRate.count++;

        const inputSequence = inputData.sequence || 0;
        const clientInputHistory = this.inputHistory.get(clientId);

        clientInputHistory.set(inputSequence, {
            inputs: { ...inputData.inputs },
            timestamp: inputData.timestamp || Date.now(),
            processed: false,
        });

        if (clientInputHistory.size > GAME_CONFIG.PREDICTION_BUFFER_SIZE) {
            const oldestKey = Math.min(...clientInputHistory.keys());
            clientInputHistory.delete(oldestKey);
        }

        robot.inputs = { ...inputData.inputs };
        robot.lastInputTime = Date.now();
        client.lastProcessedInput = inputSequence;

        this.sendToClient(clientId, {
            type: "inputAck",
            data: {
                sequence: inputSequence,
                serverSequence: this.sequenceNumber,
                timestamp: Date.now(),
                robotState: {
                    position: { ...robot.position },
                    velocity: { ...robot.velocity },
                    rotation: robot.rotation,
                },
            },
        });
    }

    handlePredictionState(clientId, predictionData) {
        const client = this.clients.get(clientId);
        if (!client) return;

        const robot = this.robots.get(client.robotId);
        if (!robot) return;

        const divergence = this.calculateDivergence(
            predictionData.position,
            robot.position
        );

        robot.reconciliation.totalDivergence += divergence;

        if (divergence > GAME_CONFIG.RECONCILIATION_THRESHOLD) {
            robot.reconciliation.corrections++;
            robot.reconciliation.lastCorrectionTime = Date.now();

            // Track server-wide correction metrics
            this.serverMetrics.totalCorrections++;
            this.serverMetrics.totalDivergence += divergence;
            
            this.serverMetrics.correctionHistory.push({
                timestamp: Date.now(),
                clientId: clientId,
                divergence: divergence,
                position: { ...predictionData.position },
                serverPosition: { ...robot.position }
            });

            // Keep only recent correction history
            if (this.serverMetrics.correctionHistory.length > 100) {
                this.serverMetrics.correctionHistory.shift();
            }

            this.sendToClient(clientId, {
                type: "stateCorrection",
                data: {
                    sequence: this.sequenceNumber,
                    timestamp: Date.now(),
                    robotState: {
                        position: { ...robot.position },
                        velocity: { ...robot.velocity },
                        rotation: robot.rotation,
                    },
                    divergence: divergence,
                    correctionType:
                        divergence > GAME_CONFIG.RECONCILIATION_THRESHOLD * 2
                            ? "snap"
                            : "smooth",
                },
            });
        }
    }

    calculateDivergence(clientPos, serverPos) {
        const dx = clientPos.x - serverPos.x;
        const dy = clientPos.y - serverPos.y;
        return Math.sqrt(dx * dx + dy * dy);
    }

    handlePongMessage(clientId, data) {
        const client = this.clients.get(clientId);
        if (!client) return;

        const now = Date.now();
        const latency = now - data.timestamp;
        client.latency = latency;
        client.lastPing = now;

        // Track latency metrics
        this.serverMetrics.clientLatencies.set(clientId, {
            current: latency,
            history: (this.serverMetrics.clientLatencies.get(clientId)?.history || []).slice(-20).concat(latency)
        });
    }

    handleClientDisconnection(clientId) {
        const client = this.clients.get(clientId);
        if (!client) return;

        console.log(`Client ${clientId} disconnected`);

        if (client.robotId) {
            this.robots.delete(client.robotId);
            this.broadcastToOthers(clientId, {
                type: "robotLeft",
                data: { robotId: client.robotId },
            });
        }

        this.clients.delete(clientId);
        this.inputHistory.delete(clientId);

        console.log(`Clients remaining: ${this.clients.size}`);
    }

    sendToClient(clientId, message) {
        const client = this.clients.get(clientId);
        if (!client || client.ws.readyState !== 1) return;

        try {
            const messageStr = JSON.stringify(message);
            this.serverMetrics.networkStats.bytesSent += messageStr.length;
            client.ws.send(messageStr);
        } catch (error) {
            console.error(`Message send error to ${clientId}:`, error);
        }
    }

    broadcast(message) {
        const messageStr = JSON.stringify(message);
        this.serverMetrics.networkStats.bytesSent += messageStr.length * this.clients.size;

        for (const [clientId, client] of this.clients) {
            if (client.ws.readyState === 1) {
                try {
                    client.ws.send(messageStr);
                } catch (error) {
                    console.error(`Broadcast error to ${clientId}:`, error);
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
                    console.error(`Broadcast error to ${clientId}:`, error);
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
            if (now - client.lastPing > 1000) {
                this.sendPing(clientId);
            }
        }
    }

    getStats() {
        const clients = Array.from(this.clients.values());
        const robots = Array.from(this.robots.values());

        return {
            connectedClients: this.clients.size,
            activeRobots: this.robots.size,
            averageLatency:
                clients.length > 0
                    ? clients.reduce((sum, client) => sum + client.latency, 0) /
                      clients.length
                    : 0,
            totalCorrections: robots.reduce(
                (sum, robot) => sum + robot.reconciliation.corrections,
                0
            ),
            averageDivergence:
                robots.length > 0
                    ? robots.reduce(
                          (sum, robot) =>
                              sum + robot.reconciliation.totalDivergence,
                          0
                      ) / robots.length
                    : 0,
            sequenceNumber: this.sequenceNumber,
            tickCount: this.tickCount,
        };
    }

    getDetailedStats() {
        const clients = Array.from(this.clients.values());
        const robots = Array.from(this.robots.values());
        const now = Date.now();
        const uptime = now - this.serverMetrics.startTime;

        // Calculate input rates
        const inputRates = new Map();
        for (const [clientId, rateData] of this.serverMetrics.inputRates) {
            const timeSince = now - rateData.lastReset;
            const rate = timeSince > 0 ? (rateData.count / timeSince) * 1000 : 0;
            inputRates.set(clientId, Math.round(rate * 10) / 10);
        }

        // Calculate average latencies with history
        const latencyStats = new Map();
        for (const [clientId, latencyData] of this.serverMetrics.clientLatencies) {
            const avg = latencyData.history.length > 0 
                ? latencyData.history.reduce((a, b) => a + b, 0) / latencyData.history.length 
                : 0;
            const min = latencyData.history.length > 0 ? Math.min(...latencyData.history) : 0;
            const max = latencyData.history.length > 0 ? Math.max(...latencyData.history) : 0;
            
            latencyStats.set(clientId, {
                current: latencyData.current,
                average: Math.round(avg * 10) / 10,
                min: min,
                max: max
            });
        }

        return {
            // Basic stats
            connectedClients: this.clients.size,
            activeRobots: this.robots.size,
            uptime: uptime,
            
            // Network stats
            totalMessages: this.serverMetrics.totalMessages,
            totalInputs: this.serverMetrics.totalInputs,
            messagesPerSecond: this.serverMetrics.networkStats.messagesPerSecond,
            bytesReceived: this.serverMetrics.networkStats.bytesReceived,
            bytesSent: this.serverMetrics.networkStats.bytesSent,
            networkThroughputKBps: {
                received: Math.round((this.serverMetrics.networkStats.bytesReceived / 1024) / (uptime / 1000) * 10) / 10,
                sent: Math.round((this.serverMetrics.networkStats.bytesSent / 1024) / (uptime / 1000) * 10) / 10
            },
            
            // Performance stats
            tickRate: {
                target: GAME_CONFIG.TICK_RATE,
                actual: this.calculateActualTickRate(),
                efficiency: Math.round((this.calculateActualTickRate() / GAME_CONFIG.TICK_RATE) * 100)
            },
            tickTiming: {
                average: Math.round(this.serverMetrics.performanceStats.avgTickTime * 100) / 100,
                maximum: this.serverMetrics.performanceStats.maxTickTime,
                samples: this.serverMetrics.performanceStats.tickTimeSamples.length
            },
            memory: {
                heapUsedMB: Math.round(this.serverMetrics.performanceStats.memoryUsage.heapUsed / 1024 / 1024),
                heapTotalMB: Math.round(this.serverMetrics.performanceStats.memoryUsage.heapTotal / 1024 / 1024),
                externalMB: Math.round(this.serverMetrics.performanceStats.memoryUsage.external / 1024 / 1024)
            },
            
            // Game stats
            prediction: {
                totalCorrections: this.serverMetrics.totalCorrections,
                avgDivergence: this.serverMetrics.totalCorrections > 0 
                    ? Math.round((this.serverMetrics.totalDivergence / this.serverMetrics.totalCorrections) * 10) / 10 
                    : 0,
                recentCorrections: this.serverMetrics.correctionHistory.filter(c => now - c.timestamp < 10000).length
            },
            
            // Client-specific stats
            clientStats: Array.from(this.clients.entries()).map(([clientId, client]) => ({
                id: clientId,
                robotId: client.robotId,
                latency: latencyStats.get(clientId) || { current: 0, average: 0, min: 0, max: 0 },
                inputRate: inputRates.get(clientId) || 0,
                lastProcessedInput: client.lastProcessedInput,
                connected: client.ws.readyState === 1
            })),
            
            // Recent activity
            recentActivity: {
                correctionsLast10s: this.serverMetrics.correctionHistory.filter(c => now - c.timestamp < 10000).length,
                inputsPerSecond: this.serverMetrics.totalInputs > 0 ? Math.round((this.serverMetrics.totalInputs / (uptime / 1000)) * 10) / 10 : 0
            }
        };
    }
}

const server = new GameServer();

// Graceful shutdown
process.on("SIGINT", () => {
    console.log("\nShutting down server...");
    console.log("Final stats:", server.getStats());
    process.exit(0);
});
