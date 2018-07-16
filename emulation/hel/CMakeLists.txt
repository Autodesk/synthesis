cmake_minimum_required (VERSION 3.0.2)
project (hel)

include(ExternalProject)

option(BUILD_DOC "Build documentation" OFF)

file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin/tests)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/build)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/docs)
file(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/user_code)

set (hel_VERSION_MAJOR 1)
set (hel_VERSION_MINOR 0)

set (WPILIB_DIRECTORY "${CMAKE_BINARY_DIR}/../external/allwpilib")
set (WPIUTIL_DIRECTORY "${CMAKE_BINARY_DIR}/../external/allwpilib/wpiutil")
set (WPILIB_SO_DIRECTORY "${CMAKE_BINARY_DIR}/../external/wpilib")
set (ASIO_DIRECTORY "${CMAKE_BINARY_DIR}/../external/ASIO/asio/")

set (VM_BUILD_COMMAND "./run_vm.sh")
set (CLONE_ASIO_COMMAND "${CMAKE_BINARY_DIR}/scripts/clone_asio.sh")

if(TEST MATCHES "^[Tt][Rr][Uu][Ee]" OR TEST MATCHES "^[Oo][Nn]" AND NOT TEST_MODE MATCHES "^[Ww][Pp][Ii]")
  if (TEST_MODE MATCHES "^[Ww][Pp][Ii]")
    set (CMAKE_C_COMPILER arm-frc-linux-gnueabi-gcc)
    set (CMAKE_CXX_COMPILER arm-frc-linux-gnueabi-g++)
    message (STATUS "Configuration: WPILib Tests")
  else()
    set (CMAKE_C_COMPILER gcc)
    set (CMAKE_CXX_COMPILER g++)
    message (STATUS "Configuration: Unit Tests")
  endif()
else()
  set (CMAKE_C_COMPILER arm-frc-linux-gnueabi-gcc)
  set (CMAKE_CXX_COMPILER arm-frc-linux-gnueabi-g++)
  message (STATUS "Configuration: Defualt")
endif()

set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -pedantic -Wextra -std=c++14")

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/build)

set(EXTERNAL_INSTALL_LOCATION ${CMAKE_BINARY_DIR}/lib)

add_executable(generate_libwpi src/generate_libwpi.cpp)
add_custom_target(libwpi.so run_generator)
add_dependencies(libwpi.so generate_libwpi)


if(CMAKE_BUILD_TYPE MATCHES "[Dd][Ee][Bb][Uu][Gg]")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O0 -g")
elseif(CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3 -Werror")
else()
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3")
endif()

if(TEST MATCHES "^[Tt][Rr][Uu][Ee]" OR TEST MATCHES "^[Oo][Nn]")
  if(TEST_MODE MATCHES "[Ww][Pp][Ii]")
    add_custom_command(OUTPUT ${CMAKE_BINARY_DIR}/lib/libwpi.so
      COMMAND ./build/generate_libwpi
      DEPENDS generate_libwpi
      VERBATIM)

    link_directories ( ${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/ni-libraries/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/athena ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/athena ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/athena ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/athena ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/athena )
  else()
    add_custom_command(OUTPUT ${CMAKE_BINARY_DIR}/lib/libwpi.so x86
      COMMAND ./build/generate_libwpi
      DEPENDS generate_libwpi
      VERBATIM)

    link_directories (${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/ni-libraries/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86-64 ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86-64 ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64 ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64 ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/x86-64)
  endif()
endif()

# If we decide to add automated unit testing, uncomment this block
if(TEST MATCHES "^[Tt][Rr][Uu][Ee]" OR TEST MATCHES "^[Oo][Nn]")
  ExternalProject_Add(GoogleTest
    DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
    GIT_REPOSITORY https://www.github.com/google/googletest
    CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION}
    )

link_directories("lib/lib64")
endif()

if(TEST MATCHES "^[Tt][Rr][Uu][Ee]" OR TEST MATCHES "^[Oo][Nn]" AND NOT TEST_MODE MATCHES "^[Ww][Pp][Ii]")

  MESSAGE("Begun building tests")

  file(GLOB test_list tests/*.cpp)
  FOREACH(file ${test_list})
    string(REPLACE ".cpp" "" test_name_unclean ${file})
    get_filename_component( test_name ${test_name_unclean} NAME)

    MESSAGE("Begun building test: ${test_name}")

    add_executable( ${test_name} ${file} tests/main_stub.cpp)

    add_dependencies(${test_name} hel)
    add_dependencies(${test_name} GoogleTest)
    add_dependencies(${test_name} asio)
    add_dependencies(${test_name} wpilib)

    link_directories (${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86_64 ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86_64 ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64 ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64 )

    target_link_libraries(${test_name} gtest gtest_main pthread hel)
    include_directories("lib/include")
    target_include_directories(${test_name} PRIVATE "${ASIO_DIRECTORY}/include" "${WPILIB_DIRECTORY}/hal/src/main/native" "${WPIUTIL_DIRECTORY}/src/main/native/include" "${WPILIB_DIRECTORY}/hal/src/main/native/include" "${WPILIB_DIRECTORY}/ni-libraries/include" "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include" "./include")

    set_target_properties( ${test_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/tests")

#target_link_libraries(${test_name} hel)

  ENDFOREACH( file test_list)

endif()

find_package(Doxygen)
if(DOXYGEN_FOUND AND CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]" OR BUILD_DOC MATCHES "^[Oo][Nn]" OR BUILD_DOC MATCHES "^[Tt][Rr][Uu][Ee]")

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

add_custom_target ( build_vm 
	COMMAND ${VM_BUILD_COMMAND}
	WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
	COMMENT "Download, build, and run synthesis VM"
	VERBATIM )

add_custom_target ( asio ALL
  COMMAND ${CLONE_ASIO_COMMAND}
  WORKING_DIRECTORY ${CMAKE_BINARY_DIR}/scripts
  COMMENT "Download the C++ ASIO library"
  VERBATIM )

if(TEST MATCHES "^[Tt][Rr][Uu][Ee]" OR TEST MATCHES "^[Oo][Nn]")
    ExternalProject_Add(wpilib 
        DOWNLOAD_COMMAND bash -c "git submodule update --init && mkdir -p ${CMAKE_BINARY_DIR}/../external/allwpilib/build"
        CONFIGURE_COMMAND mkdir -p ${CMAKE_BINARY_DIR}/../external/allwpilib/build && cd ${CMAKE_BINARY_DIR}/../external/allwpilib/build && cmake -H.. -B. -DWITHOUT_CSCORE=true
        BUILD_COMMAND cd ${CMAKE_BINARY_DIR}/../external/allwpilib/build && make -j4
        INSTALL_COMMAND ""
        )
endif()
<<<<<<< HEAD
add_library (hel SHARED src/init.cpp src/analog_input.cpp src/analog_output.cpp src/pwm.cpp src/digital_io.cpp src/can.cpp src/relay.cpp src/global.cpp src/comm.cpp src/counter.cpp src/accelerometer.cpp src/accumulator.cpp src/encoder.cpp src/power.cpp src/sys_watchdog.cpp src/spi.cpp src/net_comm.cpp src/mxp_data.cpp src/util.cpp src/json_util.cpp src/send_data.cpp src/receive_data.cpp src/sync_server.cpp)
add_dependencies(hel asio wpilib generate_libwpi)

target_include_directories( hel PRIVATE "${WPILIB_DIRECTORY}/hal/src/main/native" "${WPIUTIL_DIRECTORY}/src/main/native/include" "${WPILIB_DIRECTORY}/hal/src/main/native/include" "${WPILIB_DIRECTORY}/ni-libraries/include" "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include" "${ASIO_DIRECTORY}/include" "./include")
target_link_libraries(hel wpi)

if(TEST MATCHES "^[Tt][Rr][Uu][Ee]" OR TEST MATCHES "^[Oo][Nn]" AND TEST_MODE MATCHES "^[Ww][Pp][Ii]")
  add_executable(FRCUserProgram tests/test_project/robot.cpp)
  target_include_directories(FRCUserProgram PRIVATE "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include")
  target_include_directories(FRCUserProgram PRIVATE "${WPILIB_DIRECTORY}/hal/src/main/native/include")
  target_include_directories(FRCUserProgram PRIVATE "${WPILIB_DIRECTORY}/wpiutil/src/main/native/include")
  set_target_properties(FRCUserProgram PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/user_code")
  target_compile_options(FRCUserProgram PUBLIC -pthread)
  target_link_libraries(FRCUserProgram wpi)
endif()
