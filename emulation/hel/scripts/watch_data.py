#!/usr/bin/env python

# This script prints the data sent by HEL to the engine as it receives it

import socket
import os
import sys

try:
    from Queue import Queue, Empty
except ImportError:
    from queue import Queue, Empty

RECEIVE_PORT = 11001
DATA_PREFIX = "{\"roborio\":"
DATA_SUFFIX = "\x1B"

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client_socket.connect(("localhost",RECEIVE_PORT))

while True:
    data = client_socket.recv(4096)
    msg = data.decode("utf-8")
    if DATA_PREFIX in msg:
        msg = msg[:(-1 * len(DATA_SUFFIX))]
        os.system("clear")
        print(msg)
