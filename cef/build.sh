#!/bin/sh

if [ ! -d "build" ]; then
    mkdir build
fi

cd build/
cmake -G "Unix Makefiles" -DPROJECT_ARCH="arm64" ../
make
