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
};

export type Metrics = {
  actualTickRate: number;
  tickRate: number;
  messagesPerSecond: number;
  memoryUsageMB: number;
};
