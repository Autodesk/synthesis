#!/bin/sh

if [ "$1" = "clean" ]; then
    rm -rf build/
    rm -rf cef_binaries/
    exit 0
elif [ "$1" != "test" ] && [ "$1" != "build" ] && [ "$1" != "debug" ] && [ "$1" != "" ]; then
    echo "Usage: ./build.sh [clean|test|build|debug]"
    exit 1
fi

if [ ! -d "build" ]; then
    mkdir build
fi

cd build/

if [ "$1" = "test" ]; then
    cmake -G "Unix Makefiles" -DBUILD_TESTING=ON ../
    make test
elif [ "$1" = "debug" ]; then
    cmake -G "Unix Makefiles" -DCMAKE_BUILD_TYPE=Debug ../
    make
else
    cmake -G "Unix Makefiles" ../
    make
fi
