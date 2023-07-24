@echo off
md .\proto_out\
@RD /S /Q "./proto_out/__pycache__"
@echo on
protoc -I=../../../mirabuf --python_out=./proto_out ../../../mirabuf/*.proto
@echo off
