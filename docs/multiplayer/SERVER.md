Usage: multiplayer-server [-p <port>] [-h] [-s] [-r <permanent-room>]

A Server for Facilitating Multiplayer Synthesis

Options:
-p, --port port to run server on
-h, --headless whether to run the application without or tui or with one
-s, --secure whether to run the server through the WebSocketSecure
protocol or not. self-signed PEM certficates will be
automatically generated into the `./secrets/` directory
-r, --permanent-room
initially populate the server with a room that will persist
even when no users occupy it. value must be a six digit
string consisting only of valid base-10 digits and uppercase
characters
--help, help display usage information
