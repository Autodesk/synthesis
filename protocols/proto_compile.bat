@echo off
protoc -I=./mirabuf --csharp_out=../api/Api/Proto/ ./mirabuf/*.proto
protoc -I=./mirabuf --python_out=../exporter/SynthesisFusionAddin/proto/proto_out ./mirabuf/*.proto