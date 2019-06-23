#!/bin/bash

set -e
set -o pipefail

WPILIB_DIR=$1
HAL_CROSS_DIR=$(pwd)

printf "\nWarning: setup for HAL Windows build will make WPILib directory useless for any other purpose\n\n"

printf "\nDeleting HAL sim files\n\n"

rm -rf $WPILIB_DIR/hal/src/main/native/sim

printf "\nPatching files\n\n"

cd $WPILIB_DIR
patch -p1 < $HAL_CROSS_DIR/config.patch
patch -p1 < $HAL_CROSS_DIR/DigitalInternal.patch
cd - > /dev/null

printf "\nReplacing HAL sim files with Athena files\n\n"

mv $WPILIB_DIR/hal/src/main/native/athena $WPILIB_DIR/hal/src/main/native/sim
cp -r $HAL_CROSS_DIR/src/* $WPILIB_DIR/hal/src/main/native/sim/
cp -r $HAL_CROSS_DIR/jni_src/* $WPILIB_DIR/hal/src/main/native/cpp/jni/
cp -r $HAL_CROSS_DIR/include/* $WPILIB_DIR/hal/src/main/native/include/

printf "\nBuilding HAL JNI\n\n"

cd $WPILIB_DIR
./gradlew :hal:halJNIX86-64ReleaseSharedLibrary
cd -
