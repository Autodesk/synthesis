import socket
import sys
import controller_pb2 as controller
import io

#make sure to install google and protobuf modules

update_signals = controller.UpdateSignals()
update_signals.name = "Robot"

di1 = controller.UpdateSignal
di1.Io = controller.UpdateIOType.INPUT
di1.Class = "Digital"
di1.Value = 4.2

di2 = controller.UpdateSignal
di2.Io = controller.UpdateIOType.INPUT
di2.Class = "Digital"
di2.Value = 3.1

ao1 = controller.UpdateSignal
ao1.Io = controller.UpdateIOType.OUTPUT
ao1.Class = "Analog"
ao1.Value = 2


tmp1 = update_signals.signalMap["DI1"]
tmp1 = di1
tmp2 = update_signals.signalMap["DI2"]
tmp2 = di2
tmp3 = update_signals.signalMap["AO1"]
tmp3 = ao1

INT_SIZE_BYTES = 4


message_bytes = update_signals.SerializeToString()
size_bytes = len(message_bytes).to_bytes(INT_SIZE_BYTES, sys.byteorder)

print(size_bytes)
print(message_bytes)
print(size_bytes + message_bytes)


sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_address = ('localhost', 13000)

sock.connect(server_address)

#sock.sendall(size_bytes)
#sock.sendall(message_bytes)
sock.sendall(size_bytes + message_bytes)