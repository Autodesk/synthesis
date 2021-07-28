import socket
import sys


sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_address = ('localhost', 13000)

sock.connect(server_address)

sock.sendall(message)