import Peer, { DataConnection } from "peerjs";

class PeerConnection {
  peer: Peer;
  connection?: DataConnection;
  clientId: string;
  connected: boolean = false;
  handlePeerMessage: (data: any) => void;

  constructor(handlePeerMessage: (data: any) => void) {
    this.clientId = this.generateClientId();
    this.peer = new Peer(this.clientId, {
      host: "localhost",
      port: 9000,
      path: "/",
    });
    this.handlePeerMessage = handlePeerMessage;

    this.peer.on("open", (id: string) => {
      this.connected = true;
      console.log(`Peer connected: ID - ${id}`);
      this.connectToPeer(); // Replace 'some-peer-id' with the actual peer ID
      console.log(`Their peer ID is: ${this.getOtherPeerId()}`);
    });

    this.peer.on("connection", (conn) => {
      this.connection = conn;
      this.setupConnectionHandlers();
    });
  }

  connectToPeer() {
    if (!this.connection) return;

    const peerId = this.connection?.peer;
    this.connection = this.peer.connect(peerId);
    this.setupConnectionHandlers();
  }

  setupConnectionHandlers() {
    if (!this.connection) return;

    this.connection.on("open", () => {
      console.log("Connection opened");
    });

    this.connection.on("data", (data: any) => {
      this.handlePeerMessage(data);
    });

    this.connection.on("close", () => {
      this.connected = false;
      console.log("Connection closed");
    });

    this.connection.on("error", (err: Error) => {
      console.error("Connection error:", err);
    });
  }

  send(message: any) {
    if (this.connection && this.connected) {
      this.connection.send(message);
    }
  }

  generateClientId(): string {
    return `client-${Math.random().toString(36).substring(2, 9)}`;
  }

  getOtherPeerId(): string | null {
    if (!this.connection) return null;

    return this.connection.peer;
  }
}

export default PeerConnection;
