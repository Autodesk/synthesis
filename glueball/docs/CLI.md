```
Usage: glueball [--config-file <config-file>] [--cert-dir <cert-dir>] [-p <port>] [-h] [-s] [-r <permanent-room>]

A Server for Facilitating Multiplayer Synthesis

Options:
  --config-file     path to configuration file. additional command-line flags
                    will override options set in the config file
  --cert-dir        directory in which to store certificates and key files (only
                    for --secure mode)
  -p, --port        port to listen on
  -h, --headless    show a text console instead of the interactive TUI
  -s, --secure      encrypt websocket traffic using TLS. self-signed PEM
                    certificates will be automatically generated
  -r, --permanent-room
                    create a persistent room with this code that is always
                    available. code must be a six-character string consisting
                    only of valid base-10 digits and uppercase characters
  --help, help      display usage information
```
