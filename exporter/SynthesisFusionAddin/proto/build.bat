mkdir .\proto_out
@RD /S /Q "./proto_out/__pycache__"
protoc -I=../../../mirabuf --python_out=./proto_out ../../../mirabuf/*.proto
