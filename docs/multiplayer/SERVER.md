Usage: glueball [--config-file <config-file>] [--cert-dir <cert-dir>] [-p <port>] [-h] [-s] [-r <permanent-room>]

A Server for Facilitating Multiplayer Synthesis

Options:
  --config-file     configuration file for server. all flags passed in addition
                    to this one will be overridden by the corresponding option
                    in the specified config file
  --cert-dir        directory in which to story the certificate files in secure
                    mode
  -p, --port        on which port to run the server
  -h, --headless    whether to run the application without or tui or with one
  -s, --secure      whether to run the server through the WebSocketSecure
                    protocol or not. self-signed PEM certificates will be
                    automatically generated
  -r, --permanent-room
                    initially populate the server with a room that will persist
                    even when no users occupy it. value must be a six digit
                    string consisting only of valid base-10 digits and uppercase
                    characters
  --help, help      display usage information

