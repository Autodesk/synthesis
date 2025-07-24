import type {
  GameStateData,
  InitData,
  Message,
  Metrics,
  PingData,
  Robot,
  RobotLeftData,
  ServerMetrics,
} from "./types";
import PeerConnection from "./PeerConnection";
import DisplayManager from "./DisplayManager";
import { generateId } from "./utils";

class RobotClient {
  peerConnection: PeerConnection;
  clientId: string;
  robotId: string | null = null;

  robots: Map<string, Robot> = new Map();
  robotSpeed = 200;

  clientToRobotId: Map<string, string> = new Map();

  inputs = {
    ArrowUp: false,
    ArrowDown: false,
    ArrowLeft: false,
    ArrowRight: false,
  };
  inputSequence = 0;
  inputBuffer: Map<number, any> = new Map();

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
    this.setupRobots();
    this.displayManager = new DisplayManager();
    this.displayManager.setupCanvas();
    this.setupInputHandlers();
    this.peerConnection = new PeerConnection(this.handlePeerMessage, {
      robotId: this.robotId!,
      worldSize: this.displayManager.worldSize,
      robots: [...this.robots.values()],
    });

    this.clientId = this.peerConnection.clientId;
    this.startMetricsCollection();
    this.gameLoop();
  }

  gameLoop() {
    const iteration = () =>
      setTimeout(() => {
        this.applyClientState();
        this.sendClientState();
        this.displayManager.render(
          Date.now(),
          this.robots,
          this.robotId!,
          this.clientMetrics,
        );
        iteration();
      }, 10);
    iteration();
  }

  testPing() {
    if (!this.peerConnection.connected) return;

    this.peerConnection.send({ type: "ping", data: { timestamp: Date.now() } });
  }

  handlePeerMessage = ((data: Message) => {
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
        case "pong":
          console.log("Pong!");
          break;
        case "serverMetrics":
          this.handleServerMetrics(data.data);
          break;
        default:
          console.warn("Could not parse message:", data);
      }
    } catch (error) {
      console.error("Failed to parse server message:", error);
    }
  }).bind(this);

  handleInit(data: InitData) {
    this.displayManager.setWorldsize(data.worldSize);
    this.clientToRobotId.set(data.clientId, data.robotId);

    data.robots.forEach((robot: Robot) => {
      this.robots.set(robot.id, { ...robot });
    });
    this.displayManager.updatePlayerCount(data.robots.length + 1);
    console.log(`Initialized with ${data.robots.length + 1} robots`);
  }

  handleGameState(data: GameStateData) {
    const timestamp = Date.now();
    data.otherRobots.forEach((robotData: Robot) => {
      this.robots.set(robotData.id, robotData);
    });
    this.measurePing(timestamp, data.timestamp);
  }

  handleRobotJoined(data: Robot) {
    this.robots.set(data.id, { ...data });
    this.displayManager.updatePlayerCount(this.robots.size);
    console.log(`Robot ${data.id} joined`);
  }

  handleRobotLeft(data: RobotLeftData) {
    const removeRobot = (robot: string) => {
      this.robots.delete(robot);
      this.displayManager.updatePlayerCount(this.robots.size);
      console.log(`Robot ${data.robotId} left`);
    };
    if (data.robotId === "") {
      [...this.robots.values()]
        .filter((robot) => robot.id !== this.robotId)
        .forEach((robot) => removeRobot(robot.id));
    } else {
      removeRobot(data.robotId);
    }
  }

  handlePing(data: PingData) {
    const timestamp = Date.now();
    this.peerConnection.send({
      type: "pong",
      data: { timestamp: data.timestamp },
    });
    this.measurePing(timestamp, data.timestamp);
  }

  handleServerMetrics(data: ServerMetrics) {
    this.clientMetrics.serverMetrics = data;
  }

  measurePing(ourTimestamp: number, theirTimestamp: number) {
    const rtt = ourTimestamp - theirTimestamp;
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
    this.displayManager.updateJitter(
      Math.round(this.clientMetrics.networkStats.jitter * 10) / 10,
    );
    const last = this.clientMetrics.networkStats.roundTripTimes.length - 1;
    if (last > -1) {
      this.displayManager.updateLatency(
        Math.round(this.clientMetrics.networkStats.roundTripTimes[last]! * 10) /
          10,
      );
    }
    this.displayManager.updateUptime(this.clientMetrics.startTime);
  }

  sendClientState() {
    const clientRobotState = this.robots.get(this.robotId ?? "");
    if (!clientRobotState || !this.peerConnection.connected) return;

    this.peerConnection.send({
      type: "gameState",
      data: {
        sequence: this.inputSequence,
        otherRobots: [
          ...this.robots.values().filter((robot) => robot.id !== this.clientId),
        ],
        timestamp: Date.now(),
      },
    });
  }

  setupInputHandlers() {
    document.addEventListener("keydown", (e) => {
      console.log(`keydown: ${e.key}`);
      this.handleKeyInput(e.key, true);
    });

    document.addEventListener("keyup", (e) => {
      this.handleKeyInput(e.key, false);
    });

    document.addEventListener("keypress", (e) => {
      if (
        ["ArrowUp", "ArrowLeft", "ArrowDown", "ArrowRight", "r", "m"].includes(
          e.key,
        )
      ) {
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

    if (["ArrowUp", "ArrowLeft", "ArrowDown", "ArrowRight"].includes(key)) {
      this.inputs[key as "ArrowUp" | "ArrowLeft" | "ArrowDown" | "ArrowRight"] =
        pressed;
    }
  }

  applyClientState() {
    const clientRobotState = this.robots.get(this.robotId!);
    if (!clientRobotState) return;

    const now = Date.now();
    const deltaTime = Math.min(
      (now - clientRobotState.lastUpdateTime) / 1000,
      1 / 60,
    );

    const targetVelocity = { x: 0, y: 0 };
    if (this.inputs.ArrowUp) targetVelocity.y -= this.robotSpeed;
    if (this.inputs.ArrowDown) targetVelocity.y += this.robotSpeed;
    if (this.inputs.ArrowLeft) targetVelocity.x -= this.robotSpeed;
    if (this.inputs.ArrowRight) targetVelocity.x += this.robotSpeed;

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

  setupRobots() {
    const clientRobot: Robot = {
      id: generateId("robot"),
      position: { x: 0, y: 0 },
      rotation: 0,
      velocity: { x: 0, y: 0 },
      lastUpdateTime: Date.now(),
    };
    this.robotId = clientRobot.id;
    this.robots.set(this.robotId, clientRobot);
  }
}

document.addEventListener("DOMContentLoaded", () => {
  new RobotClient();
});
