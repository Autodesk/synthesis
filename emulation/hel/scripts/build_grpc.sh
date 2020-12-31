#!/bin/bash

SUDO=$(which sudo)

# Exit script if any command fails
set -e
set -o pipefail

cd $1 # GRPC folder

ADDITIONAL_FLAGS="-Wno-error"
export CXXFLAGS="$CXXFLAGS $ADDITIONAL_FLAGS"
export CFLAGS="$CFLAGS $ADDITIONAL_FLAGS"

TARGET_TRIPLE=$(${TOOLCHAIN}gcc -dumpmachine)

if [[ $(which grpc_cpp_plugin) == "" || $(which protoc) == "" ]] ; then
    printf "Installing native gRPC\n\n"
    make -j10 && \
        ${SUDO} make install && \
        ${SUDO} ldconfig
    make clean
else
    printf "Native gRPC installed. Skipping native build.\n\n"
fi

export GRPC_CROSS_COMPILE=true # TODO probably move this into if statement below
if [[ ${TOOLCHAIN} != "" ]] ; then 
    printf "Cross-compiling and installing gRPC\n\n"

    export GRPC_CROSS_AROPTS="cr --target=elf32-little"
else
    SCRIPT=$(realpath $0)
    SCRIPTPATH=$(dirname $SCRIPT)
    patch -p1 < $SCRIPTPATH/../external_configs/grpc.patch # This is necessary for v1.21.4 for x86
fi

make plugins -j10 \
     HAS_PKG_CONFIG=false \
     CC=${TOOLCHAIN}gcc \
     CXX=${TOOLCHAIN}g++ \
     CPP=${TOOLCHAIN}cpp \
     RANLIB=${TOOLCHAIN}ranlib \
     LD=${TOOLCHAIN}ld \
     LDXX=${TOOLCHAIN}g++ \
     HOSTLD=${TOOLCHAIN}ld \
     AR=${TOOLCHAIN}ar \
     AS=${TOOLCHAIN}as \
     AROPTS='-r' \
     EMBED_ZLIB="true" \
     PROTOBUF_CONFIG_OPTS="--host=${TARGET_TRIPLE} --build=$(gcc -dumpmachine) --with-protoc=$(find . -name protoc -print -quit)"

make REQUIRE_CUSTOM_LIBRARIES_opt=1 static -j10 \
     HAS_PKG_CONFIG=false \
     CC=${TOOLCHAIN}gcc \
     CXX=${TOOLCHAIN}g++ \
     CPP=${TOOLCHAIN}cpp \
     RANLIB=${TOOLCHAIN}ranlib \
     LD=${TOOLCHAIN}ld \
     LDXX=${TOOLCHAIN}g++ \
     HOSTLD=${TOOLCHAIN}ld \
     AR=${TOOLCHAIN}ar \
     AS=${TOOLCHAIN}as \
     AROPTS='-r' \
     EMBED_ZLIB="true" \
     PROTOBUF_CONFIG_OPTS="--host=${TARGET_TRIPLE} --build=$(gcc -dumpmachine) --with-protoc=$(find . -name protoc -print -quit)"
