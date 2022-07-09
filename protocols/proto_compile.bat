@echo off
protoc --csharp_out=../api/Api/Gen/Proto/ v1/*.proto
protoc --proto_path=../mirabuf --csharp_out=../api/Api/Gen/Mirabuf/ ../mirabuf/*.proto