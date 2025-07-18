import Peer from "peerjs";

class PeerConnection {
  peer: Peer;
  connection: any;
  clientId: string;
  connected: boolean = false;

  constructor(clientId: string) {
    this.clientId = clientId;
    this.peer = new Peer(clientId);

    this.peer.on("open", (id: string) => {
      this.connected = true;
      console.log(`Peer connected: ID - ${id}`);
    });

    this.peer.on("connection", (conn: any) => {
      this.connection = conn;
      this.setupConnectionHandlers();
    });
  }

  connectToPeer(peerId: string) {
    this.connection = this.peer.connect(peerId);
    this.setupConnectionHandlers();
  }

  setupConnectionHandlers() {
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

  handlePeerMessage(data: any) {
    // Custom logic to handle incoming messages
    console.log("Received message:", data);
  }
}

export default PeerConnection;
