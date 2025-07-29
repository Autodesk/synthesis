import MirabufInstance from "@/mirabuf/MirabufInstance";
import Mechanism from "../physics/Mechanism";
import PhysicsSystem from "../physics/PhysicsSystem";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";

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

export type Message =
  // Represents the initial information given in thee lobby or smth
  | { type: "info"; data: ClientInfo }
  | { type: "init"; data: InitData }
  | { type: "update"; data: UpdateMultiplayerObjectData[] }
  | { type: "collision"; data: CollisionData }
  | { type: "newObject"; data: InitMultiplayerObjectData }
  | { type: "robotLeft"; data: RobotLeftData }
  | { type: "ping"; data: PingData }
  | { type: "pong"; data: PingData };

export type ClientInfo = {
  displayName: string;
  clientId: string;
};

// TODO: Figure out if InitMultiplayerObjectData is still necessary
export type InitData = {
  physicsSystem: PhysicsSystem;
  objects: InitMultiplayerObjectData[]; // We need to send the entire scene object with rendering data and configuration (for fields and such)
};

export type UpdateMultiplayerObjectData = {
  sceneObjectKey: number;
  mechanism: Mechanism;
  instance: MirabufInstance;
};

export type InitMultiplayerObjectData = {
  key: number; // TODO Check if we actually have to sync up keys (i think it's best if we do)
  sceneObject: MirabufSceneObject;
};

export type CollisionData = {
  physicsSystem: PhysicsSystem;
  sceneObject: Map<number, MirabufSceneObject>;
};

export type RobotLeftData = {
  sceneObjectKey: number;
};

export type PingData = { timestamp: number };
