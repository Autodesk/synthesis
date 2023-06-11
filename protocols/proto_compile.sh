#!/bin/bash
if ! [ -d ../api/Api/Gen/ ]
then
    mkdir ../api/Api/Gen/
fi
if ! [ -d ../api/Api/Gen/Proto/ ]
then
    mkdir ../api/Api/Gen/Proto/
fi
if ! [ -d ../api/Api/Gen/Mirabuf/ ]
then
    mkdir ../api/Api/Gen/Mirabuf/
fi

protoc --csharp_out=../api/Api/Gen/Proto/ v1/*.proto
protoc --proto_path=../mirabuf --csharp_out=../api/Api/Gen/Mirabuf/ ../mirabuf/*.proto