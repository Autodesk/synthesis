rm -rf -v ./proto_out
mkdir ./proto_out
git submodule update --init --recursive
protoc -I=../../../mirabuf --python_out=./proto_out ../../../mirabuf/*.proto