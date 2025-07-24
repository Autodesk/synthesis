<!--toc:start-->

- [Task](#task)
- [Symptom](#symptom)
- [Solution](#solution)
  - [Code Quality](#code-quality)
  - [Performance](#performance)
  - [Architecture](#architecture)
    - [Authoritative Client Approach](#authoritative-client-approach)
- [Verification](#verification)
<!--toc:end-->

## Task

<!--
Please include any relevant Jira ticket ID(s) at the end of the PR title, in the form AARD-xxxx, where "AARD" is Jira project.
Include the same Jira ticket ID(s) in this section.
-->

AARD-1960

<!--
Provide a brief description of what the task was here.
-->

The point of this spike was to determine the feasibility of the WebRTC protocol for peer-to-peer communication between clients in the Synthesis multiplayer mode.

This spike has been split up into three sections:

1. Code quality
2. Performance
3. Architecture

## Symptom

<!--
How does the problem manifest itself?
Describe the problem as seen by the user, or by the caller or the code if it's not directly visible to the user.

Note: "Symptom" can include new product use cases, not just bugs.
-->

We need to decide on a technology to use for multiplayer communication.

## Solution

<!--
How did you fix the problem/symptom?
Explain your approach and reasoning for choosing this solution.
-->

### Code Quality

In my opinion, the [PeerJs](https://peerjs.com/) framework makes establishing connections and sending message between peers concise and easy to understand. It is a helpful and robust abstraction on the native WebRTC API.

PeerJs allows you to use this approximate logic for finding and messaging another peer

Setup custom PeerJs server on port 9000

```typescript
const peerServer = PeerServer({ port: PORT, path: "/", allow_discovery: true });
```

Connect to a PeerJs server

```typescript
const clientId = this.generateClientId();
const peer = new Peer(clientId, {
  host: "localhost",
  port: 9000,
  path: "/",
});
```

Get a list of all peers on the server and connect to one of them

```typescript
peer.listAllPeers((peers) => {
  const otherPeers: string[] = [];
  peers
    .filter((peerId) => peerId !== clientId)
    .forEach((peerId) => otherPeers.push(peerId as string));

  const firstPeer = otherPeers.pop();
  const connection = peer.connect(otherPeers);
});
```

Setup event listeners for peer

```typescript
connection.on("open", () => {
  console.log("Connection opened");
});

connection.on("data", (data: any) => {
  // Message handling logic here
});
```

### Performance

Most of this game was retrofitted from the [client prediction model spike](https://github.com/Autodesk/synthesis/pull/1220)

### Architecture

Due to the significant computational and thus financial cost of having an [authoritative server](https://github.com/Autodesk/synthesis/pull/1198) do the physics calculations and handle the I/O for all multiplayer games, with performance for the client being the same, a peer-to-peer solution would be preferable.

There have been two proposals for a peer-to-peer architecture that effectively synchonize the states of the two clients during disruptive events like collisions and knocking over game pieces, the [Hybrid Approach](#authoritative-client-approach) and the [Authoritative Client Approach](#authoritative-client-approach).

#### Authoritative Client Approach

In this architecture, one the hosting client essentially acts as the authoritative server, doing all the physics calculations and sending the entire physics state to the guest client every tick. This much like its server-based counterpart, this would be potentially extremely slow for the host, but it could be sped up for the guest with a [prediction model](https://github.com/Autodesk/synthesis/pull/1220).

#### Hybrid Approach

In this architecture, each client handles handles the physics calculations for their own robot which it sends to the other client each tick. The exception to this would be whenever it collided with either another robot or a game piece, in that case, it would send over its entire physics state to the other client(s), who would apply it. The hosting robot would have priority in the case that each client sent

## Verification

<!--
How did you test and verify your changes were correct?
List steps taken, tests/added/updated, and any manual verification done that should be replicated in review.
-->

---

Before merging, ensure the following criteria are met:

- [ ] All acceptance criteria outlined in the ticket are met.
- [ ] Necessary test cases have been added and updated.
- [ ] A feature toggle or safe disable path has been added (if applicable).
- [ ] User-facing polish:
  - Ask: _"Is this ready-looking?"_
- [ ] Cross-linking between Jira and GitHub:
  - PR links to the relevant Jira issue.
  - Jira ticket has a comment referencing this PR.

```

```
