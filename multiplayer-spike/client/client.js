class MultiplayerRobotClient {
    constructor() {
        this.ws = null;
        this.clientId = null;
        this.robotId = null;
        this.connected = false;

        this.robots = new Map();
        this.worldSize = { width: 1000, height: 1000 };
        this.inputs = {
            forward: false,
            backward: false,
            left: false,
            right: false,
        };

        this.canvas = document.getElementById("gameCanvas");
        this.ctx = this.canvas.getContext("2d");

        this.statusEl = document.getElementById("status");
        this.playerCountEl = document.getElementById("playerCount");
        this.latencyEl = document.getElementById("latency");

        this.setupCanvas();
        this.setupInputHandlers();
        this.connect();
        this.startRenderLoop();
    }

    connect() {
        const protocol = location.protocol === "https:" ? "wss:" : "ws:";
        const wsUrl = `${protocol}//${location.host}/game`;

        console.log(`Connecting to ${wsUrl}...`);

        try {
            this.ws = new WebSocket(wsUrl);

            this.ws.onopen = () => {
                this.connected = true;
                this.statusEl.textContent = "Connected";
                console.log("Connected to game server");
            };

            this.ws.onmessage = (event) => {
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
                case "worldState":
                    this.handleWorldState(message.data);
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
            this.robots.set(robot.id, robot);
        });

        this.playerCountEl.textContent = data.robots.length;
        console.log(`Initialized as robot ${this.robotId.substring(0, 8)}`);
    }

    handleWorldState(data) {
        data.robots.forEach((robotData) => {
            if (this.robots.has(robotData.id)) {
                const robot = this.robots.get(robotData.id);
                robot.position = robotData.position;
                robot.velocity = robotData.velocity;
                robot.rotation = robotData.rotation;
            }
        });
    }

    handleRobotJoined(data) {
        this.robots.set(data.id, data);
        this.playerCountEl.textContent = this.robots.size;
        console.log(`Robot ${data.id.substring(0, 8)} joined`);
    }

    handleRobotLeft(data) {
        this.robots.delete(data.robotId);
        this.playerCountEl.textContent = this.robots.size;
        console.log(`Robot ${data.robotId.substring(0, 8)} left`);
    }

    handlePing(data) {
        this.send({
            type: "pong",
            data: { timestamp: data.timestamp },
        });

        const latency = Date.now() - data.timestamp;
        this.latencyEl.textContent = latency;
    }

    setupInputHandlers() {
        const keys = {
            KeyW: "forward",
            KeyS: "backward",
            KeyA: "left",
            KeyD: "right",
        };

        document.addEventListener("keydown", (e) => {
            const action = keys[e.code];
            if (action && !this.inputs[action]) {
                this.inputs[action] = true;
                this.sendInputs();
            }
        });

        document.addEventListener("keyup", (e) => {
            const action = keys[e.code];
            if (action && this.inputs[action]) {
                this.inputs[action] = false;
                this.sendInputs();
            }
        });

        window.addEventListener("resize", () => {
            this.setupCanvas();
        });
    }

    sendInputs() {
        this.send({
            type: "input",
            data: { ...this.inputs },
        });
    }

    setupCanvas() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }

    startRenderLoop() {
        const render = () => {
            this.render();
            requestAnimationFrame(render);
        };
        requestAnimationFrame(render);
    }

    render() {
        this.ctx.fillStyle = "#000";
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

        const myRobot = this.robots.get(this.robotId);
        let offsetX = 0;
        let offsetY = 0;

        if (myRobot) {
            offsetX = this.canvas.width / 2 - myRobot.position.x;
            offsetY = this.canvas.height / 2 - myRobot.position.y;
        }

        this.ctx.strokeStyle = "#333";
        this.ctx.lineWidth = 2;
        this.ctx.strokeRect(
            offsetX,
            offsetY,
            this.worldSize.width,
            this.worldSize.height
        );

        for (const robot of this.robots.values()) {
            this.drawRobot(robot, offsetX, offsetY);
        }
    }

    drawRobot(robot, offsetX, offsetY) {
        const isOwnRobot = robot.id === this.robotId;
        const x = robot.position.x + offsetX;
        const y = robot.position.y + offsetY;

        this.ctx.save();
        this.ctx.translate(x + 25, y + 25);
        this.ctx.rotate((robot.rotation * Math.PI) / 180);

        this.ctx.fillStyle = isOwnRobot ? "#ff0000" : "#00ff00";
        this.ctx.fillRect(-25, -25, 50, 50);

        this.ctx.fillStyle = "#fff";
        this.ctx.beginPath();
        this.ctx.moveTo(15, 0);
        this.ctx.lineTo(-5, -8);
        this.ctx.lineTo(-5, 8);
        this.ctx.closePath();
        this.ctx.fill();

        this.ctx.restore();
    }

    send(message) {
        if (this.connected && this.ws.readyState === WebSocket.OPEN) {
            this.ws.send(JSON.stringify(message));
        }
    }
}

document.addEventListener("DOMContentLoaded", () => {
    window.gameClient = new MultiplayerRobotClient();
});
