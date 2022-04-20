mkdir .\proto_out
git submodule update --init --recursive
protoc -I=../../../protocols/mirabuf --python_out=./proto_out ../../../protocols/mirabuf/*.proto