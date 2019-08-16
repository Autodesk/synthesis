# Target system
SET(CMAKE_SYSTEM_NAME Linux)
SET(CMAKE_SYSTEM_VERSION 1)

set(CMAKE_FIND_ROOT_PATH /usr/arm-frc2019-linux-gnueabi)

# Cross compiler
SET(CMAKE_C_COMPILER   /usr/bin/arm-frc2019-linux-gnueabi-gcc)
SET(CMAKE_CXX_COMPILER /usr/bin/arm-frc2019-linux-gnueabi-g++)

# Search for programs in the build host directories
SET(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)

# Libraries and headers in the target directories
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_PACKAGE ONLY)
