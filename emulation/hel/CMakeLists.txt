cmake_minimum_required(VERSION 3.0.2)
project(hel)

INCLUDE(ExternalProject)

option(BUILD_DOC "Build documentation" OFF)

FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin/tests)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin/benchmarks)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/user-code)

SET(hel_VERSION_MAJOR 1)
SET(hel_VERSION_MINOR 0)

SET(NILIB_DIRECTORY "${CMAKE_BINARY_DIR}/lib/ni-libraries/src/ni-libraries")
SET(WPILIB_DIRECTORY "${CMAKE_BINARY_DIR}/lib/wpilib/src/wpilib")
SET(ASIO_DIRECTORY "${CMAKE_BINARY_DIR}/lib/ASIO/asio")
SET(GRPC_DIRECTORY "${CMAKE_BINARY_DIR}/lib/grpc/src/grpc")

SET(GRPC "v1.21.4")
# SET(CTRE_DIRECTORY "${CMAKE_BINARY_DIR}/lib/ctre/src/ctre")

SET(VM_BUILD_COMMAND "./run_vm.sh")
SET(CLONE_ASIO_COMMAND "${CMAKE_BINARY_DIR}/scripts/clone_asio.sh")

SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -pedantic -Wextra -std=c++17 -DHEL_ENABLE_WARNINGS")

SET(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
SET(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/build)

SET(EXTERNAL_INSTALL_LOCATION ${CMAKE_BINARY_DIR}/lib)

if(CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]")
  SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O0 -Werror -Wno-ignored-qualifiers")
else()
  if(NOT CMAKE_BUILD_TYPE MATCHES "^[Dd][Ee][Bb][Uu][Gg]") #Note: do not compile with optimizations
    SET(CMAKE_BUILD_TYPE DEBUG)
    MESSAGE(WARNING "No build type specified; defaulting to debug.")
  endif()
  SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O0 -g")
endif()

if(ARCH MATCHES "^[Aa][Rr][Mm]")
  SET(CMAKE_C_COMPILER arm-frc2019-linux-gnueabi-gcc)
  SET(CMAKE_CXX_COMPILER arm-frc2019-linux-gnueabi-g++)

  LINK_DIRECTORIES(
    ${CMAKE_BINARY_DIR}/lib
    ${CMAKE_BINARY_DIR}/lib/lib
    ${NILIB_DIRECTORY}/src/lib
    ${NILIB_DIRECTORY}/src/lib/chipobject
    ${NILIB_DIRECTORY}/src/lib/netcomm
    ${GRPC_DIRECTORY}/libs/opt
    ${GRPC_DIRECTORY}/libs/opt/protobuf
    ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/athena/release
    ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/athena/release
    ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/athena/release
    ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/athena/release
    ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/athena/release
    ${WPILIB_DIRECTORY}/cameraserver/build/libs/cameraserver/shared/athena/release
    ${WPILIB_DIRECTORY}/build/tmp/expandedArchives/opencv-cpp-3.4.4-4-linuxathena.zip_3e38e7c98115770c2774d54027662b61/linux/athena/shared/
    # ${CTRE_DIRECTORY}/arm/cpp/build/libs/cTRE_Phoenix/static
    # ${CTRE_DIRECTORY}/libraries/driver/lib
  )
else()
  if(NOT ARCH MATCHES "([Xx]86([-_]64)?)")
    MESSAGE(WARNING "No architecture specified; defaulting to x86.")
    SET(ARCH "x86_64")
  endif()
  SET(CMAKE_C_COMPILER gcc)
  SET(CMAKE_CXX_COMPILER g++)

  LINK_DIRECTORIES(
    ${CMAKE_BINARY_DIR}/lib
    ${CMAKE_BINARY_DIR}/lib/lib
    ${CMAKE_BINARY_DIR}/lib/lib64
    ${NILIB_DIRECTORY}/src/lib
    ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86-64/release
    ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86-64/release
    ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64/release
    ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64/release
    ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/x86-64/release
    ${WPILIB_DIRECTORY}/build/tmp/expandedArchives/opencv-cpp-3.4.4-4-linuxx86-64.zip_fe8636d74ac63a8d6fc247de44ba271c/linux/x86-64/shared/
  )
endif()

ADD_CUSTOM_TARGET(build_vm
	COMMAND ${VM_BUILD_COMMAND}
	WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
	COMMENT "Download, build, and run synthesis VM"
	VERBATIM)

ADD_CUSTOM_TARGET(asio ALL
  COMMAND ${CLONE_ASIO_COMMAND}
  WORKING_DIRECTORY ${CMAKE_BINARY_DIR}/scripts
  COMMENT "Download the C++ ASIO library"
  VERBATIM)

ADD_CUSTOM_TARGET(gen_grpc
  COMMAND ${CMD_GEN_GRPC}
  WORKING_DIRECTORY ${CMAKE_BINARY_DIR}/
  COMMENT "Generate libhel protobuf and gRPC files"
  VERBATIM)
ADD_DEPENDENCIES(gen_grpc grpc)

EXTERNALPROJECT_ADD(ni-libraries
  PREFIX lib/ni-libraries
  GIT_REPOSITORY https://github.com/wpilibsuite/ni-libraries.git
  GIT_TAG ${NILIB}
  CONFIGURE_COMMAND ""
  BUILD_COMMAND ""
  INSTALL_COMMAND "")

EXTERNALPROJECT_ADD(wpilib
  PREFIX lib/wpilib
  GIT_REPOSITORY https://github.com/wpilibsuite/allwpilib.git
  GIT_TAG ${WPILIB}
  CONFIGURE_COMMAND ""
  BUILD_COMMAND ${CMAKE_BINARY_DIR}/scripts/check_if_built.sh WPILib ${WPILIB_DIRECTORY} ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/athena/release/libwpilibc.so ./gradlew build
  INSTALL_COMMAND "")
# EXTERNALPROJECT_ADD(ctre
#   PREFIX lib/ctre
#   GIT_REPOSITORY https://github.com/CrossTheRoadElec/Phoenix-frc-lib
#   GIT_TAG v2018.19.0
#   CONFIGURE_COMMAND cd ${CTRE_DIRECTORY} && chmod +x ./gradlew
#   PATCH_COMMAND cd ${CMAKE_BINARY_DIR}/lib/ctre/src/ctre/ && patch -p1 < ${CMAKE_BINARY_DIR}/external-configs/ctre.patch
#   BUILD_COMMAND ${CMAKE_BINARY_DIR}/scripts/check_if_built.sh CRTE ${CTRE_DIRECTORY} ${CTRE_DIRECTORY}/arm/cpp/build/libs/cTRE_Phoenix/static/libCTRE_Phoenix.a ./gradlew arm:cpp:CTRE_PhoenixStaticLibrary
#   INSTALL_COMMAND "")
# ADD_DEPENDENCIES(ctre wpilib)

ExternalProject_ADD(grpc
  PREFIX lib/grpc
  GIT_REPOSITORY https://github.com/grpc/grpc.git
  GIT_TAG ${GRPC}
  CONFIGURE_COMMAND ""
  #BUILD_COMMAND sudo ${CMAKE_BINARY_DIR}/scripts/build_grpc.sh ${GRPC_DIRECTORY}
  BUILD_COMMAND ${CMAKE_BINARY_DIR}/scripts/check_if_built.sh GRPC ${GRPC_DIRECTORY} ${GRPC_DIRECTORY}/libs/opt/libgrpc.a sudo ${CMAKE_BINARY_DIR}/scripts/build_grpc.sh ${GRPC_DIRECTORY}
  INSTALL_COMMAND "")

ADD_LIBRARY(hel SHARED
  src/roborio.cpp
  src/roborio_manager.cpp
  src/send_data.cpp
  src/receive_data.cpp
  src/sync_server.cpp
  src/sync_client.cpp
  src/util.cpp
  src/json_util.cpp
  src/init.cpp
  src/analog_input.cpp
  src/analog_output.cpp
  src/pwm_system.cpp
  src/digital_system.cpp
  src/relay_system.cpp
  src/global.cpp
  src/comm.cpp
  src/counter.cpp
  src/accelerometer.cpp
  src/accumulator.cpp
  src/fpga_encoder.cpp
  src/power.cpp
  src/sys_watchdog.cpp
  src/spi_system.cpp
  src/net_comm.cpp
  src/mxp_data.cpp
  src/error.cpp
  src/alarm.cpp
  src/system.cpp
  src/interrupt_manager.cpp
  src/analog_input_calibration.cpp
  src/joystick.cpp
  src/match_info.cpp
  src/robot_mode.cpp
  src/encoder_manager.cpp
  src/system_interface.cpp
  src/pcm.cpp
  src/can_device.cpp
  src/can_motor_controller.cpp
  src/can_comm.cpp
  src/pdp.cpp)
  lib/gen/emulator_service.pb.cc
  lib/gen/emulator_service.grpc.pb.cc
  src/robot_input_service.cpp
  src/robot_output_service.cpp
)
ADD_DEPENDENCIES(hel asio ni-libraries gen_grpc)

TARGET_INCLUDE_DIRECTORIES(hel SYSTEM PRIVATE
  "${CMAKE_BINARY_DIR}/include"
  "${ASIO_DIRECTORY}/include"
  "${NILIB_DIRECTORY}/src/include"
  "${GRPC_DIRECTORY}/include"
  "${GRPC_DIRECTORY}/third_party/protobuf/src"
  "${CMAKE_BINARY_DIR}/lib/gen"
)

if((TESTING MATCHES "^[Tt][Rr][Uu][Ee]" OR TESTING MATCHES "^[Oo][Nn]") AND CMAKE_BUILD_TYPE MATCHES "^[Dd][Ee][Bb][Uu][Gg]")
  IF(NOT DEFINED WPILIB)
    MESSAGE(FATAL_ERROR "Please specify WPILib Version with WPILIB tag")
  ENDIF()

  if(NOT NO_ROBOT MATCHES "[Tt][Rr][Uu][Ee]" OR NOT NO_ROBOT MATCHES "[Oo][Nn]" OR NOT ARCH MATCHES "([Xx]86([-_]64)?)")
    ADD_EXECUTABLE(FRCUserProgram tests/test_projects/robot_teleop.cpp)
    ADD_DEPENDENCIES(FRCUserProgram wpilib ni-libraries)
    # ADD_DEPENDENCIES(ctre)
    TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE
      "${WPILIB_DIRECTORY}/cameraserver/src/main/native/include"
      "${WPILIB_DIRECTORY}/cscore/src/main/native/include"
      "${WPILIB_DIRECTORY}/ntcore/src/main/native/include"
      "${WPILIB_DIRECTORY}/wpiutil/src/main/native/include"
      "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include"
      "${WPILIB_DIRECTORY}/hal/src/main/native/include"
      # "${CTRE_DIRECTORY}/cpp/include/"
      # "${CTRE_DIRECTORY}/libraries/driver/include"
      )
    SET_TARGET_PROPERTIES(FRCUserProgram PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/user-code")
    TARGET_COMPILE_OPTIONS(FRCUserProgram PUBLIC -pthread)
    TARGET_LINK_LIBRARIES(FRCUserProgram wpi pthread)
    # TARGET_LINK_LIBRARIES(libCTRE_Phoenix.a libCTRE_PhoenixCCI.a)
  else()
    MESSAGE(WARNING "Robot code generation not supported on x86_64. Skipping test robot code generation.")
  endif()

  if(NOT NO_UNIT MATCHES "[Tt][Rr][Uu][Ee]" OR NOT NO_UNIT MATCHES "[Oo][Nn]")
    if(ARCH MATCHES "^[Aa][Rr][Mm]")
      EXTERNALPROJECT_ADD(GoogleTest
        PREFIX lib/google_test
        DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
        GIT_REPOSITORY https://www.github.com/google/googletest
        CMAKE_ARGS -DCMAKE_TOOLCHAIN_FILE=${CMAKE_BINARY_DIR}/external-configs/gtest_arm.cmake -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION})
    else()
      MESSAGE(WARNING "HAL- and CTRE-based tests are not supported with the x86_64 architecture compiler flag.")
      EXTERNALPROJECT_ADD(GoogleTest
        PREFIX lib/google_test
        DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
        GIT_REPOSITORY https://www.github.com/google/googletest
        CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION})
    endif()
    MESSAGE("Disovering test files:")

    FILE(GLOB test_list tests/*.cpp)
    FOREACH(file ${test_list})
      STRING(REPLACE ".cpp" "" test_name_unclean ${file})
      GET_FILENAME_COMPONENT(test_name ${test_name_unclean} NAME)

      MESSAGE("Test \"${test_name}\" found. Generating build target.")

      ADD_EXECUTABLE(${test_name} ${file} tests/main_stub.cpp)
      ADD_DEPENDENCIES(${test_name} hel GoogleTest asio wpilib)

      TARGET_LINK_LIBRARIES(${test_name} libgtest.a libgtest_main.a pthread libgrpc_plugin_support.a libprotobuf.a libgpr.a libgrpc.a hel)
      TARGET_COMPILE_OPTIONS(${test_name} PUBLIC -pthread)

      if(ARCH MATCHES "^[Aa][Rr][Mm]")
        # ADD_DEPENDENCIES(${test_name} ctre)
        TARGET_LINK_LIBRARIES(${test_name} wpi) # libCTRE_Phoenix.a libCTRE_PhoenixCCI.a)
        # TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE
        #   "${CTRE_DIRECTORY}/cpp/include/"
        #   "${CTRE_DIRECTORY}/libraries/driver/include")
      else()
        TARGET_LINK_LIBRARIES(${test_name} wpi-x86)
      endif()

      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE
        "${WPILIB_DIRECTORY}/cscore/src/main/native/include"
        "${WPILIB_DIRECTORY}/ntcore/src/main/native/include"
        "${WPILIB_DIRECTORY}/wpiutil/src/main/native/include"
        "${WPILIB_DIRECTORY}/cameraserver/src/main/native/include"
        "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include"
        "${WPILIB_DIRECTORY}/hal/src/main/native/include"
  	"${GRPC_DIRECTORY}/include"
	"${GRPC_DIRECTORY}/third_party/protobuf/src"
  	"${CMAKE_BINARY_DIR}/lib/gen"
	"${NILIB_DIRECTORY}/src/include"
        # "${CTRE_DIRECTORY}/cpp/include/"
        # "${CTRE_DIRECTORY}/libraries/driver/include"
        "${CMAKE_BINARY_DIR}/include"
        "${ASIO_DIRECTORY}/include"
        "${CMAKE_BINARY_DIR}/lib/include")

      SET_TARGET_PROPERTIES(${test_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/tests")
    ENDFOREACH(file test_list)
  else()
    MESSAGE(STATUS "Skipped generation of unit tests.")
  endif()
endif()

if((BENCHMARK MATCHES "^[Tt][Rr][Uu][Ee]" OR BENCHMARK MATCHES "^[Oo][Nn]") AND CMAKE_BUILD_TYPE MATCHES "^[Dd][Ee][Bb][Uu][Gg]")
  if(ARCH MATCHES "^[Aa][Rr][Mm]")
    MESSAGE(WARNING "Benchmarks not supported in ARM mode. If you wish to run benchmarks, please use -DARCH=x86_64")
    return()
  else()
    MESSAGE(WARNING "HAL- and CTRE-based benchmarks are not supported with the x86_64 architecture compiler flag.")
    EXTERNALPROJECT_ADD(GoogleBench
      PREFIX lib/GoogleBench
      GIT_REPOSITORY https://github.com/google/benchmark
      CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION})
  endif()

  MESSAGE("Discovering benchmark files")

  FILE(GLOB benchmark_list benchmarks/*.cpp)
  FOREACH(file ${benchmark_list})
    STRING(REPLACE ".cpp" "" benchmark_name_unclean ${file})
    GET_FILENAME_COMPONENT(benchmark_name ${benchmark_name_unclean} NAME)

    MESSAGE("Benchmark file discovered: ${benchmark_name}. Generating build target ${benchmark_name}_bench")

    ADD_EXECUTABLE(${benchmark_name} ${file})
    ADD_DEPENDENCIES(${benchmark_name} hel GoogleTest GoogleBench asio wpilib)

    TARGET_LINK_LIBRARIES(${benchmark_name} gtest gtest_main pthread hel benchmark benchmark_main wpi-x86)
    TARGET_COMPILE_OPTIONS(${benchmark_name} PUBLIC -pthread)

    TARGET_INCLUDE_DIRECTORIES(${benchmark_name} SYSTEM PRIVATE
      "${WPILIB_DIRECTORY}/cscore/src/main/native/include"
      "${WPILIB_DIRECTORY}/ntcore/src/main/native/include"
      "${WPILIB_DIRECTORY}/wpiutil/src/main/native/include"
      "${WPILIB_DIRECTORY}/cameraserver/src/main/native/include"
      "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include"
      "${WPILIB_DIRECTORY}/hal/src/main/native/include"
      "${GRPC_DIRECTORY}/include"
      "${GRPC_DIRECTORY}/third_party/protobuf/src"
      "${CMAKE_BINARY_DIR}/lib/gen"
      "${NILIB_DIRECTORY}/src/include"
      # "${CTRE_DIRECTORY}/cpp/include/"
      # "${CTRE_DIRECTORY}/libraries/driver/include"
      "${CMAKE_BINARY_DIR}/include"
      "${ASIO_DIRECTORY}/include"
      "${CMAKE_BINARY_DIR}/include"
      "${CMAKE_BINARY_DIR}/lib/include")

    SET_TARGET_PROPERTIES(${benchmark_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/benchmarks")
  ENDFOREACH(file in test_list)
endif()

if(CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]" OR BUILD_DOC MATCHES "^[Oo][Nn]" OR BUILD_DOC MATCHES "^[Tt][Rr][Uu][Ee]")
  FIND_PACKAGE(Doxygen)
  if(DOXYGEN_FOUND)
    SET(DOXYFILE ${CMAKE_BINARY_DIR}/docs/Doxyfile)

    ADD_CUSTOM_TARGET(doxygen ALL
      COMMAND ${DOXYGEN_EXECUTABLE} ${DOXYFILE}
      WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
      COMMENT "Generating API documentation"
      VERBATIM)
  else()
    MESSAGE("Doxygen not found; skipping")
  endif()
endif()
