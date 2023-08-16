#!/bin/sh

if [ "$1" = "clean" ]; then
    rm -rf build/
    rm -rf cef_binaries/
    exit 0
fi

if [ ! -d "build" ]; then
    mkdir build
fi

cd build/
cmake ../
