mkdir ./proto_out
protoc -I=../../../protocols/mirabuf --python_out=./proto_out ../../../protocols/mirabuf/*.proto