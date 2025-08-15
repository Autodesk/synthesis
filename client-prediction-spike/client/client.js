class PredictionRobotClient {
    constructor() {
        this.ws = null;
        this.clientId = null;
        this.robotId = null;
        this.connected = false;

        this.robots = new Map();
        this.worldSize = { width: 1000, height: 1000 };
        this.robotSize = { width: 50, height: 50 };
        this.robotSpeed = 200;

        this.inputs = {
            w: false,
            s: false,
            a: false,
            d: false,
        };
        this.inputSequence = 0;
        this.inputBuffer = new Map();

        this.predictionEnabled = true;
        this.clientRobotState = null;
        this.serverRobotState = null;
        this.stateHistory = new Map();

        this.correctionCount = 0;
        this.totalDivergence = 0;
        this.lastCorrectionTime = 0;
        this.inputLagMeasurements = [];

        // Enhanced client metrics
        this.clientMetrics = {
            startTime: Date.now(),
            totalFrames: 0,
            frameTimes: [],
            inputsSent: 0,
            messagesReceived: 0,
            bytesReceived: 0,
            bytesSent: 0,
            connectionTime: 0,
            averageFPS: 0,
            predictionAccuracy: {
                samples: [],
                averageError: 0
            },
            networkStats: {
                packetsLost: 0,
                roundTripTimes: [],
                jitter: 0
            },
            serverMetrics: null
        };

        this.canvas = document.getElementById("gameCanvas");
        this.ctx = this.canvas.getContext("2d");
        this.lastRenderTime = 0;

        this.statusEl = document.getElementById("status");
        this.playerCountEl = document.getElementById("playerCount");
        this.latencyEl = document.getElementById("latency");
        this.serverTickEl = document.getElementById("serverTick");
        this.predictionEnabledEl = document.getElementById("predictionEnabled");
        this.correctionCountEl = document.getElementById("correctionCount");
        this.avgDivergenceEl = document.getElementById("avgDivergence");
        this.inputLagEl = document.getElementById("inputLag");
        this.inputSequenceEl = document.getElementById("inputSequence");
        this.lastCorrectionEl = document.getElementById("lastCorrection");

        // Additional metric elements
        this.fpsEl = document.getElementById("fps");
        this.serverTickRateEl = document.getElementById("serverTickRate");
        this.networkThroughputEl = document.getElementById("networkThroughput");
        this.serverMemoryEl = document.getElementById("serverMemory");
        this.totalMessagesEl = document.getElementById("totalMessages");
        this.predictionAccuracyEl = document.getElementById("predictionAccuracy");
        this.uptimeEl = document.getElementById("uptime");
        this.jitterEl = document.getElementById("jitter");

        // Panel elements for toggling
        this.infoPanel = document.getElementById("info");
        this.predictionPanel = document.getElementById("predictionStats");
        this.serverPanel = document.getElementById("serverStats");
        this.controlsPanel = document.getElementById("controls");

        this.metricsVisible = true;

        this.setupCanvas();
        this.setupInputHandlers();
        this.connect();
        this.startRenderLoop();
        this.startMetricsCollection();
    }

    connect() {
        const protocol = location.protocol === "https:" ? "wss:" : "ws:";
        const wsUrl = `${protocol}//${location.host}`;

        console.log(`Connecting to ${wsUrl}...`);

        try {
            this.ws = new WebSocket(wsUrl);

            this.ws.onopen = () => {
                this.connected = true;
                this.clientMetrics.connectionTime = Date.now();
                this.statusEl.textContent = "Connected";
                console.log("Connected to prediction server");
            };

            this.ws.onmessage = (event) => {
                this.clientMetrics.messagesReceived++;
                this.clientMetrics.bytesReceived += event.data.length;
                this.handleServerMessage(event.data);
            };

            this.ws.onclose = () => {
                this.connected = false;
                this.statusEl.textContent = "Disconnected";
                console.log("Disconnected from server");

                setTimeout(() => {
                    if (!this.connected) {
                        console.log("Attempting to reconnect...");
                        this.connect();
                    }
                }, 3000);
            };

            this.ws.onerror = (error) => {
                console.error("WebSocket error:", error);
                this.statusEl.textContent = "Error";
            };
        } catch (error) {
            console.error("Failed to connect:", error);
            this.statusEl.textContent = "Failed";
        }
    }

    handleServerMessage(data) {
        try {
            const message = JSON.parse(data);

            switch (message.type) {
                case "init":
                    this.handleInit(message.data);
                    break;
                case "gameState":
                    this.handleGameState(message.data);
                    break;
                case "robotJoined":
                    this.handleRobotJoined(message.data);
                    break;
                case "robotLeft":
                    this.handleRobotLeft(message.data);
                    break;
                case "ping":
                    this.handlePing(message.data);
                    break;
                case "inputAck":
                    this.handleInputAck(message.data);
                    break;
                case "stateCorrection":
                    this.handleStateCorrection(message.data);
                    break;
                case "serverMetrics":
                    this.handleServerMetrics(message.data);
                    break;
                default:
                    console.warn("Unknown message type:", message.type);
            }
        } catch (error) {
            console.error("Failed to parse server message:", error);
        }
    }

    handleInit(data) {
        this.clientId = data.clientId;
        this.robotId = data.robotId;
        this.worldSize = data.worldSize;

        data.robots.forEach((robot) => {
            this.robots.set(robot.id, { ...robot });

            if (robot.id === this.robotId) {
                this.clientRobotState = {
                    position: { ...robot.position },
                    velocity: { ...robot.velocity },
                    rotation: robot.rotation,
                    lastUpdateTime: Date.now(),
                };
                this.serverRobotState = { ...this.clientRobotState };
            }
        });

        this.playerCountEl.textContent = data.robots.length;
        console.log(`Initialized with ${data.robots.length} robots`);
    }

    handleGameState(data) {
        this.serverTickEl.textContent = data.sequence;

        data.robots.forEach((robotData) => {
            const robot = this.robots.get(robotData.id);
            if (robot) {
                robot.position = { ...robotData.position };
                robot.velocity = { ...robotData.velocity };
                robot.rotation = robotData.rotation;
                robot.reconciliation = robotData.reconciliation;

                if (robotData.id === this.robotId) {
                    this.serverRobotState = {
                        position: { ...robotData.position },
                        velocity: { ...robotData.velocity },
                        rotation: robotData.rotation,
                        lastUpdateTime: Date.now(),
                    };

                    if (this.predictionEnabled && this.clientRobotState) {
                        this.sendPredictionState();
                    }
                }
            }
        });

        this.playerCountEl.textContent = data.robots.length;
    }

    handleRobotJoined(data) {
        this.robots.set(data.id, { ...data });
        this.playerCountEl.textContent = this.robots.size;
        console.log(`Robot ${data.id} joined`);
    }

    handleRobotLeft(data) {
        this.robots.delete(data.robotId);
        this.playerCountEl.textContent = this.robots.size;
        console.log(`Robot ${data.robotId} left`);
    }

    handlePing(data) {
        const timestamp = Date.now();
        this.sendMessage({
            type: "pong",
            data: { timestamp: data.timestamp },
        });

        // Track round trip time for jitter calculation
        const rtt = timestamp - data.timestamp;
        this.clientMetrics.networkStats.roundTripTimes.push(rtt);
        if (this.clientMetrics.networkStats.roundTripTimes.length > 10) {
            this.clientMetrics.networkStats.roundTripTimes.shift();
        }

        // Calculate jitter (variation in latency)
        if (this.clientMetrics.networkStats.roundTripTimes.length > 1) {
            const rtts = this.clientMetrics.networkStats.roundTripTimes;
            const avg = rtts.reduce((a, b) => a + b, 0) / rtts.length;
            const variance = rtts.reduce((sum, rtt) => sum + Math.pow(rtt - avg, 2), 0) / rtts.length;
            this.clientMetrics.networkStats.jitter = Math.sqrt(variance);
        }
    }

    handleServerMetrics(data) {
        this.clientMetrics.serverMetrics = data;
        this.updateServerMetricsDisplay();
    }

    startMetricsCollection() {
        // Update client metrics every second
        setInterval(() => {
            this.updateClientMetrics();
        }, 1000);
    }

    updateClientMetrics() {
        // Calculate FPS
        if (this.clientMetrics.frameTimes.length > 0) {
            const validFrameTimes = this.clientMetrics.frameTimes.filter(t => t > 0);
            if (validFrameTimes.length > 0) {
                const avgFrameTime = validFrameTimes.reduce((a, b) => a + b, 0) / validFrameTimes.length;
                this.clientMetrics.averageFPS = Math.round(1000 / avgFrameTime);
            }
        }

        // Calculate prediction accuracy
        if (this.clientMetrics.predictionAccuracy.samples.length > 0) {
            this.clientMetrics.predictionAccuracy.averageError = 
                this.clientMetrics.predictionAccuracy.samples.reduce((a, b) => a + b, 0) / 
                this.clientMetrics.predictionAccuracy.samples.length;
        }

        this.updateMetricsDisplay();
    }

    updateMetricsDisplay() {
        // Update existing elements that might exist
        if (this.fpsEl) this.fpsEl.textContent = this.clientMetrics.averageFPS || "0";
        if (this.totalMessagesEl) this.totalMessagesEl.textContent = this.clientMetrics.messagesReceived;
        if (this.predictionAccuracyEl) {
            this.predictionAccuracyEl.textContent = 
                Math.round(this.clientMetrics.predictionAccuracy.averageError * 10) / 10 + "px";
        }
        if (this.jitterEl) {
            this.jitterEl.textContent = Math.round(this.clientMetrics.networkStats.jitter * 10) / 10 + "ms";
        }
        if (this.uptimeEl) {
            const uptime = Date.now() - this.clientMetrics.startTime;
            const seconds = Math.floor(uptime / 1000) % 60;
            const minutes = Math.floor(uptime / 60000);
            this.uptimeEl.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;
        }
    }

    updateServerMetricsDisplay() {
        const serverMetrics = this.clientMetrics.serverMetrics;
        if (!serverMetrics) return;

        if (this.serverTickRateEl) {
            this.serverTickRateEl.textContent = `${serverMetrics.actualTickRate}/${serverMetrics.tickRate}`;
        }
        if (this.networkThroughputEl) {
            this.networkThroughputEl.textContent = `${serverMetrics.messagesPerSecond}/s`;
        }
        if (this.serverMemoryEl) {
            this.serverMemoryEl.textContent = `${serverMetrics.memoryUsageMB}MB`;
        }
    }

    handleInputAck(data) {
        this.inputBuffer.delete(data.sequence);

        const inputTime = this.stateHistory.get(data.sequence);
        if (inputTime) {
            const lag = Date.now() - inputTime.timestamp;
            this.inputLagMeasurements.push(lag);

            if (this.inputLagMeasurements.length > 10) {
                this.inputLagMeasurements.shift();
            }

            const avgLag =
                this.inputLagMeasurements.reduce((a, b) => a + b, 0) /
                this.inputLagMeasurements.length;
            this.inputLagEl.textContent = Math.round(avgLag);
        }
    }

    handleStateCorrection(data) {
        if (!this.predictionEnabled) return;

        this.correctionCount++;
        this.totalDivergence += data.divergence;
        this.lastCorrectionTime = Date.now();

        // Track prediction accuracy
        this.clientMetrics.predictionAccuracy.samples.push(data.divergence);
        if (this.clientMetrics.predictionAccuracy.samples.length > 50) {
            this.clientMetrics.predictionAccuracy.samples.shift();
        }

        this.showCorrectionIndicator(data.correctionType);

        if (data.correctionType === "snap") {
            this.clientRobotState = {
                position: { ...data.robotState.position },
                velocity: { ...data.robotState.velocity },
                rotation: data.robotState.rotation,
                lastUpdateTime: Date.now(),
            };
        } else {
            this.smoothStateCorrection(data.robotState);
        }

        this.updatePredictionMetrics();
    }

    showCorrectionIndicator(type) {
        this.lastCorrectionEl.textContent = type.toUpperCase();
        this.lastCorrectionEl.classList.add("correction-flash");

        setTimeout(() => {
            this.lastCorrectionEl.classList.remove("correction-flash");
        }, 500);
    }

    smoothStateCorrection(targetState) {
        const smoothingFactor = 0.3;

        this.clientRobotState.position.x =
            this.clientRobotState.position.x * (1 - smoothingFactor) +
            targetState.position.x * smoothingFactor;
        this.clientRobotState.position.y =
            this.clientRobotState.position.y * (1 - smoothingFactor) +
            targetState.position.y * smoothingFactor;
        this.clientRobotState.velocity.x =
            this.clientRobotState.velocity.x * (1 - smoothingFactor) +
            targetState.velocity.x * smoothingFactor;
        this.clientRobotState.velocity.y =
            this.clientRobotState.velocity.y * (1 - smoothingFactor) +
            targetState.velocity.y * smoothingFactor;
        this.clientRobotState.rotation = targetState.rotation;
    }

    sendPredictionState() {
        if (!this.clientRobotState) return;

        this.sendMessage({
            type: "predictionState",
            data: {
                position: { ...this.clientRobotState.position },
                velocity: { ...this.clientRobotState.velocity },
                rotation: this.clientRobotState.rotation,
                timestamp: Date.now(),
            },
        });
    }

    setupCanvas() {
        this.canvas.width = this.worldSize.width;
        this.canvas.height = this.worldSize.height;

        window.addEventListener("resize", () => {
            this.canvas.width = this.worldSize.width;
            this.canvas.height = this.worldSize.height;
        });
    }

    setupInputHandlers() {
        document.addEventListener("keydown", (e) => {
            this.handleKeyInput(e.key.toLowerCase(), true);
        });

        document.addEventListener("keyup", (e) => {
            this.handleKeyInput(e.key.toLowerCase(), false);
        });

        document.addEventListener("keypress", (e) => {
            if (["w", "a", "s", "d", "p", "r", "m"].includes(e.key.toLowerCase())) {
                e.preventDefault();
            }
        });
    }

    handleKeyInput(key, pressed) {
        if (pressed) {
            switch (key) {
                case "p":
                    this.togglePrediction();
                    return;
                case "r":
                    this.resetStats();
                    return;
                case "m":
                    this.toggleMetrics();
                    return;
            }
        }

        if (["w", "a", "s", "d"].includes(key)) {
            const oldInputs = { ...this.inputs };
            this.inputs[key] = pressed;

            if (JSON.stringify(oldInputs) !== JSON.stringify(this.inputs)) {
                this.sendInputs();
            }
        }
    }

    sendInputs() {
        if (!this.connected) return;

        this.inputSequence++;
        this.clientMetrics.inputsSent++;
        const timestamp = Date.now();

        this.inputBuffer.set(this.inputSequence, {
            inputs: { ...this.inputs },
            timestamp: timestamp,
        });

        this.stateHistory.set(this.inputSequence, {
            state: this.clientRobotState ? { ...this.clientRobotState } : null,
            timestamp: timestamp,
        });

        if (this.stateHistory.size > 60) {
            const oldestKey = Math.min(...this.stateHistory.keys());
            this.stateHistory.delete(oldestKey);
        }

        const message = {
            type: "input",
            data: {
                inputs: { ...this.inputs },
                sequence: this.inputSequence,
                timestamp: timestamp,
            },
        };

        this.sendMessage(message);
        this.inputSequenceEl.textContent = this.inputSequence;
    }

    applyClientPrediction() {
        if (!this.clientRobotState) return;

        const now = Date.now();
        const deltaTime = Math.min(
            (now - this.clientRobotState.lastUpdateTime) / 1000,
            1 / 60
        );

        const targetVelocity = { x: 0, y: 0 };

        if (this.inputs.w) targetVelocity.y -= this.robotSpeed;
        if (this.inputs.s) targetVelocity.y += this.robotSpeed;
        if (this.inputs.a) targetVelocity.x -= this.robotSpeed;
        if (this.inputs.d) targetVelocity.x += this.robotSpeed;

        const smoothing = 0.15;
        this.clientRobotState.velocity.x =
            this.clientRobotState.velocity.x * (1 - smoothing) +
            targetVelocity.x * smoothing;
        this.clientRobotState.velocity.y =
            this.clientRobotState.velocity.y * (1 - smoothing) +
            targetVelocity.y * smoothing;

        this.clientRobotState.position.x +=
            this.clientRobotState.velocity.x * deltaTime;
        this.clientRobotState.position.y +=
            this.clientRobotState.velocity.y * deltaTime;

        this.clientRobotState.position.x = Math.max(
            0,
            Math.min(
                this.worldSize.width - this.robotSize.width,
                this.clientRobotState.position.x
            )
        );
        this.clientRobotState.position.y = Math.max(
            0,
            Math.min(
                this.worldSize.height - this.robotSize.height,
                this.clientRobotState.position.y
            )
        );

        if (
            Math.abs(this.clientRobotState.velocity.x) > 10 ||
            Math.abs(this.clientRobotState.velocity.y) > 10
        ) {
            this.clientRobotState.rotation =
                (Math.atan2(
                    this.clientRobotState.velocity.y,
                    this.clientRobotState.velocity.x
                ) *
                    180) /
                Math.PI;
        }

        this.clientRobotState.lastUpdateTime = now;
    }

    togglePrediction() {
        this.predictionEnabled = !this.predictionEnabled;
        this.predictionEnabledEl.textContent = this.predictionEnabled
            ? "ON"
            : "OFF";
        this.predictionEnabledEl.className = this.predictionEnabled
            ? "stat-value prediction-enabled"
            : "stat-value prediction-disabled";

        console.log(
            `Prediction ${this.predictionEnabled ? "enabled" : "disabled"}`
        );
    }

    resetStats() {
        this.correctionCount = 0;
        this.totalDivergence = 0;
        this.inputLagMeasurements = [];
        this.lastCorrectionEl.textContent = "None";
        
        // Reset client metrics
        this.clientMetrics.totalFrames = 0;
        this.clientMetrics.frameTimes = [];
        this.clientMetrics.inputsSent = 0;
        this.clientMetrics.messagesReceived = 0;
        this.clientMetrics.predictionAccuracy.samples = [];
        this.clientMetrics.networkStats.roundTripTimes = [];
        this.clientMetrics.startTime = Date.now();
        
        this.updatePredictionMetrics();
        console.log("Stats reset");
    }

    toggleMetrics() {
        this.metricsVisible = !this.metricsVisible;
        const display = this.metricsVisible ? "block" : "none";
        
        if (this.infoPanel) this.infoPanel.style.display = display;
        if (this.predictionPanel) this.predictionPanel.style.display = display;
        if (this.serverPanel) this.serverPanel.style.display = display;
        if (this.controlsPanel) this.controlsPanel.style.display = display;
        
        console.log(`Metrics panels ${this.metricsVisible ? "shown" : "hidden"}`);
    }

    updatePredictionMetrics() {
        this.correctionCountEl.textContent = this.correctionCount;

        const avgDivergence =
            this.correctionCount > 0
                ? (this.totalDivergence / this.correctionCount).toFixed(1)
                : "0.0";
        this.avgDivergenceEl.textContent = avgDivergence;
    }

    sendMessage(message) {
        if (this.ws && this.ws.readyState === WebSocket.OPEN) {
            const messageStr = JSON.stringify(message);
            this.clientMetrics.bytesSent += messageStr.length;
            this.ws.send(messageStr);
        }
    }

    startRenderLoop() {
        const render = (timestamp) => {
            this.render(timestamp);
            requestAnimationFrame(render);
        };
        requestAnimationFrame(render);
    }

    render(timestamp) {
        // Track frame timing
        if (this.lastRenderTime > 0) {
            const frameTime = timestamp - this.lastRenderTime;
            this.clientMetrics.frameTimes.push(frameTime);
            if (this.clientMetrics.frameTimes.length > 60) {
                this.clientMetrics.frameTimes.shift();
            }
        }
        this.clientMetrics.totalFrames++;

        if (this.predictionEnabled && this.clientRobotState) {
            this.applyClientPrediction();
        }

        this.ctx.fillStyle = "#000";
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        this.drawGrid();

        for (const [robotId, robot] of this.robots) {
            if (robotId === this.robotId) {
                this.drawOwnRobot(robot);
            } else {
                this.drawOtherRobot(robot);
            }
        }

        this.drawDebugInfo();

        this.lastRenderTime = timestamp;
    }

    drawGrid() {
        this.ctx.strokeStyle = "#333";
        this.ctx.lineWidth = 1;

        for (let x = 0; x <= this.worldSize.width; x += 100) {
            this.ctx.beginPath();
            this.ctx.moveTo(x, 0);
            this.ctx.lineTo(x, this.worldSize.height);
            this.ctx.stroke();
        }

        for (let y = 0; y <= this.worldSize.height; y += 100) {
            this.ctx.beginPath();
            this.ctx.moveTo(0, y);
            this.ctx.lineTo(this.worldSize.width, y);
            this.ctx.stroke();
        }
    }

    drawOwnRobot(robot) {
        const state =
            this.predictionEnabled && this.clientRobotState
                ? this.clientRobotState
                : robot;

        if (this.predictionEnabled && this.serverRobotState) {
            this.drawRobotAt(
                this.serverRobotState.position,
                this.serverRobotState.rotation,
                "rgba(255, 255, 255, 0.3)",
                "Ghost",
                true
            );
        }

        this.drawRobotAt(state.position, state.rotation, "#ff4444", "YOU"),
            false;

        if (
            this.predictionEnabled &&
            Date.now() - this.lastCorrectionTime < 1000
        ) {
            this.drawCorrectionIndicator(state.position);
        }
    }

    drawOtherRobot(robot) {
        this.drawRobotAt(
            robot.position,
            robot.rotation,
            "#44ff44",
            "OTHER",
            false
        );
    }

    drawRobotAt(position, rotation, color, label, secondary) {
        this.ctx.save();
        this.ctx.translate(
            position.x + this.robotSize.width / 2,
            position.y + this.robotSize.height / 2
        );
        this.ctx.rotate((rotation * Math.PI) / 180);

        this.ctx.fillStyle = color;
        this.ctx.fillRect(
            -this.robotSize.width / 2,
            -this.robotSize.height / 2,
            this.robotSize.width,
            this.robotSize.height
        );

        this.ctx.fillStyle = "#fff";
        this.ctx.fillRect(this.robotSize.width / 2 - 5, -2, 10, 4);

        this.ctx.restore();

        this.ctx.font = "10px monospace";
        this.ctx.textAlign = "center";
        if (secondary) {
            this.ctx.fillStyle = "#aaa";
            this.ctx.fillText(
                label,
                position.x + this.robotSize.width / 2,
                position.y + 62
            );
        } else {
            this.ctx.fillStyle = "#fff";
            this.ctx.fillText(
                label,
                position.x + this.robotSize.width / 2,
                position.y - 5
            );
        }
    }

    drawCorrectionIndicator(position) {
        const radius = 30;
        const alpha = Math.max(
            0,
            1 - (Date.now() - this.lastCorrectionTime) / 1000
        );

        this.ctx.strokeStyle = `rgba(255, 255, 0, ${alpha})`;
        this.ctx.lineWidth = 3;
        this.ctx.beginPath();
        this.ctx.arc(
            position.x + this.robotSize.width / 2,
            position.y + this.robotSize.height / 2,
            radius,
            0,
            Math.PI * 2
        );
        this.ctx.stroke();
    }

    drawDebugInfo() {
        if (
            !this.predictionEnabled ||
            !this.clientRobotState ||
            !this.serverRobotState
        )
            return;

        this.ctx.strokeStyle = "rgba(255, 255, 0, 0.8)";
        this.ctx.lineWidth = 2;
        this.ctx.beginPath();
        this.ctx.moveTo(
            this.clientRobotState.position.x + this.robotSize.width / 2,
            this.clientRobotState.position.y + this.robotSize.height / 2
        );
        this.ctx.lineTo(
            this.serverRobotState.position.x + this.robotSize.width / 2,
            this.serverRobotState.position.y + this.robotSize.height / 2
        );
        this.ctx.stroke();
    }
}

document.addEventListener("DOMContentLoaded", () => {
    new PredictionRobotClient();
});
