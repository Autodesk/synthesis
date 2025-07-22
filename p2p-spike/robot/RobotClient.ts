import type { Metrics, Robot, ServerMetrics } from "./types";
import PeerConnection from "./PeerConnection";
import DisplayManager from "./DisplayManager";

class RobotClient {
  peerConnection: PeerConnection;
  clientId: string;
  robotId: string | null = null;

  robots: Map<string, Robot> = new Map();
  robotSpeed = 200;

  inputs = { w: false, s: false, a: false, d: false };
  // inputSequence = 0;
  // inputBuffer: Map<number, any> = new Map();

  displayManager: DisplayManager;

  clientMetrics: Metrics = {
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
    serverMetrics: {} as ServerMetrics,
  };

  constructor() {
    this.peerConnection = new PeerConnection(this.handlePeerMessage);
    this.clientId = this.peerConnection.clientId;
    this.displayManager = new DisplayManager();

    this.displayManager.setupCanvas();
    this.setupInputHandlers();
    this.displayManager.startRenderLoop(
      this.robots,
      this.robotId ?? "",
      this.clientMetrics,
    );
    this.startMetricsCollection();
  }

  handlePeerMessage(data: any) {
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
    this.displayManager.setWorldsize(data.worldSize);

    data.robots.forEach((robot: Robot) => {
      this.robots.set(robot.id, { ...robot });
    });
    this.displayManager.updatePlayerCount(data.robots.length);
    console.log(`Initialized with ${data.robots.length} robots`);
  }

  handleGameState(data: any) {
    this.displayManager.updateServerTick(data.sequence);

    data.robots.forEach((robotData: any) => {
      const robot = this.robots.get(robotData.id);
      if (robot) {
        robot.position = { ...robotData.position };
        robot.velocity = { ...robotData.velocity };
        robot.rotation = robotData.rotation;
      }
    });

    this.displayManager.updatePlayerCount(data.robots.length);
  }

  handleRobotJoined(data: any) {
    this.robots.set(data.id, { ...data });
    this.displayManager.updatePlayerCount(this.robots.size);
    console.log(`Robot ${data.id} joined`);
  }

  handleRobotLeft(data: any) {
    this.robots.delete(data.robotId);
    this.displayManager.updatePlayerCount(this.robots.size);
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
    this.displayManager.updateFPS(this.clientMetrics.averageFPS ?? 0);
    this.displayManager.updateTotalMessages(
      this.clientMetrics.messagesReceived,
    );
    this.displayManager.updateJitter(
      Math.round(this.clientMetrics.networkStats.jitter * 10) / 10,
    );
    this.displayManager.updateUptime(this.clientMetrics.startTime);
  }

  sendClientState() {
    const clientRobotState = this.robots.get(this.robotId ?? "");
    if (!clientRobotState) return;

    this.peerConnection.send({
      type: "clientState",
      data: {
        position: clientRobotState.position,
        velocity: clientRobotState.velocity,
        rotation: clientRobotState.rotation,
        timestamp: Date.now(),
      },
    });
  }

  setupCanvas() {}

  setupInputHandlers() {
    document.addEventListener("keydown", (e) => {
      this.handleKeyInput(e.key.toLowerCase(), true);
    });

    document.addEventListener("keyup", (e) => {
      this.handleKeyInput(e.key.toLowerCase(), false);
    });

    document.addEventListener("keypress", (e) => {
      if (["w", "a", "s", "d", "r", "m"].includes(e.key.toLowerCase())) {
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
          this.displayManager.toggleMetrics();
          return;
      }
    }

    if (["w", "a", "s", "d"].includes(key)) {
      this.inputs[key as "w" | "a" | "s" | "d"] = pressed;
    }
  }

  applyClientState() {
    const clientRobotState = this.robots.get(this.robotId ?? "");
    if (!clientRobotState) return;

    const now = Date.now();
    const deltaTime = Math.min(
      (now - clientRobotState.lastUpdateTime) / 1000,
      1 / 60,
    );

    const targetVelocity = { x: 0, y: 0 };
    if (this.inputs.w) targetVelocity.y -= this.robotSpeed;
    if (this.inputs.s) targetVelocity.y += this.robotSpeed;
    if (this.inputs.a) targetVelocity.x -= this.robotSpeed;
    if (this.inputs.d) targetVelocity.x += this.robotSpeed;

    const smoothing = 0.15;
    clientRobotState.velocity.x =
      clientRobotState.velocity.x * (1 - smoothing) +
      targetVelocity.x * smoothing;
    clientRobotState.velocity.y =
      clientRobotState.velocity.y * (1 - smoothing) +
      targetVelocity.y * smoothing;

    clientRobotState.position.x += clientRobotState.velocity.x * deltaTime;
    clientRobotState.position.y += clientRobotState.velocity.y * deltaTime;

    clientRobotState.position.x = Math.max(
      0,
      Math.min(
        this.displayManager.worldSize.width -
          this.displayManager.robotSize.width,
        clientRobotState.position.x,
      ),
    );
    clientRobotState.position.y = Math.max(
      0,
      Math.min(
        this.displayManager.worldSize.height -
          this.displayManager.robotSize.height,
        clientRobotState.position.y,
      ),
    );

    if (
      Math.abs(clientRobotState.velocity.x) > 10 ||
      Math.abs(clientRobotState.velocity.y) > 10
    ) {
      clientRobotState.rotation =
        (Math.atan2(clientRobotState.velocity.y, clientRobotState.velocity.x) *
          180) /
        Math.PI;
    }

    clientRobotState.lastUpdateTime = now;
  }

  resetStats() {
    // Reset client metrics
    this.clientMetrics.totalFrames = 0;
    this.clientMetrics.frameTimes = [];
    this.clientMetrics.inputsSent = 0;
    this.clientMetrics.messagesReceived = 0;
    this.clientMetrics.networkStats.roundTripTimes = [];
    this.clientMetrics.startTime = Date.now();

    console.log("Stats reset");
  }

  sendMessage(message: any) {
    if (this.peerConnection && this.peerConnection.connected) {
      const messageStr = JSON.stringify(message);
      this.clientMetrics.bytesSent += messageStr.length;
      this.peerConnection.send(messageStr);
    }
  }
}

document.addEventListener("DOMContentLoaded", () => {
  new RobotClient();
});
