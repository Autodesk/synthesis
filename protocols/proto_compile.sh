#!/bin/sh
mkdir -p ../api/Api/Gen/Proto/
mkdir -p ../api/Api/Gen/Mirabuf/
protoc --csharp_out=../api/Api/Gen/Proto/ v1/*.proto
protoc --proto_path=../mirabuf --csharp_out=../api/Api/Gen/Mirabuf/ ../mirabuf/*.proto

