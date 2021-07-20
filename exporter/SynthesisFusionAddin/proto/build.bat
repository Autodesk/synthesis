mkdir proto_out
del /f/s/q proto_out > nul
protoc -I=../../../protocols/mirabuf --python_out=./proto_out ../../../protocols/mirabuf/*.proto