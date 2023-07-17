#!/bin/sh

if [ "$1" = "clean" ]; then
    rm -rf build/
    rm -rf cef_binaries/
    exit 0
elif [ "$1" != "test" || "$1" != "build" || "$1" != "" ]; then
    echo "Usage: ./build.sh [clean|test|build]]"
    exit 1
fi

if [ ! -d "build" ]; then
    mkdir build
fi

cd build/

if [ "$1" = "test" ]; then
    cmake -G "Unix Makefiles" -DPROJECT_ARCH="arm64" -DBUILD_TESTING=ON ../
    make test
else
    cmake -G "Unix Makefiles" -DPROJECT_ARCH="arm64" ../
    make
fi
