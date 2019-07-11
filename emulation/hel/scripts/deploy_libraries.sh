#!/bin/bash

# Deploy all WPILib support libraries

WPILIB_DIR="./lib/wpilib/src/wpilib"
OPENCV_DIR="${WPILIB_DIR}/build/tmp/expandedArchives/opencv-cpp-3.4.4-4-linuxathenadebug.zip_fb1838f89a18efba82e3c679b6ef0c3b/linux/athena/shared"

deploy(){
    if [ -f $1 ] || [ -d $1 ]; then
        printf "Copying $1\n"
        scp -r -P 10022 $1 lvuser@localhost:/home/lvuser
    else
        printf "Error: $1 Does not exist\n"
    fi
}

# Deploy all WPILib libaries, C++ and Java
deploy "${WPILIB_DIR}/cameraserver/build/libs/cameraserver/shared/athena/debug/libcameraserverd.so"
deploy "${WPILIB_DIR}/cscore/build/libs/cscore/shared/athena/debug/libcscored.so"
deploy "${WPILIB_DIR}/hal/build/libs/hal/shared/athena/debug/libwpiHald.so"
deploy "${WPILIB_DIR}/ntcore/build/libs/ntcore/shared/athena/debug/libntcored.so"
deploy "${WPILIB_DIR}/wpilibc/build/libs/wpilibc/shared/athena/debug/libwpilibcd.so"
deploy "${WPILIB_DIR}/wpiutil/build/libs/wpiutil/shared/athena/debug/libwpiutild.so"

deploy "${WPILIB_DIR}/cameraserver/build/libs/cameraserver.jar"
deploy "${WPILIB_DIR}/cscore/build/libs/cscore.jar"
deploy "${WPILIB_DIR}/hal/build/libs/hal.jar"
deploy "${WPILIB_DIR}/ntcore/build/libs/ntcore.jar"
deploy "${WPILIB_DIR}/wpilibj/build/libs/wpilibj.jar"
deploy "${WPILIB_DIR}/wpiutil/build/libs/wpiutil.jar"

# Deploy all OpenCV libraries
for i in ${OPENCV_DIR}/lib*.so.3.4; do
    deploy "$i"
done

printf "Files successfully copied\n"

# Deploy HEL
deploy "./lib/libhel.so"
