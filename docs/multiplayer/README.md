# Multiplayer Setup

## Running the Server

### From Binary

TODO

### Build from Scratch

1. [Install the rust toolchain](https://rustup.rs/)
2. Clone the project (e.g. `git clone https://github.com/Autodesk/synthesis.git`)
3. `cd multiplayer`
4. Build the application `cargo build --release`
5. Run the binary `./target/release/multiplayer-server`

## Connecting

1. Make sure server is running on known port in insecure mode (default)
2. Open Fission (`https://synthesis.autodesk.com/fission`)
3. Press the `Multiplayer` button on the starting modal
4. Type in the appropriate port
5. Switch secure mode off (on the client).
6. Press the `Test Connection` button
7. Type in your display name
8. Either
   a. `Join` a room from the list, if there are any, or
   b. Press `Create Room` to start a new game that others can join
9. Enjoy the Synthesis multiplayer experience!

## Proxying

The steps above will allow you to access your server with a local IP address. If you want to make your server accessible across the internet, you can either use port forwarding or a proxy service. A short guide on setting up a free proxy can be found at [./PROXY.md](./PROXY.md)

