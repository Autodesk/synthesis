#!/bin/bash

# Exit script if any command fails
set -e
set -o pipefail

TOOLCHAIN=arm-frc2019-linux-gnueabi

if [[ $( bc <<< "$( $TOOLCHAIN-gcc -dumpversion | cut -c1 ) > 7" ) == 1 ]] ; then
	export CXXFLAGS="-Wno-error=class-memaccess -Wno-error=ignored-qualifiers -Wno-error=stringop-truncation"
	export CFLAGS="-Wno-error=class-memaccess -Wno-error=ignored-qualifiers -Wno-error=stringop-truncation"
fi

git submodule update --init

printf "\nCross-compiling and installing protobuf\n\n\n"

cd third_party/protobuf
./autogen.sh
./configure
make -j10 && make check && sudo make install && sudo ldconfig

printf "\nCross-compiling and installing gRPC\n\n\n"

cd ../../
make clean
make plugins -j10

export GRPC_CROSS_COMPILATION=true
export GRPC_CROSS_AROPTS="cr --target=$TOOLCHAIN"

make \
     HAS_PKG_CONFIG=false \
     CC=$TOOLCHAIN-gcc \
     CXX=$TOOLCHAIN-g++ \
     RANLIB=$TOOLCHAIN-ranlib \
     LD=$TOOLCHAIN-ld \
     HOSTLD=$TOOLCHAIN-ld \
     AR=$TOOLCHAIN-ar \
     AROPTS='-r' \
     PROTOBUF_CONFIG_OPTS="--host=$TOOLCHAIN --with-protoc=$(find . -name protoc -print -quit)" static -j10
