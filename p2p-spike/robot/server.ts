import { PeerServer } from "peer";

const PORT = 9000;

const peerServer = PeerServer({ port: PORT, path: "/" });

console.log(`WebRTC Connection Server Running on Port ${PORT}`);
