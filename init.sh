#!/bin/sh

git submodule update --init --recursive
cd protocols
./proto_compile.sh
cd ../engine/EngineDeps
dotnet build
./copy_deps.sh
cd ../../api
dotnet build
./post_build.sh
cd ..
