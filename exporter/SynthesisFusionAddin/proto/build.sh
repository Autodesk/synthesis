mkdir ./proto_out
rm -rf ./proto_out/__pycache__
git submodule update --init --recursive
protoc -I=../../../mirabuf --python_out=./proto_out ../../../mirabuf/*.proto