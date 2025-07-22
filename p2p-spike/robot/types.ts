export type Point = {
  x: number;
  y: number;
};

export type Velocity = {
  x: number;
  y: number;
};

export type Robot = {
  id: string;
  position: Point;
  rotation: number; // degrees
  velocity: Velocity;
  lastUpdateTime: number;
};

export type ServerMetrics = {
  actualTickRate?: number;
  tickRate?: number;
  messagesPerSecond?: number;
  memoryUsageMB?: number;
};

export type Metrics = {
  startTime: number;
  totalFrames: number;
  frameTimes: number[];
  inputsSent: number;
  messagesReceived: number;
  bytesReceived: number;
  bytesSent: number;
  connectionTime: number;
  averageFPS: number;
  networkStats: {
    packetsLost: number;
    roundTripTimes: number[];
    jitter: number;
  };
  serverMetrics: ServerMetrics;
};
