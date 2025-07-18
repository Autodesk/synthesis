import { Peer } from "peerjs";
const peer = new Peer("client", {
  host: "localhost",
  port: 9000,
  path: "/",
});
const conn = peer.connect("client");

console.log(`yay: ${conn.peer}`);

peer.on("connection", () => {
  conn.on("data", (data) => {
    console.log(data);
  });
  conn.on("open", () => {
    conn.send("hello");
  });
});
