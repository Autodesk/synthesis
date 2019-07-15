#!/bin/bash

printf "\n"

# Finding protoc

PROTOC=""
if [[ "$*" != *"--system-protoc"* ]] ; then
    if [[ $(find . -name protoc -type f | wc -l) == 0 ]] ; then
        echo "Failed to find local protoc. Checking for system protoc."
        if [[ "$(which protoc)" != "" ]] ; then
            PROTOC=protoc
            echo "System protoc found. Defaulting to system protoc."
        else
            echo "Failed to find local or system protoc. Cannot continue execution. Exiting."
            return -1
        fi
    else
        PROTOC=$(find . -name protoc)
        echo "Found local protoc. Setting default protoc executable as local copy."
    fi
elif [[ "$(which protoc)" == "" ]] && [[ "$*" == *"--system-protoc"* ]] ; then
    echo "Failed to locate system protoc. Exiting."
    return -2
else
    PROTOC="protoc"
    echo "Found protoc in path. Using system protoc."
fi

# Finding gRPC plugin

GRPC_PLUGIN=""
if [[ "$*" != *"--system-grpc"* ]] ; then
    if [[ $(find . -name grpc_cpp_plugin -type f | wc -l) == 0 ]] ; then
        echo "Failed to find local grpc_cpp_plugin. Checking for system grpc_cpp_plugin."
        if [[ "$(which grpc_cpp_plugin)" != "" ]] ; then
            GRPC_PLUGIN=$(which grpc_cpp_plugin)
            echo "System grpc_cpp_plugin found. Defaulting to system grpc_cpp_plugin."
        else
            echo "Failed to find local and system grpc_cpp_plugin. Cannot continue execution. Exiting."
            return -1
        fi
    else
        GRPC_PLUGIN=$(find . -name grpc_cpp_plugin)
        echo "Found local grpc_cpp_plugin. Setting default gRPC cpp plugin as local copy"
    fi
elif [[ "$(which grpc_cpp_plugin)" == "" ]] && [[ "$*" == *"--system-grpc"* ]] ; then
    echo "Failed to locate system grpc_cpp_plugin. Exiting."
    return -2
else
    GRPC_PLUGIN=$(which grpc_cpp_plugin)
    echo "Found grpc_cpp_plugin in path. Using system gRPC cpp plugin."
fi

mkdir -p lib/gen
$PROTOC ../api/v1/proto/emulator_service.proto -I../api/v1/proto --cpp_out=./lib/gen
$PROTOC ../api/v1/proto/emulator_service.proto -I../api/v1/proto --grpc_out=./lib/gen --plugin=protoc-gen-grpc=$GRPC_PLUGIN

printf "\n"
