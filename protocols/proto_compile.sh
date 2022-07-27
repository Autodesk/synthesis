#!/usr/bash
mkdir -p ../api/Api/Gen/Proto/
mkdir -p ../api/Api/Gen/Mirabuf/
mkdir -p ../server/ServerApi/Gen/Proto/
protoc --csharp_out=../api/Api/Gen/Proto/ v1/*.proto
protoc --proto_path=v1/server --csharp_out=../server/ServerApi/Gen/Proto/ v1/server/*.proto
protoc --proto_path=../mirabuf --csharp_out=../api/Api/Gen/Mirabuf/ ../mirabuf/*.proto

