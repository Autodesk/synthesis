#!/bin/sh

./scripts/osx/resolve_deps.sh
git submodule update --init --recursive
cd protocols
./proto_compile.sh
cd ../engine/EngineDeps
./setup.sh
cd ../../api
dotnet build
./post_build.sh
cd ..
