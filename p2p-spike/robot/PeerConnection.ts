import Peer from "peerjs";

class PeerConnection {
  peer: Peer;
  connection: any;
  clientId: string;
  connected: boolean = false;
  handlePeerMessage: (data: any) => void;

  constructor(clientId: string, handlePeerMessage: (data: any) => void) {
    this.clientId = clientId;
    this.peer = new Peer(clientId, {
      host: "localhost",
      port: 9000,
      path: "/",
    });
    this.handlePeerMessage = handlePeerMessage;

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
}

export default PeerConnection;
