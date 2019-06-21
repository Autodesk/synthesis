#!/bin/bash 

PROTOC=""
GRPC_PLUGIN=""

if [[ "$*" != *"--system-protoc"* ]] ; then
    if [[ $(find . -name protoc -type f | wc -l) == 0 ]] ; then
        echo "Failed to find protoc. Checking for system protoc."
        if [[ "$(which protoc)" != "" ]] ; then
            echo "System protoc found. Defaulting to system protoc."
            PROTOC=protoc
        else
            echo "Failed to find local and system protoc. Cannot continue execution. Exiting."
            return -1
        fi
    else
        echo "Found protoc. Setting default protoc executable as local copy"
        protoc=$(find . -name protoc)
    fi
elif [[ "$(which protoc)" == "" ]] && [[ "$*" == *"--system-protoc"* ]] ; then
    echo "Failed to locate system protoc. Exiting."
    return -2
else
    echo "Found protoc in path. Using system protoc"
    PROTOC="protoc"
fi

if [[ "$*" != *"--system-grpc"* ]] ; then
    if [[ $(find . -name grpc_cpp_plugin -type f | wc -l) == 0 ]] ; then
        echo "Failed to find grpc_cpp_plugin. Checking for system grpc_cpp_plugin."
        if [[ "$(which grpc_cpp_plugin)" != "" ]] ; then
            echo "System grpc_cpp_plugin found. Defaulting to system grpc_cpp_plugin."
            GRPC_CPP_PLUGIN=grpc_cpp_plugin
        else
            echo "Failed to find local and system grpc_cpp_plugin. Cannot continue execution. Exiting."
            return -1
        fi
    else
        echo "Found grpc_cpp_plugin. Setting default protoc executable as local copy"
        GRPC_PLUGIN=$(find . -name grpc_cpp_plugin)
    fi
elif [[ "$(which grpc_cpp_plugin)" == "" ]] && [[ "$*" == *"--system-grpc"* ]] ; then
    echo "Failed to locate system grpc_cpp_plugin. Exiting."
    return -2
else
    echo "Found grpc_cpp_plugin in path. Using system protoc"
    GRPC_PLUGIN="grpc_cpp_plugin"
fi

mkdir -p lib/gen
$PROTOC -I ../api/v1/proto/ ../api/v1/proto/emulator_service.proto --cpp_out=./lib/gen
$PROTOC -I ../api/v1/proto ../api/v1/proto/emulator_service.proto --grpc_out=./lib/gen --plugin=protoc-gen-grpc=$(which $GRPC_PLUGIN)
