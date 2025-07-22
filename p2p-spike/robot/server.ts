import { PeerServer } from "peer";

export const PORT = 9000;

const peerServer = PeerServer({ port: PORT, path: "/" });

console.log(`WebRTC Connection Server Running on Port ${PORT}`);

peerServer.on("connection", (client) => {
  console.log(`Connection with client ${client.getId()}`);
});
