import type { Metrics, Point, Robot } from "../types";
import PeerConnection from "./PeerConnection";

class RobotClient {
  peerConnection: PeerConnection;
  clientId: string;
  robotId: string | null = null;

  robots: Map<string, Robot> = new Map();
  worldSize = { width: 1000, height: 1000 };
  robotSize = { width: 50, height: 50 };
  robotSpeed = 200;

  inputs = { w: false, s: false, a: false, d: false };
  inputSequence = 0;
  inputBuffer: Map<number, any> = new Map();

  clientRobotState: Robot | null = null;
  stateHistory: Map<number, Robot> = new Map();

  correctionCount = 0;
  totalDivergence = 0;
  lastCorrectionTime = 0;
  inputLagMeasurements: number[] = [];

  clientMetrics = {
    startTime: Date.now(),
    totalFrames: 0,
    frameTimes: [] as number[],
    inputsSent: 0,
    messagesReceived: 0,
    bytesReceived: 0,
    bytesSent: 0,
    connectionTime: 0,
    averageFPS: 0,
    networkStats: { packetsLost: 0, roundTripTimes: [] as number[], jitter: 0 },
    serverMetrics: {} as Metrics,
  };

  canvas: HTMLCanvasElement;
  ctx: CanvasRenderingContext2D;
  lastRenderTime = 0;

  // HTML Elements
  statusEl: HTMLElement;
  playerCountEl: HTMLElement;
  latencyEl: HTMLElement;
  serverTickEl: HTMLElement;
  correctionCountEl: HTMLElement;
  avgDivergenceEl: HTMLElement;
  inputLagEl: HTMLElement;
  inputSequenceEl: HTMLElement;
  lastCorrectionEl: HTMLElement;
  fpsEl: HTMLElement;
  serverTickRateEl: HTMLElement;
  networkThroughputEl: HTMLElement;
  serverMemoryEl: HTMLElement;
  totalMessagesEl: HTMLElement;
  uptimeEl: HTMLElement;
  jitterEl: HTMLElement;
  infoPanel: HTMLElement;
  serverPanel: HTMLElement;
  controlsPanel: HTMLElement;
  metricsVisible = true;

  constructor() {
    this.clientId = this.generateClientId();
    this.peerConnection = new PeerConnection(this.clientId);

    this.canvas = document.getElementById("gameCanvas") as HTMLCanvasElement;
    this.ctx = this.canvas.getContext("2d")!;
    this.statusEl = document.getElementById("status")!;
    this.playerCountEl = document.getElementById("playerCount")!;
    this.latencyEl = document.getElementById("latency")!;
    this.serverTickEl = document.getElementById("serverTick")!;
    this.correctionCountEl = document.getElementById("correctionCount")!;
    this.avgDivergenceEl = document.getElementById("avgDivergence")!;
    this.inputLagEl = document.getElementById("inputLag")!;
    this.inputSequenceEl = document.getElementById("inputSequence")!;
    this.lastCorrectionEl = document.getElementById("lastCorrection")!;
    this.fpsEl = document.getElementById("fps")!;
    this.serverTickRateEl = document.getElementById("serverTickRate")!;
    this.networkThroughputEl = document.getElementById("networkThroughput")!;
    this.serverMemoryEl = document.getElementById("serverMemory")!;
    this.totalMessagesEl = document.getElementById("totalMessages")!;
    this.uptimeEl = document.getElementById("uptime")!;
    this.jitterEl = document.getElementById("jitter")!;
    this.infoPanel = document.getElementById("info")!;
    this.serverPanel = document.getElementById("serverStats")!;
    this.controlsPanel = document.getElementById("controls")!;

    this.setupCanvas();
    this.setupInputHandlers();
    this.startRenderLoop();
    this.startMetricsCollection();

    // Create or connect to a peer
    this.peerConnection.peer.on("open", (id) => {
      console.log(`My peer ID is: ${id}`);
      // Connect to another peer (for example using a fixed peer ID for simplicity)
      this.peerConnection.connectToPeer("some-peer-id"); // Replace 'some-peer-id' with the actual peer ID
    });

    this.peerConnection.peer.on("connection", (conn) => {
      console.log("Connected to another peer");
      this.peerConnection.connection = conn;
      this.peerConnection.setupConnectionHandlers();
    });
  }

  generateClientId(): string {
    return `client-${Math.random().toString(36).substring(2, 9)}`;
  }

  handleServerMessage(data: any) {
    try {
      switch (data.type) {
        case "init":
          this.handleInit(data.data);
          break;
        case "gameState":
          this.handleGameState(data.data);
          break;
        case "robotJoined":
          this.handleRobotJoined(data.data);
          break;
        case "robotLeft":
          this.handleRobotLeft(data.data);
          break;
        case "ping":
          this.handlePing(data.data);
          break;
        case "inputAck":
          this.handleInputAck(data.data);
          break;
        case "serverMetrics":
          this.handleServerMetrics(data.data);
          break;
        default:
          console.warn("Unknown message type:", data.type);
      }
    } catch (error) {
      console.error("Failed to parse server message:", error);
    }
  }

  handleInit(data: any) {
    this.clientId = data.clientId;
    this.robotId = data.robotId;
    this.worldSize = data.worldSize;

    data.robots.forEach((robot: Robot) => {
      this.robots.set(robot.id, { ...robot });

      if (robot.id === this.robotId) {
        this.clientRobotState = {
          position: robot.position,
          velocity: robot.velocity,
          rotation: robot.rotation,
          lastUpdateTime: Date.now(),
        };
        this.serverRobotState = this.clientRobotState;
      }
    });

    this.playerCountEl.textContent = data.robots.length;
    console.log(`Initialized with ${data.robots.length} robots`);
  }

  handleGameState(data: any) {
    this.serverTickEl.textContent = data.sequence;

    data.robots.forEach((robotData: any) => {
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
        }
      }
    });

    this.playerCountEl.textContent = data.robots.length;
  }

  handleRobotJoined(data: any) {
    this.robots.set(data.id, { ...data });
    this.playerCountEl.textContent = this.robots.size.toString();
    console.log(`Robot ${data.id} joined`);
  }

  handleRobotLeft(data: any) {
    this.robots.delete(data.robotId);
    this.playerCountEl.textContent = this.robots.size.toString();
    console.log(`Robot ${data.robotId} left`);
  }

  handlePing(data: any) {
    const timestamp = Date.now();
    this.peerConnection.send({
      type: "pong",
      data: { timestamp: data.timestamp },
    });

    const rtt = timestamp - data.timestamp;
    this.clientMetrics.networkStats.roundTripTimes.push(rtt);
    if (this.clientMetrics.networkStats.roundTripTimes.length > 10) {
      this.clientMetrics.networkStats.roundTripTimes.shift();
    }

    if (this.clientMetrics.networkStats.roundTripTimes.length > 1) {
      const rtts = this.clientMetrics.networkStats.roundTripTimes;
      const avg = rtts.reduce((a, b) => a + b, 0) / rtts.length;
      const variance =
        rtts.reduce((sum, rtt) => sum + Math.pow(rtt - avg, 2), 0) /
        rtts.length;
      this.clientMetrics.networkStats.jitter = Math.sqrt(variance);
    }
  }

  handleServerMetrics(data: any) {
    this.clientMetrics.serverMetrics = data;
    this.updateServerMetricsDisplay();
  }

  startMetricsCollection() {
    setInterval(() => {
      this.updateClientMetrics();
    }, 1000);
  }

  updateClientMetrics() {
    if (this.clientMetrics.frameTimes.length > 0) {
      const validFrameTimes = this.clientMetrics.frameTimes.filter(
        (t) => t > 0,
      );
      if (validFrameTimes.length > 0) {
        const avgFrameTime =
          validFrameTimes.reduce((a, b) => a + b, 0) / validFrameTimes.length;
        this.clientMetrics.averageFPS = Math.round(1000 / avgFrameTime);
      }
    }

    this.updateMetricsDisplay();
  }

  updateMetricsDisplay() {
    if (this.fpsEl)
      this.fpsEl.textContent = this.clientMetrics.averageFPS.toString() || "0";
    if (this.totalMessagesEl)
      this.totalMessagesEl.textContent =
        this.clientMetrics.messagesReceived.toString();
    if (this.jitterEl) {
      this.jitterEl.textContent =
        Math.round(this.clientMetrics.networkStats.jitter * 10) / 10 + "ms";
    }
    if (this.uptimeEl) {
      const uptime = Date.now() - this.clientMetrics.startTime;
      const seconds = Math.floor(uptime / 1000) % 60;
      const minutes = Math.floor(uptime / 60000);
      this.uptimeEl.textContent = `${minutes}:${seconds.toString().padStart(2, "0")}`;
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

  handleInputAck(data: any) {
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
      this.inputLagEl.textContent = Math.round(avgLag).toString();
    }
  }

  showCorrectionIndicator(type: string) {
    this.lastCorrectionEl.textContent = type.toUpperCase();
    this.lastCorrectionEl.classList.add("correction-flash");

    setTimeout(() => {
      this.lastCorrectionEl.classList.remove("correction-flash");
    }, 500);
  }

  sendPredictionState() {
    if (!this.clientRobotState) return;

    this.peerConnection.send({
      type: "predictionState",
      data: {
        position: this.clientRobotState.position,
        velocity: this.clientRobotState.velocity,
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

  handleKeyInput(key: string, pressed: boolean) {
    if (pressed) {
      switch (key) {
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

    this.peerConnection.send(message);
    this.inputSequenceEl.textContent = this.inputSequence.toString();
  }

  applyClientPrediction() {
    const now = Date.now();
    const deltaTime = Math.min(
      (now - this.clientRobotState.lastUpdateTime) / 1000,
      1 / 60,
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
        this.clientRobotState.position.x,
      ),
    );
    this.clientRobotState.position.y = Math.max(
      0,
      Math.min(
        this.worldSize.height - this.robotSize.height,
        this.clientRobotState.position.y,
      ),
    );

    if (
      Math.abs(this.clientRobotState.velocity.x) > 10 ||
      Math.abs(this.clientRobotState.velocity.y) > 10
    ) {
      this.clientRobotState.rotation =
        (Math.atan2(
          this.clientRobotState.velocity.y,
          this.clientRobotState.velocity.x,
        ) *
          180) /
        Math.PI;
    }

    this.clientRobotState.lastUpdateTime = now;
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
    this.clientMetrics.networkStats.roundTripTimes = [];
    this.clientMetrics.startTime = Date.now();

    console.log("Stats reset");
  }

  toggleMetrics() {
    this.metricsVisible = !this.metricsVisible;
    const display = this.metricsVisible ? "block" : "none";

    if (this.infoPanel) this.infoPanel.style.display = display;
    if (this.serverPanel) this.serverPanel.style.display = display;
    if (this.controlsPanel) this.controlsPanel.style.display = display;

    console.log(`Metrics panels ${this.metricsVisible ? "shown" : "hidden"}`);
  }

  sendMessage(message: any) {
    if (this.peerConnection && this.peerConnection.connected) {
      const messageStr = JSON.stringify(message);
      this.clientMetrics.bytesSent += messageStr.length;
      this.peerConnection.send(messageStr);
    }
  }

  startRenderLoop() {
    const render = (timestamp: number) => {
      this.render(timestamp);
      requestAnimationFrame(render);
    };
    requestAnimationFrame(render);
  }
  render(timestamp: number) {
    // Track frame timing
    if (this.lastRenderTime > 0) {
      const frameTime = timestamp - this.lastRenderTime;
      this.clientMetrics.frameTimes.push(frameTime);

      if (this.clientMetrics.frameTimes.length > 60) {
        this.clientMetrics.frameTimes.shift();
      }
    }
    this.clientMetrics.totalFrames++;

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

  drawOwnRobot(robot: Robot) {
    this.drawRobotAt(robot.position, robot.rotation, "#ff4444", "YOU", false);
  }

  drawOtherRobot(robot: Robot) {
    this.drawRobotAt(robot.position, robot.rotation, "#44ff44", "OTHER", false);
  }

  drawRobotAt(
    position: Point,
    rotation: number,
    color: string,
    label: string,
    secondary: boolean,
  ) {
    this.ctx.save();
    this.ctx.translate(
      position.x + this.robotSize.width / 2,
      position.y + this.robotSize.height / 2,
    );
    this.ctx.rotate((rotation * Math.PI) / 180);

    this.ctx.fillStyle = color;
    this.ctx.fillRect(
      -this.robotSize.width / 2,
      -this.robotSize.height / 2,
      this.robotSize.width,
      this.robotSize.height,
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
        position.y + 62,
      );
    } else {
      this.ctx.fillStyle = "#fff";
      this.ctx.fillText(
        label,
        position.x + this.robotSize.width / 2,
        position.y - 5,
      );
    }
  }

  drawCorrectionIndicator(position: Point) {
    const radius = 30;
    const alpha = Math.max(
      0,
      1 - (Date.now() - this.lastCorrectionTime) / 1000,
    );

    this.ctx.strokeStyle = `rgba(255, 255, 0, ${alpha})`;
    this.ctx.lineWidth = 3;
    this.ctx.beginPath();
    this.ctx.arc(
      position.x + this.robotSize.width / 2,
      position.y + this.robotSize.height / 2,
      radius,
      0,
      Math.PI * 2,
    );
    this.ctx.stroke();
  }

  drawDebugInfo() {
    if (!this.clientRobotState || !this.serverRobotState) return;

    this.ctx.strokeStyle = "rgba(255, 255, 0, 0.8)";
    this.ctx.lineWidth = 2;
    this.ctx.beginPath();
    this.ctx.moveTo(
      this.clientRobotState.position.x + this.robotSize.width / 2,
      this.clientRobotState.position.y + this.robotSize.height / 2,
    );
    this.ctx.lineTo(
      this.serverRobotState.position.x + this.robotSize.width / 2,
      this.serverRobotState.position.y + this.robotSize.height / 2,
    );
    this.ctx.stroke();
  }
}

document.addEventListener("DOMContentLoaded", () => {
  new RobotClient();
});
