import Peer, { DataConnection } from "peerjs";
import type {
  ClientInfo as ClientInfo,
  InitMultiplayerObjectData,
  Message,
} from "./types";
import PhysicsSystem from "../physics/PhysicsSystem";
import MirabufSceneObject from "@/mirabuf/MirabufSceneObject";
import World from "../World";
import { PORT } from "../../../../multiplayer/server";

class PeerConnection {
  serverConnection: Peer;
  connections: DataConnection[] = [];

  info: ClientInfo;
  isHost: boolean;
  connected: boolean = false;
  otherPeers: string[] = [];

  handlePeerMessage: (data: Message) => void;

  constructor(
    handlePeerMessage: (data: Message) => void,
    displayName: string = generateId("guest"),
    isHost: boolean = false,
  ) {
    this.isHost = isHost;
    this.info = { clientId: generateId("client"), displayName };
    this.serverConnection = new Peer(this.info.clientId, {
      host: window.location.hostname,
      port: PORT,
      path: "/connect",
    });

    this.handlePeerMessage = handlePeerMessage;

    this.serverConnection.on("open", (id: string) => {
      console.log(`Client connected: ID - ${id}`);
      this.connectToPeer(); // Replace 'some-peer-id' with the actual peer ID
    });

    this.serverConnection.on("connection", (conn) => {
      this.connections.push(conn);
      this.setupConnectionHandlers();
    });
  }

  connectToPeer() {
    this.serverConnection.listAllPeers((peers) => {
      console.log(`Peers: ${peers}`);
      peers
        .filter((peer) => peer !== this.info.clientId)
        .forEach((peer) => this.otherPeers.push(peer as string));
      if (this.otherPeers.length > 0)
        this.connections.push(
          this.serverConnection.connect(this.otherPeers[0]!),
        );
      console.log(
        `Connections: ${this.connections?.map((c) => c.connectionId)}`,
      );
      this.setupConnectionHandlers();
    });
  }

  // Called by the host, initializes the world with some defined set of objects, robots can be spawned in later
  initWorld(physicsSystem: PhysicsSystem) {
    const sceneObjects: InitMultiplayerObjectData[] = [
      ...World.sceneRenderer.sceneObjects.entries(),
    ]
      .filter(
        (sceneObjectPair): sceneObjectPair is [number, MirabufSceneObject] =>
          sceneObjectPair[1] instanceof MirabufSceneObject,
      )
      .map(([key, sceneObject]) => {
        return {
          key,
          sceneObject,
        };
      });
    const message: Message = {
      type: "init",
      data: { physicsSystem, objects: sceneObjects },
    };
    this.connections.forEach((c) => c.send(message));
  }

  setupConnectionHandlers() {
    if (this.connections.length === 0) return;

    this.connections.forEach((connection) => {
      connection.on("open", () => {
        this.connected = true;
        console.log("Connection opened");
        this.emit({ type: "info", data: this.info });
      });

      connection.on("data", (data: unknown) => {
        this.handlePeerMessage(data as Message);
      });

      connection.on("close", () => {
        this.connected = false;
        this.handlePeerMessage({
          type: "robotLeft",
          data: { sceneObjectKey: 0 }, // TODO Get actual sceneObjectKey
        });
        console.log("Connection closed");
      });

      // this.connection.on("disconnected", () => {
      //   this.handlePeerMessage({ type: "robotLeft", data: { robotId: "" } });
      // })

      connection.on("error", (err: Error) => {
        console.error("Connection error:", err);
      });
    });
  }

  emit(message: Message) {
    this.connections.forEach((connection) => connection.send(message));
  }

  send(message: Message, clientId: string) {
    if (this.connections.length === 0 || !this.connected) return;
    const connection = this.connections.find(
      (conn) => conn.connectionId === clientId,
    );
    if (!connection) return;

    connection.send(message);
  }

  getOtherPeerIds(): string[] {
    return this.connections.map((c) => c.peer);
  }
}

function generateId(root: string): string {
  return `${root}-${Math.random().toString(36).substring(2, 9)}`;
}

export default PeerConnection;
