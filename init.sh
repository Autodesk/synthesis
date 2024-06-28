#!/bin/sh

git submodule update --init --recursive
cd protocols
./proto_compile.sh
cd ..
