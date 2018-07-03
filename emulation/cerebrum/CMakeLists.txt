cmake_minimum_required (VERSION 2.6)
project (cerebrum)

include(ExternalProject)

option(BUILD_DOC "Build documentation" OFF)

file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin/tests)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/build)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/docs)

set (cerebrum_VERSION_MAJOR 1)
set (cerebrum_VERSION_MINOR 0)

set (WPILIB_DIRECTORY "../external/allwpilib")
set (WPIUTIL_DIRECTORY "../external/allwpilib/wpiutil")

set (CMAKE_C_COMPILER arm-frc-linux-gnueabi-gcc)
set (CMAKE_CXX_COMPILER arm-frc-linux-gnueabi-g++)

set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -pedantic -Wextra -std=c++14")

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/build)

if(CMAKE_BUILD_TYPE MATCHES "[Dd][Ee][Bb][Uu][Gg]")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O0 -g")
elseif(CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3 -Werror")
else()
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3")
endif()


# If we decide to add automated unit testing, uncomment this block
if(TEST MATCHES "^[Tt][Rr][Uu][Ee]" OR TEST MATCHES "^[Oo][Nn]")
  ExternalProject_Add(GoogleTest
    DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
    GIT_REPOSITORY https://www.github.com/google/googletest
    CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION}
    )
endif(TEST)

find_package(Doxygen)
if(DOXYGEN_FOUND)#AND CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]")

  #set(DOXYGEN_IN ${CMAKE_BINARY_DIR}/docs/Doxyfile.in)
  #set(DOXYGEN_OUT ${CMAKE_BINARY_DIR}/docs/Doxyfile)

  #configure_file(${DOXYGEN_IN} ${DOXYGEN_OUT})

  set(DOXYFILE ${CMAKE_BINARY_DIR}/docs/Doxyfile)

  add_custom_target( doxygen ALL
    COMMAND ${DOXYGEN_EXECUTABLE} ${DOXYFILE}
    WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
    COMMENT "Generating API documentation"
    VERBATIM )

else()
  MESSAGE("Doxygen not found; skipping")
endif()

include_directories("${WPILIB_DIRECTORY}/hal/src/main/native")
include_directories("${WPILIB_DIRECTORY}/ni-libraries/include")
include_directories("${WPIUTIL_DIRECTORY}/src/main/native/include")
include_directories("${WPILIB_DIRECTORY}/hal/src/main/native/include")
include_directories("./include")

add_library (cerebrum SHARED src/analog_input.cpp src/analog_output.cpp src/pwm.cpp src/digital_io.cpp)
add_executable (digital-io src/digital_io.cpp)
target_compile_options (digital-io PUBLIC -DDIGITAL_IO_TEST)
