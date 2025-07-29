import Peer, { DataConnection } from "peerjs";
import type { InitData, Message } from "./types";

class PeerConnection {
  peer: Peer;
  connection?: DataConnection;
  clientId: string;
  connected: boolean = false;
  otherPeers: string[] = [];
  initialization: InitData;
  handlePeerMessage: (data: Message) => void;

  constructor(
    handlePeerMessage: (data: Message) => void,
    initialization: Omit<InitData, "clientId">,
  ) {
    this.clientId = this.generateClientId();
    this.peer = new Peer(this.clientId, {
      host: "localhost",
      port: 9000,
      path: "/connect",
    });
    this.initialization = { ...initialization, clientId: this.clientId };
    this.handlePeerMessage = handlePeerMessage;

    this.peer.on("open", (id: string) => {
      console.log(`Client connected: ID - ${id}`);
      this.connectToPeer(); // Replace 'some-peer-id' with the actual peer ID
    });

    this.peer.on("connection", (conn) => {
      this.connection = conn;
      this.setupConnectionHandlers();
    });
  }

  connectToPeer() {
    this.peer.listAllPeers((peers) => {
      console.log(`Peers: ${peers}`);
      peers
        .filter((peer) => peer !== this.clientId)
        .forEach((peer) => this.otherPeers.push(peer as string));
      if (this.otherPeers.length > 0)
        this.connection = this.peer.connect(this.otherPeers[0]!);
      console.log(`Connection: ${this.connection?.peer}`);
      this.setupConnectionHandlers();
    });
  }

  setupConnectionHandlers() {
    if (!this.connection) return;

    this.connection.on("open", () => {
      this.connected = true;
      console.log("Connection opened");
      this.send({ type: "init", data: this.initialization });
    });

    this.connection.on("data", (data: any) => {
      this.handlePeerMessage(data);
    });

    this.connection.on("close", () => {
      this.connected = false;
      this.handlePeerMessage({ type: "robotLeft", data: { robotId: "" } });
      console.log("Connection closed");
    });

    // this.connection.on("disconnected", () => {
    //   this.handlePeerMessage({ type: "robotLeft", data: { robotId: "" } });
    // })

    this.connection.on("error", (err: Error) => {
      console.error("Connection error:", err);
    });
  }

  send(message: Message) {
    if (!this.connection || !this.connected) return;
    this.connection.send(message);
  }

  generateClientId(): string {
    return generateId("client");
  }

  getOtherPeerId(): string | null {
    if (!this.connection) return null;

    return this.connection.peer;
  }
}

function generateId(root: string): string {
  return `${root}-${Math.random().toString(36).substring(2, 9)}`;
}

export default PeerConnection;
