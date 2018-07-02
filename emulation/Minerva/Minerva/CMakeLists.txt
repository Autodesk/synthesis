cmake_minimum_required (VERSION 2.6)
project (Minerva)

set (Minerva_VERSION_MAJOR 1)
set (Minerva_VERSION_MINOR 0)

set (WPILIB_DIRECTORY "../../external/allwpilib")
set (WPIUTIL_DIRECTORY "../../external/allwpilib/wpiutil")

set (CMAKE_C_COMPILER arm-frc-linux-gnueabi-gcc)
set (CMAKE_CXX_COMPILER arm-frc-linux-gnueabi-g++)

set (CMAKE_CXX_STANDARD 14)
set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall")

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)

include_directories("${WPILIB_DIRECTORY}/hal/src/main/native")
include_directories("${WPILIB_DIRECTORY}/ni-libraries/include")
include_directories("${WPIUTIL_DIRECTORY}/src/main/native/include")
include_directories("${WPILIB_DIRECTORY}/hal/src/main/native/include")
include_directories("./include")

add_library (minerva SHARED src/analog_input.cpp src/analog_output.cpp src/pwm.cpp src/digital_io.cpp)
