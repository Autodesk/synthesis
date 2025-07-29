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
};

// TODO: Update this with Physics States instead of robots
// export type Message =
//   | { type: "init"; data: InitData }
//   | { type: "gameState"; data: GameStateData }
//   | { type: "collision"; data: CollisionData }
//   | { type: "robotJoined"; data: RobotJoinedData }
//   | { type: "robotLeft"; data: RobotLeftData }
//   | { type: "ping"; data: PingData }
//   | { type: "pong"; data: PingData };
//
// export type InitData = {
//   clientId: string;
//   robotId: string;
//   worldSize: { height: number; width: number };
//   robots: Robot[];
// };
//
// export type GameStateData = {
//   sequence: number;
//   otherRobots: Robot[];
//   timestamp: number;
// };
//
// export type CollisionData = {
//   sequence: number;
//   robots: Robot[];
//   timestamp: number;
// };
//
// export type RobotJoinedData = Robot;
// export type RobotLeftData = { robotId: string };
// export type PingData = { timestamp: number };
