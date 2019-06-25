#!/bin/bash

set -e
set -o pipefail

if [ $# -eq 0 ]; then
    printf "Please specify WPILib path\n"
    exit 1
fi
WPILIB_DIR=$1
HAL_CROSS_DIR=$(pwd)

printf "Warning: setup for HAL (Athena) cross compile will make that WPILib directory useless for any other purpose\n\n"

printf "Deleting HAL sim files\n\n"
rm -rf $WPILIB_DIR/hal/src/main/native/sim

printf "Patching files\n\n"

cd $WPILIB_DIR
patch -p1 < $HAL_CROSS_DIR/config.patch
if [[ "$OSTYPE" == "cygwin" || "$OSTYPE" == "win32" ]]; then
    patch -p1 < $HAL_CROSS_DIR/DigitalInternal.patch
fi
cd - > /dev/null

printf "\nReplacing HAL sim files with Athena files\n\n"

mv $WPILIB_DIR/hal/src/main/native/athena $WPILIB_DIR/hal/src/main/native/sim
cp -r $HAL_CROSS_DIR/jni_src/* $WPILIB_DIR/hal/src/main/native/cpp/jni/
if [[ "$OSTYPE" == "cygwin" || "$OSTYPE" == "win32" ]]; then
    cp -r $HAL_CROSS_DIR/src/* $WPILIB_DIR/hal/src/main/native/sim/
    cp -r $HAL_CROSS_DIR/include/* $WPILIB_DIR/hal/src/main/native/include/
fi

printf "Building HAL JNI\n\n"

cd $WPILIB_DIR
./gradlew :hal:halJNIX86-64ReleaseSharedLibrary
cd -
