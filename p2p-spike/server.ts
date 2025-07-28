import { ExpressPeerServer } from "peer";
import express from "express";
import path from "path";

export const PORT = 9000;

await Bun.build({
  entrypoints: ["./client.html"],
  outdir: "./build",
});

const app = express();
app.use("/", express.static(path.join(import.meta.dir, "build")));

app.get("/client", async (_req, res, _next) => {
  res.send(await Bun.file("./build/client.html").text());
});

const server = app.listen(PORT);

const peerServer = ExpressPeerServer(server, {
  path: "/",
  allow_discovery: true,
});

app.use("/connect", peerServer);

console.log(`WebRTC Connection Server Running on Port ${PORT}`);

peerServer.on("connection", (client) => {
  console.log(`Connection with client ${client.getId()}`);
});
