#!/bin/bash

set -e
set -o pipefail

WPILIB_DIR=$1
HAL_CROSS_DIR=$(pwd)

printf "Deleting HAL sim files\n"
rm -rf $WPILIB_DIR/hal/src/main/native/sim

printf "Patching files\n"
cd $WPILIB_DIR
patch -p1 < $HAL_CROSS_DIR/config.patch
patch -p1 < $HAL_CROSS_DIR/DigitalInternal.patch
cd -

printf "Replacing HAL sim files with Athena files\n"
mv $WPILIB_DIR/hal/src/main/native/athena $WPILIB_DIR/hal/src/main/native/sim
cp -r $HAL_CROSS_DIR/include/* $WPILIB_DIR/hal/src/main/native/include/

printf "Building HAL JNI\n"
cd $WPILIB_DIR
./gradlew :hal:halJNIX86-64ReleaseSharedLibrary
cd -
