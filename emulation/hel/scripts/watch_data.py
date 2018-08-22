#!/usr/bin/env python

# This script prints the data sent by HEL to the engine as it receives it

import socket
import os
import sys
import json

try:
    from Queue import Queue, Empty
except ImportError:
    from queue import Queue, Empty

RECEIVE_PORT = 11001
DATA_PREFIX = "{\"roborio\":"
DATA_SUFFIX = "\x1B"

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client_socket.connect(("localhost",RECEIVE_PORT))

try:
    while True:
        data = client_socket.recv(4096)
        if not data:
            pass
        msg = data.decode("utf-8")
        if DATA_PREFIX in msg:
            msg = msg[:(-1 * len(DATA_SUFFIX))]
            os.system("clear")
            parsed = json.loads(msg);
            print(json.dumps(parsed, indent = 4, sort_keys = False))
except KeyboardInterrupt:
    client_socket.close()
