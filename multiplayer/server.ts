import { PeerServer } from "peer";

export const PORT = 9002;

const peerServer = PeerServer({
  path: "/",
  port: PORT,
  allow_discovery: true,
});

console.log(`WebRTC Connection Server Running on Port ${PORT}`);
const connected = new Set()
peerServer.on("connection", (client) => {
  console.log(`Connection with client ${client.getId()}`);
  connected.add(client.getId())
});
peerServer.on("disconnect", (client) => {
  console.log(`Client ${client.getId()} disconnecting`)

  connected.delete(client.getId())
})

setInterval(() => {
  let string = ""
  connected.forEach((id) => string+=id+" ")
  console.log(string)
}, 15000)
// peerServer.on("")