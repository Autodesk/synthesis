mkdir .\proto_out
@RD /S /Q "./proto_out/__pycache__"
git submodule update --init --recursive
protoc -I=../../../protocols/mirabuf --python_out=./proto_out ../../../protocols/mirabuf/*.proto