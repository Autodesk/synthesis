#!/usr/bash
mkdir -p ../api/Api/Gen/Proto/
mkdir -p ../api/Api/Gen/Mirabuf/
mkdir -p ../server/Gen/Proto/
protoc --csharp_out=../api/Api/Gen/Proto/ v1/*.proto
protoc --csharp_out=../server/Gen/Proto/ v1/ServerConnection.proto
protoc --proto_path=../mirabuf --csharp_out=../api/Api/Gen/Mirabuf/ ../mirabuf/*.proto

