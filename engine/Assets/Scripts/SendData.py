import socket
import sys
import controller_pb2 as controller
import io

#make sure to install google and protobuf modules
#maybe try using double instead of value

di1 = controller.UpdateSignal(io=controller.UpdateIOType.INPUT, deviceType="Digital", value=4.2)

di2 = controller.UpdateSignal(io = controller.UpdateIOType.INPUT, deviceType = "Digital", value=3.1)

ao1 = controller.UpdateSignal(io = controller.UpdateIOType.OUTPUT, deviceType = "Analog", value=2)


tmp = {
  "DI1": di1,
  "DI2": di2,
  "AO1": ao1
}

update_signals = controller.UpdateSignals(name="Robot", signalMap=tmp)

INT_SIZE_BYTES = 4

message_bytes = update_signals.SerializeToString()
size_bytes = len(message_bytes).to_bytes(INT_SIZE_BYTES, sys.byteorder)

print(update_signals.ByteSize())
print(int.from_bytes(size_bytes, sys.byteorder))
print(size_bytes)
print(message_bytes)

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_address = ('localhost', 13000)

sock.connect(server_address)

sock.sendall(size_bytes + message_bytes)