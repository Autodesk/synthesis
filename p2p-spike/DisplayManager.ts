import type { Dimensions, Metrics, Point, Robot } from "./types";

export default class DisplayManger {
  worldSize: Dimensions = { width: 1000, height: 1000 };
  robotSize: Dimensions = { width: 50, height: 50 };

  canvas: HTMLCanvasElement;
  ctx: CanvasRenderingContext2D;
  lastRenderTime = 0;

  // HTML Elements
  statusEl: HTMLElement;
  playerCountEl: HTMLElement;
  latencyEl: HTMLElement;
  fpsEl: HTMLElement;
  uptimeEl: HTMLElement;
  jitterEl: HTMLElement;
  infoPanel: HTMLElement;
  controlsPanel: HTMLElement;
  metricsVisible = true;

  constructor() {
    this.canvas = document.getElementById("gameCanvas") as HTMLCanvasElement;
    this.ctx = this.canvas.getContext("2d")!;
    this.statusEl = document.getElementById("status")!;
    this.playerCountEl = document.getElementById("playerCount")!;
    this.latencyEl = document.getElementById("latency")!;
    this.fpsEl = document.getElementById("fps")!;
    this.uptimeEl = document.getElementById("uptime")!;
    this.jitterEl = document.getElementById("jitter")!;
    this.infoPanel = document.getElementById("info")!;
    this.controlsPanel = document.getElementById("controls")!;
  }

  updatePlayerCount(n: number) {
    this.playerCountEl.textContent = n.toString();
  }

  updateFPS(n: number) {
    this.fpsEl.textContent = n.toString();
  }

  updateJitter(n: number) {
    this.jitterEl.textContent = n.toString() + "ms";
  }
  updateLatency(n: number) {
    this.latencyEl.textContent = n.toString() + "ms";
  }
  updateUptime(startTime: number) {
    const uptime = Date.now() - startTime;
    const seconds = Math.floor(uptime / 1000) % 60;
    const minutes = Math.floor(uptime / 60000);
    this.uptimeEl.textContent = `${minutes}:${seconds.toString().padStart(2, "0")}`;
  }
  setWorldsize({ width, height }: { width: number; height: number }) {
    this.worldSize.width = width;
    this.worldSize.height = height;
  }
  toggleMetrics() {
    this.metricsVisible = !this.metricsVisible;
    const display = this.metricsVisible ? "block" : "none";

    if (this.infoPanel) this.infoPanel.style.display = display;
    if (this.controlsPanel) this.controlsPanel.style.display = display;

    console.log(`Metrics panels ${this.metricsVisible ? "shown" : "hidden"}`);
  }

  setupCanvas() {
    this.canvas.width = this.worldSize.width;
    this.canvas.height = this.worldSize.height;

    window.addEventListener("resize", () => {
      this.canvas.width = this.worldSize.width;
      this.canvas.height = this.worldSize.height;
    });
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

  render(
    timestamp: number,
    robots: Map<string, Robot>,
    robotId: string,
    clientMetrics: Metrics,
  ) {
    // Track frame timing
    if (this.lastRenderTime > 0) {
      const frameTime = timestamp - this.lastRenderTime;
      clientMetrics.frameTimes.push(frameTime);

      if (clientMetrics.frameTimes.length > 60) {
        clientMetrics.frameTimes.shift();
      }
    }
    clientMetrics.totalFrames++;

    this.ctx.fillStyle = "#000";
    this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);

    this.drawGrid();

    for (const [otherRobotId, robot] of robots) {
      if (otherRobotId === robotId) {
        this.drawOwnRobot(robot);
      } else {
        this.drawOtherRobot(robot);
      }
    }

    this.lastRenderTime = timestamp;
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
}
