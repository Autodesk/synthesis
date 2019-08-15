cmake_minimum_required(VERSION 3.0.2)
project(hel)

INCLUDE(ExternalProject)

option(BUILD_DOC "Build documentation" OFF)

FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/tests)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/benchmarks)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/user-code)

SET(hel_VERSION_MAJOR 1)
SET(hel_VERSION_MINOR 0)

SET(SCRIPTS_DIR "${CMAKE_SOURCE_DIR}/scripts")
SET(USER_CODE_DIR "${CMAKE_BINARY_DIR}/user-code")
SET(NILIB_DIR "${CMAKE_BINARY_DIR}/lib/ni-libraries/src/ni-libraries")
SET(WPILIB_DIR "${CMAKE_BINARY_DIR}/lib/wpilib/src/wpilib")
SET(GRPC_DIR "${CMAKE_BINARY_DIR}/lib/grpc/src/grpc")
SET(PROTOBUF_SOURCE_DIR "${CMAKE_SOURCE_DIR}/../api/v1/proto")
SET(GRPC_GEN_DIR "${CMAKE_BINARY_DIR}/lib/gen")

SET(GRPC "v1.21.4")
# SET(CTRE_DIR "${CMAKE_BINARY_DIR}/lib/ctre/src/ctre")

SET(VM_BUILD_COMMAND "./run_vm.sh")

OPTION(NO_TESTING "Disable unit tests" OFF)
OPTION(NO_ROBOT "Disable test robot code" ON)
OPTION(BENCH "Enable benchmark builds" OFF)
OPTION(X86 "Build for x86" OFF)
OPTION(BUILD_DOC "Enable building of documentation" OFF)

SET(DEFAULT_BUILD_TYPE "Release")

IF(NOT CMAKE_BUILD_TYPE AND NOT CMAKE_CONFIGURATION_TYPES)
  MESSAGE(STATUS "Setting build type to '${DEFAULT_BUILD_TYPE}' as none was specified")
  SET(CMAKE_BUILD_TYPE "${DEFAULT_BUILD_TYPE}" CACHE
    STRING "Choose the type of build." FORCE)
  SET_PROPERTY(CACHE CMAKE_BUILD_TYPE PROPERTY STRINGS
    "Debug" "Release" "MinSizeRel" "RelWithDebInfo")
ENDIF()

STRING(TOUPPER ${CMAKE_BUILD_TYPE} CMAKE_BUILD_TYPE)

SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -pedantic -Wextra -std=c++17 -DHEL_ENABLE_WARNINGS -fPIC")
IF(${HEL_DEFAULT_ENABLED_STATUS} MATCHES "(true|false)")
  SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DHEL_DEFAULT_ENABLED_STATUS=${HEL_DEFAULT_ENABLED_STATUS}")
ENDIF()
SET(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
SET(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/build)

SET(EXTERNAL_INSTALL_LOCATION ${CMAKE_BINARY_DIR}/lib)

if(CMAKE_BUILD_TYPE MATCHES "^RELEASE$" OR CMAKE_BUILD_TYPE MATCHES "^RELWITHDEBINFO$")
  if(NOT WIN32)
    SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O0 -Werror -Wno-ignored-qualifiers")
  endif()
  SET(RELEASE ON)
  SET(DEBUG FALSE)
endif()
if(CMAKE_BUILD_TYPE MATCHES "^DEBUG$" OR CMAKE_BUILD_TYPE MATCHES "^RELWITHDEBINFO$")
  SET(DEBUG ON)
  SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -g")
endif()
if(CMAKE_BUILD_TYPE MATCHES "^MINSIZEREL$")
  SET(RELEASE ON)
  SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Os")
endif()

if(NOT X86)
  SET(CMAKE_C_COMPILER arm-frc2019-linux-gnueabi-gcc)
  SET(CMAKE_CXX_COMPILER arm-frc2019-linux-gnueabi-g++)


  ExternalProject_ADD(grpc
    PREFIX lib/grpc
    GIT_REPOSITORY https://github.com/grpc/grpc.git
    GIT_TAG ${GRPC}
    CONFIGURE_COMMAND ""
    BUILD_COMMAND TOOLCHAIN=arm-frc2019-linux-gnueabi- ${SCRIPTS_DIR}/check_if_built.sh GRPC ${GRPC_DIR} "${GRPC_DIR}/libs/opt/protobuf/libprotobuf.a ${GRPC_DIR}/libs/opt/libgrpc.a ${GRPC_DIR}/libs/opt/libgrpc++.a" " ${SCRIPTS_DIR}/build_grpc.sh ${GRPC_DIR}"
    INSTALL_COMMAND "")


  LINK_DIRECTORIES(
    ${CMAKE_BINARY_DIR}/lib
    ${CMAKE_BINARY_DIR}/lib/lib
    ${NILIB_DIR}/src/lib
    ${NILIB_DIR}/src/lib/chipobject
    ${NILIB_DIR}/src/lib/netcomm
    ${GRPC_DIR}/libs/opt
    ${GRPC_DIR}/libs/opt/protobuf
    ${WPILIB_DIR}/wpilibc/build/libs/wpilibc/shared/athena/release
    ${WPILIB_DIR}/wpiutil/build/libs/wpiutil/shared/athena/release
    ${WPILIB_DIR}/ntcore/build/libs/ntcore/shared/athena/release
    ${WPILIB_DIR}/hal/build/libs/hal/shared/athena/release
    ${WPILIB_DIR}/cscore/build/libs/cscore/shared/athena/release
    ${WPILIB_DIR}/cameraserver/build/libs/cameraserver/shared/athena/release
    ${WPILIB_DIR}/build/tmp/expandedArchives/opencv-cpp-3.4.4-4-linuxathena.zip_3e38e7c98115770c2774d54027662b61/linux/athena/shared/
    # ${CTRE_DIR}/arm/cpp/build/libs/cTRE_Phoenix/static
    # ${CTRE_DIR}/libraries/driver/lib
  )
else()
  ExternalProject_ADD(grpc
    PREFIX lib/grpc
    GIT_REPOSITORY https://github.com/grpc/grpc.git
    GIT_TAG ${GRPC}
    CONFIGURE_COMMAND ""
    BUILD_COMMAND ${SCRIPTS_DIR}/check_if_built.sh GRPC ${GRPC_DIR} "${GRPC_DIR}/libs/opt/protobuf/libprotobuf.a ${GRPC_DIR}/libs/opt/libgrpc.a ${GRPC_DIR}/libs/opt/libgrpc++.a" "${SCRIPTS_DIR}/build_grpc.sh ${GRPC_DIR}"
    INSTALL_COMMAND "")

  SET(CMAKE_C_COMPILER gcc)
  SET(CMAKE_CXX_COMPILER g++)

  LINK_DIRECTORIES(
    ${CMAKE_BINARY_DIR}/lib
    ${CMAKE_BINARY_DIR}/lib/lib
    ${CMAKE_BINARY_DIR}/lib/lib64
    ${GRPC_DIR}/libs/opt
    ${GRPC_DIR}/libs/opt/protobuf
    ${NILIB_DIR}/src/lib
    ${WPILIB_DIR}/wpilibc/build/libs/wpilibc/shared/x86-64/release
    ${WPILIB_DIR}/wpiutil/build/libs/wpiutil/shared/x86-64/release
    ${WPILIB_DIR}/ntcore/build/libs/ntcore/shared/x86-64/release
    ${WPILIB_DIR}/hal/build/libs/hal/shared/x86-64/release
    ${WPILIB_DIR}/cscore/build/libs/cscore/shared/x86-64/release
    ${WPILIB_DIR}/build/tmp/expandedArchives/opencv-cpp-3.4.4-4-linuxx86-64.zip_fe8636d74ac63a8d6fc247de44ba271c/linux/x86-64/shared/
  )
endif()

ADD_CUSTOM_TARGET(build_vm
	COMMAND ${VM_BUILD_COMMAND}
	WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
	COMMENT "Download, build, and run synthesis VM"
	VERBATIM)

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
  BUILD_COMMAND ""
  INSTALL_COMMAND "")

ADD_CUSTOM_TARGET(wpilibc
  COMMAND ${SCRIPTS_DIR}/check_if_built.sh WPILib ${WPILIB_DIR} "${WPILIB_DIR}/wpilibc/build/libs/wpilibc/shared/athena/release/libwpilibc.so" "./gradlew :wpilibc:wpilibcAthenaReleaseSharedLibrary"
  WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
  COMMENT "Build WPILib for C++"
  VERBATIM)
ADD_DEPENDENCIES(wpilibc wpilib)

ADD_CUSTOM_TARGET(wpilibj
  COMMAND ${SCRIPTS_DIR}/check_if_built.sh WPILib ${WPILIB_DIR} "${WPILIB_DIR}/wpilibj/build/libs/wpilibj.jar" "./gradlew :wpilibj:assemble"
  WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
  COMMENT "Build WPILib for Java"
  VERBATIM)
ADD_DEPENDENCIES(wpilibj wpilib)

# EXTERNALPROJECT_ADD(ctre
#   PREFIX lib/ctre
#   GIT_REPOSITORY https://github.com/CrossTheRoadElec/Phoenix-frc-lib
#   GIT_TAG v2018.19.0
#   CONFIGURE_COMMAND cd ${CTRE_DIR} && chmod +x ./gradlew
#   PATCH_COMMAND cd ${CMAKE_BINARY_DIR}/lib/ctre/src/ctre/ && patch -p1 < ${CMAKE_BINARY_DIR}/external-configs/ctre.patch
#   BUILD_COMMAND ${SCRIPTS_DIR}/check_if_built.sh CRTE ${CTRE_DIR} ${CTRE_DIR}/arm/cpp/build/libs/cTRE_Phoenix/static/libCTRE_Phoenix.a "./gradlew arm:cpp:CTRE_PhoenixStaticLibrary"
#   INSTALL_COMMAND "")
# ADD_DEPENDENCIES(ctre wpilib)

FILE(GLOB PROTOBUF_SOURCE_FILES ${PROTOBUF_SOURCE_DIR}/*)
SET(GENERATED_FILES ${GRPC_GEN_DIR}/emulator_service.grpc.pb.cc ${GRPC_GEN_DIR}/emulator_service.grpc.pb.h ${GRPC_GEN_DIR}/emulator_service.pb.cc ${GRPC_GEN_DIR}/emulator_service.pb.h)

ADD_CUSTOM_COMMAND(
  OUTPUT ${GENERATED_FILES}
  COMMAND ${SCRIPTS_DIR}/gen_grpc.sh --system-protoc
  WORKING_DIRECTORY ${CMAKE_BINARY_DIR}/
  COMMENT "Generate libhel protobuf and gRPC files"
  DEPENDS ${PROTOBUF_SOURCE_FILES}
  VERBATIM)
ADD_CUSTOM_TARGET(grpc_gen DEPENDS ${GENERATED_FILES})
ADD_DEPENDENCIES(grpc_gen grpc)

ADD_LIBRARY(hel SHARED
  src/roborio.cpp
  src/roborio_manager.cpp
  src/robot_outputs.cpp
  src/robot_inputs.cpp
  src/util.cpp
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
  src/dma_manager.cpp
  src/interrupt.cpp
  src/interrupt_manager.cpp
  src/analog_trigger.cpp
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
  src/pdp.cpp
  src/robot_input_service.cpp
  src/robot_output_service.cpp
  ${GENERATED_FILES}
)
ADD_DEPENDENCIES(hel ni-libraries grpc_gen)
TARGET_LINK_LIBRARIES(hel pthread libgrpc++.a libprotobuf.a libgpr.a libgrpc.a libgrpc_cronet.a libz.a libgrpc++.a)
TARGET_COMPILE_OPTIONS(hel PUBLIC -pthread)
TARGET_INCLUDE_DIRECTORIES(hel SYSTEM PRIVATE
  "${CMAKE_SOURCE_DIR}/include"
  "${NILIB_DIR}/src/include"
  "${GRPC_DIR}/include"
  "${GRPC_DIR}/third_party/protobuf/src"
  "${GRPC_GEN_DIR}"
)

if(NOT NO_TESTING AND DEBUG)
  IF(NOT DEFINED WPILIB)
    MESSAGE(FATAL_ERROR "Please specify WPILib Version with WPILIB tag")
  ENDIF()

  if(NOT NO_ROBOT OR NOT x86)
    ADD_EXECUTABLE(FRCUserProgram tests/test_projects/robot_teleop.cpp)
    ADD_DEPENDENCIES(FRCUserProgram wpilibc ni-libraries)
    # ADD_DEPENDENCIES(ctre)
    TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE
      "${WPILIB_DIR}/cameraserver/src/main/native/include"
      "${WPILIB_DIR}/cscore/src/main/native/include"
      "${WPILIB_DIR}/ntcore/src/main/native/include"
      "${WPILIB_DIR}/wpiutil/src/main/native/include"
      "${WPILIB_DIR}/wpilibc/src/main/native/include"
      "${WPILIB_DIR}/hal/src/main/native/include"
      # "${CTRE_DIR}/cpp/include/"
      # "${CTRE_DIR}/libraries/driver/include"
      )
    SET_TARGET_PROPERTIES(FRCUserProgram PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${USER_CODE_DIR}")
    TARGET_COMPILE_OPTIONS(FRCUserProgram PUBLIC -pthread)
    TARGET_LINK_LIBRARIES(FRCUserProgram wpi pthread)
    # TARGET_LINK_LIBRARIES(libCTRE_Phoenix.a libCTRE_PhoenixCCI.a)

    # Java Test Project
    FIND_PACKAGE(Java REQUIRED)
    INCLUDE(UseJava)
    FILE(GLOB_RECURSE JAVA_SOURCES tests/test_projects/java/*.java)
    ADD_JAR(JavaFRCUserProgram ${JAVA_SOURCES} INCLUDE_JARS "${WPILIB_DIR}/wpilibj/build/libs/wpilibj.jar" OUTPUT_DIR "${USER_CODE_DIR}")
    ADD_DEPENDENCIES(JavaFRCUserProgram wpilibj ni-libraries)
  else()
    MESSAGE(WARNING "Robot code generation not supported on x86_64. Skipping test robot code generation.")
  endif()

  if(NOT NO_TESTING)
    if(NOT X86)
      EXTERNALPROJECT_ADD(GoogleTest
        PREFIX lib/google_test
        DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
        GIT_REPOSITORY https://www.github.com/google/googletest
        CMAKE_ARGS -DCMAKE_TOOLCHAIN_FILE=${CMAKE_SOURCE_DIR}/external-configs/gtest_arm.cmake -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION})
    else()
      MESSAGE(WARNING "HAL- and CTRE-based tests are not supported with the x86_64 architecture compiler flag.")
      EXTERNALPROJECT_ADD(GoogleTest
        PREFIX lib/google_test
        DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
        GIT_REPOSITORY https://www.github.com/google/googletest
        CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION})
    endif()
    MESSAGE("Disovering test files:")

    FILE(GLOB test_list ${CMAKE_SOURCE_DIR}/tests/*.cpp)
    FOREACH(file ${test_list})
      STRING(REPLACE ".cpp" "" test_name_unclean ${file})
      GET_FILENAME_COMPONENT(test_name ${test_name_unclean} NAME)

      MESSAGE("Test \"${test_name}\" found. Generating build target.")

      ADD_EXECUTABLE(${test_name} ${file} tests/main_stub.cpp)
      ADD_DEPENDENCIES(${test_name} hel GoogleTest wpilibc)

      TARGET_LINK_LIBRARIES(${test_name} libgtest.a libgtest_main.a pthread libgrpc++.a libprotobuf.a libgpr.a libgrpc.a hel)
      TARGET_COMPILE_OPTIONS(${test_name} PUBLIC -pthread)

      if(NOT X86)
        # ADD_DEPENDENCIES(${test_name} ctre)
        TARGET_LINK_LIBRARIES(${test_name} wpi) # libCTRE_Phoenix.a libCTRE_PhoenixCCI.a)
        # TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE
        #   "${CTRE_DIR}/cpp/include/"
        #   "${CTRE_DIR}/libraries/driver/include")
      else()
        TARGET_LINK_LIBRARIES(${test_name} wpi-x86)
      endif()

      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE
        "${WPILIB_DIR}/cscore/src/main/native/include"
        "${WPILIB_DIR}/ntcore/src/main/native/include"
        "${WPILIB_DIR}/wpiutil/src/main/native/include"
        "${WPILIB_DIR}/cameraserver/src/main/native/include"
        "${WPILIB_DIR}/wpilibc/src/main/native/include"
        "${WPILIB_DIR}/hal/src/main/native/include"
		"${GRPC_DIR}/include"
		"${GRPC_DIR}/third_party/protobuf/src"
		"${CMAKE_BINARY_DIR}/lib/gen"
		"${NILIB_DIR}/src/include"
        # "${CTRE_DIR}/cpp/include/"
        # "${CTRE_DIR}/libraries/driver/include"
        "${CMAKE_SOURCE_DIR}/include"
        "${CMAKE_BINARY_DIR}/include"
        "${CMAKE_BINARY_DIR}/lib/include")

      SET_TARGET_PROPERTIES(${test_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/tests")
    ENDFOREACH(file test_list)
  else()
    MESSAGE(STATUS "Skipped generation of unit tests.")
  endif()
endif()

if(BENCH AND DEBUG)
  if(NOT X86)
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

  FILE(GLOB benchmark_list ${CMAKE_SOURCE_DIR}/benchmarks/*.cpp)
  FOREACH(file ${benchmark_list})
    STRING(REPLACE ".cpp" "" benchmark_name_unclean ${file})
    GET_FILENAME_COMPONENT(benchmark_name ${benchmark_name_unclean} NAME)

    MESSAGE("Benchmark file discovered: ${benchmark_name}. Generating build target ${benchmark_name}_bench")

    ADD_EXECUTABLE(${benchmark_name} ${file})
    ADD_DEPENDENCIES(${benchmark_name} hel GoogleTest GoogleBench wpilibc)

    TARGET_LINK_LIBRARIES(${benchmark_name} gtest gtest_main pthread hel benchmark benchmark_main wpi-x86)
    TARGET_COMPILE_OPTIONS(${benchmark_name} PUBLIC -pthread)

    TARGET_INCLUDE_DIRECTORIES(${benchmark_name} SYSTEM PRIVATE
      "${WPILIB_DIR}/cscore/src/main/native/include"
      "${WPILIB_DIR}/ntcore/src/main/native/include"
      "${WPILIB_DIR}/wpiutil/src/main/native/include"
      "${WPILIB_DIR}/cameraserver/src/main/native/include"
      "${WPILIB_DIR}/wpilibc/src/main/native/include"
      "${WPILIB_DIR}/hal/src/main/native/include"
      "${GRPC_DIR}/include"
      "${GRPC_DIR}/third_party/protobuf/src"
      "${CMAKE_BINARY_DIR}/lib/gen"
      "${NILIB_DIR}/src/include"
      # "${CTRE_DIR}/cpp/include/"
      # "${CTRE_DIR}/libraries/driver/include"
      "${CMAKE_SOURCE_DIR}/include"
      "${CMAKE_BINARY_DIR}/include"
      "${CMAKE_BINARY_DIR}/lib/include")

    SET_TARGET_PROPERTIES(${benchmark_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/benchmarks")
  ENDFOREACH(file in test_list)
endif()

if(RELEASE OR BUILD_DOC)
  FIND_PACKAGE(Doxygen)
  if(DOXYGEN_FOUND)
    SET(DOXYFILE ${CMAKE_SOURCE_DIR}/docs/Doxyfile)

    ADD_CUSTOM_TARGET(doxygen ALL
      COMMAND ${DOXYGEN_EXECUTABLE} ${DOXYFILE}
      WORKING_DIRECTORY ${CMAKE_SOURCE_DIR}
      COMMENT "Generating API documentation"
      VERBATIM)
  else()
    MESSAGE("Doxygen not found; skipping")
  endif()
endif()
