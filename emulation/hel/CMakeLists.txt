cmake_minimum_required (VERSION 3.0.2)
project (hel)

INCLUDE(ExternalProject)

option(BUILD_DOC "Build documentation" OFF)

FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/bin/tests)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/build)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/docs)
FILE(MAKE_DIRECTORY ${CMAKE_BINARY_DIR}/user-code)

SET (hel_VERSION_MAJOR 1)
SET (hel_VERSION_MINOR 0)

SET (WPILIB_DIRECTORY "${CMAKE_BINARY_DIR}/lib/wpilib/src/wpilib")
SET (ASIO_DIRECTORY "${CMAKE_BINARY_DIR}/lib/ASIO/asio")
SET (CTRE_DIRECTORY "${CMAKE_BINARY_DIR}/lib/ctre/src/ctre")

SET (VM_BUILD_COMMAND "./run_vm.sh")
SET (CLONE_ASIO_COMMAND "${CMAKE_BINARY_DIR}/scripts/clone_asio.sh")

SET (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -pedantic -Wextra -std=c++14")

SET(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
SET(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/build)

SET(EXTERNAL_INSTALL_LOCATION ${CMAKE_BINARY_DIR}/lib)

FIND_PACKAGE(Doxygen)

if(CMAKE_BUILD_TYPE MATCHES "[Dd][Ee][Bb][Uu][Gg]")
  SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O0 -g")
elseif(CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]")
  SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3 -Werror")
else()
  SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3")
endif()

if(ARCH MATCHES "[Aa][Rr][Mm]")
  SET (CMAKE_C_COMPILER arm-frc-linux-gnueabi-gcc)
  SET (CMAKE_CXX_COMPILER arm-frc-linux-gnueabi-g++)

  LINK_DIRECTORIES ( ${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/ni-libraries/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/athena ${WPILIB_DIRECTORY}/build/dependencies/wpiutil-cpp/linuxathena/linux/athena/shared ${WPILIB_DIRECTORY}/build/dependencies/ntcore-cpp/linuxathena/linux/athena/shared ${WPILIB_DIRECTORY}/hal/build/libs/halAthena/shared ${WPILIB_DIRECTORY}/build/dependencies/cscore-cpp/linuxathena/linux/athena/shared ${WPILIB_DIRECTORY}/build/tmp/expandedArchives/opencv-cpp-3.2.0-linuxathena.zip_882ce6d6786024fd1378ddee15c75ec3/linux/athena/shared/ ${CTRE_DIRECTORY}/arm/cpp/build/libs/cTRE_Phoenix/static/ ${CTRE_DIRECTORY}/libraries/driver/lib/)
elseif(ARCH MATCHES "([Xx]86([-_]64)?)")

  SET (CMAKE_C_COMPILER gcc)
  SET (CMAKE_CXX_COMPILER g++)

  LINK_DIRECTORIES (${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/ni-libraries/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86-64 ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86-64 ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64 ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64 ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/x86-64 ${WPILIB_DIRECTORY}/build/tmp/expandedArchives/opencv-cpp-3.2.0-linuxx86-64.zip_999e5b59f8220bfc2a18df6ce4bd57fb/linux/x86-64/shared/)

else()
  MESSAGE(WARNING "No architecture specified; defaulting to x86.")
  SET (CMAKE_C_COMPILER gcc)
  SET (CMAKE_CXX_COMPILER g++)
  SET (ARCH "x86_64")

  LINK_DIRECTORIES (${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/ni-libraries/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86-64 ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86-64 ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64 ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64 ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/x86-64 ${WPILIB_DIRECTORY}/build/tmp/expandedArchives/opencv-cpp-3.2.0-linuxx86-64.zip_999e5b59f8220bfc2a18df6ce4bd57fb/linux/x86-64/shared/)
endif()

ADD_CUSTOM_TARGET ( build_vm
	COMMAND ${VM_BUILD_COMMAND}
	WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
	COMMENT "Download, build, and run synthesis VM"
	VERBATIM )

ADD_CUSTOM_TARGET ( asio ALL
  COMMAND ${CLONE_ASIO_COMMAND}
  WORKING_DIRECTORY ${CMAKE_BINARY_DIR}/scripts
  COMMENT "Download the C++ ASIO library"
  VERBATIM )

EXTERNALPROJECT_ADD(wpilib
  PREFIX lib/wpilib
  GIT_REPOSITORY https://www.github.com/wpilibsuite/allwpilib.git
  GIT_TAG v2018.4.1
  CONFIGURE_COMMAND ""
  BUILD_COMMAND ${CMAKE_BINARY_DIR}/scripts/check_if_built.sh ${WPILIB_DIRECTORY} ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86-64/libwpilibc.so  ./gradlew :wpilibc:build WPILib
  INSTALL_COMMAND ""
  )

EXTERNALPROJECT_ADD(ctre
  PREFIX lib/ctre
  GIT_REPOSITORY https://github.com/CrossTheRoadElec/Phoenix-frc-lib
  GIT_TAG Phoenix_v5.5.0.0
  CONFIGURE_COMMAND cd ${CTRE_DIRECTORY} && chmod +x ./gradlew
  #PATCH_COMMAND cd ${CMAKE_BINARY_DIR}/lib && patch -p0 < ${CMAKE_BINARY_DIR}/external-configs/ctre-includes.patch && patch -p0 < ${CMAKE_BINARY_DIR}/custom-project-configs/ctre-src.patch
  PATCH_COMMAND cd ${CMAKE_BINARY_DIR}/lib/ctre/src/ctre && patch -p0 < ${CMAKE_BINARY_DIR}/external-configs/gradle.patch
  BUILD_COMMAND ${CMAKE_BINARY_DIR}/scripts/check_if_built.sh ${CTRE_DIRECTORY} ${CTRE_DIRECTORY}/arm/cpp/build/libs/cTRE_Phoenix/static/libCTRE_Phoenix.a ./gradlew arm:cpp:CTRE_PhoenixStaticLibrary CTRE
  INSTALL_COMMAND ""
  )
ADD_DEPENDENCIES(ctre wpilib)

ADD_LIBRARY (hel SHARED src/roborio_manager.cpp src/init.cpp src/analog_input.cpp src/analog_output.cpp src/pwm.cpp src/digital_io.cpp src/can.cpp src/relay.cpp src/global.cpp src/comm.cpp src/counter.cpp src/accelerometer.cpp src/accumulator.cpp src/encoder.cpp src/power.cpp src/sys_watchdog.cpp src/spi.cpp src/net_comm.cpp src/mxp_data.cpp src/error.cpp src/util.cpp src/json_util.cpp src/send_data.cpp src/receive_data.cpp src/sync_server.cpp src/alarm.cpp src/system.cpp src/sync_client.cpp src/interrupt_manager.cpp src/analog_input_calibration.cpp src/joystick.cpp src/match_info.cpp src/robot_mode.cpp)
ADD_DEPENDENCIES(hel asio wpilib) 

TARGET_INCLUDE_DIRECTORIES( hel SYSTEM PRIVATE "${WPILIB_DIRECTORY}/hal/src/main/native" "${WPILIB_DIRECTORY}/build/dependencies/wpiutil-cpp/headers" "${WPILIB_DIRECTORY}/hal/src/main/native/include" "${WPILIB_DIRECTORY}/ni-libraries/include" "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include" "${ASIO_DIRECTORY}/include" "./include")
if(ARCH MATCHES "[Aa][Rr][Mm]")
  TARGET_LINK_LIBRARIES(hel wpi)
else()
  TARGET_LINK_LIBRARIES(hel wpi-x86)
endif()


if(TESTING MATCHES "^[Tt][Rr][Uu][Ee]" OR TESTING MATCHES "^[Oo][Nn]")
   LINK_DIRECTORIES("lib/lib64")
   LINK_DIRECTORIES("lib/lib")

  if(NOT NO_ROBOT MATCHES "[Tt][Rr][Uu][Ee]" OR NOT NO_ROBOT MATCHES "[Oo][Nn]")
    if(NOT ARCH MATCHES "([Xx]86([-_]64)?)")
      ADD_EXECUTABLE(FRCUserProgram tests/test_projects/robot_can.cpp)
      ADD_DEPENDENCIES(FRCUserProgram wpilib ctre)
      TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include")
      TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/build/dependencies/ntcore-cpp/headers")
      TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/build/dependencies/cscore-cpp/headers")
      TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/hal/src/main/native/include")
      TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/build/dependencies/wpiutil-cpp/headers")
      TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE "${CTRE_DIRECTORY}/cpp/include/")
      TARGET_INCLUDE_DIRECTORIES(FRCUserProgram SYSTEM PRIVATE "${CTRE_DIRECTORY}/libraries/driver/include")
      SET_TARGET_PROPERTIES(FRCUserProgram PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/user-code")
      TARGET_COMPILE_OPTIONS(FRCUserProgram PUBLIC -pthread)
      TARGET_LINK_LIBRARIES(FRCUserProgram wpi pthread)
      TARGET_LINK_LIBRARIES(FRCUserProgram libCTRE_Phoenix.a libCTRE_PhoenixCCI.a)
    else()
      MESSAGE(WARNING "Robot code generation not supported on x86_64.")
    endif()
  else()
    MESSAGE(STATUS "Skipping test robot code generation.")
  endif()

  if(NOT NO_UNIT MATCHES "[Tt][Rr][Uu][Ee]" OR NOT NO_UNIT MATCHES "[Oo][Nn]")
    if(ARCH MATCHES "[Aa][Rr][Mm]")
    EXTERNALPROJECT_ADD(GoogleTest
      PREFIX lib/google_test
      DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
      GIT_REPOSITORY https://www.github.com/google/googletest
      CMAKE_ARGS -DCMAKE_TOOLCHAIN_FILE=${CMAKE_BINARY_DIR}/external-configs/gtest_arm.cmake -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION}
      )
    elseif()
      EXTERNALPROJECT_ADD(GoogleTest
        PREFIX lib/google_test
        DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
        GIT_REPOSITORY https://www.github.com/google/googletest
        CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION}
        )
    endif()

   MESSAGE("Disovering test files:")

    FILE(GLOB test_list tests/*.cpp)
    FOREACH(file ${test_list})
      STRING(REPLACE ".cpp" "" test_name_unclean ${file})
      GET_FILENAME_COMPONENT( test_name ${test_name_unclean} NAME)

      MESSAGE("Test file discovered: ${test_name}. Generating build target ${test_name}_test")

      ADD_EXECUTABLE( ${test_name} ${file} tests/main_stub.cpp)

      ADD_DEPENDENCIES(${test_name} hel)
      ADD_DEPENDENCIES(${test_name} GoogleTest)
      ADD_DEPENDENCIES(${test_name} asio)
      ADD_DEPENDENCIES(${test_name} wpilib)
      ADD_DEPENDENCIES(${test_name} ctre)

      LINK_DIRECTORIES (${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86_64 ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86_64 ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64 ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64 )

      TARGET_LINK_LIBRARIES(${test_name} gtest gtest_main pthread hel wpi libCTRE_Phoenix.a libCTRE_PhoenixCCI.a)
      INCLUDE_DIRECTORIES(SYSTEM "lib/include")

      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${CMAKE_BINARY_DIR}/include")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${WPILIB_DIRECTORY}/build/dependencies/ntcore-cpp/headers")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${WPILIB_DIRECTORY}/build/dependencies/cscore-cpp/headers")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${WPILIB_DIRECTORY}/hal/src/main/native/include")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${WPILIB_DIRECTORY}/build/dependencies/wpiutil-cpp/headers")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${WPILIB_DIRECTORY}/ni-libraries/include")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${CTRE_DIRECTORY}/cpp/include/")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${CTRE_DIRECTORY}/libraries/driver/include")
      TARGET_INCLUDE_DIRECTORIES(${test_name} SYSTEM PRIVATE "${ASIO_DIRECTORY}/include")

      SET_TARGET_PROPERTIES( ${test_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/tests")

      #TARGET_LINK_LIBRARIES(${test_name} hel)

    ENDFOREACH( file test_list)

  else()
    MESSAGE(STATUS "Skipped generation of unit tests.")
  endif()
endif()

if(BENCHMARK MATCHES "^[Tt][Rr][Uu][Ee]" OR BENCHMARK MATCHES "^[Oo][Nn]")
  if(ARCH MATCHES "[Aa][Rr][Mm]")
    MESSAGE(WARNING "Benchmarks not supported in ARM mode. If you wish to run benchmarks, please append -DARCH=x86_64 to cmake command")
    return()
  else()
    EXTERNALPROJECT_ADD(GoogleBench
      PREFIX lib/GoogleBench
      GIT_REPOSITORY https://github.com/google/benchmark
      CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION}
      )
  endif()

  MESSAGE("Discovering benchmark files")

  FILE(GLOB benchmark_list benchmarks/*.cpp)
  FOREACH(file ${benchmark_list})
    STRING(REPLACE ".cpp" "" benchmark_name_unclean ${file})
    GET_FILENAME_COMPONENT( benchmark_name ${benchmark_name_unclean} NAME)

    MESSAGE("Benchmark file discovered: ${benchmark_name}. Generating build target ${benchmark_name}_bench")

    ADD_EXECUTABLE( ${benchmark_name} ${file})

    ADD_DEPENDENCIES(${benchmark_name} GoogleBench)
    ADD_DEPENDENCIES(${benchmark_name} hel)

    TARGET_LINK_LIBRARIES(${benchmark_name} benchmark pthread)

    SET_TARGET_PROPERTIES( ${benchmark_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/benchmarks")

  ENDFOREACH(file in test_list)

endif()

if(DOXYGEN_FOUND AND CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]" OR BUILD_DOC MATCHES "^[Oo][Nn]" OR BUILD_DOC MATCHES "^[Tt][Rr][Uu][Ee]")

  SET(DOXYFILE ${CMAKE_BINARY_DIR}/docs/Doxyfile)

  ADD_CUSTOM_TARGET( doxygen ALL
    COMMAND ${DOXYGEN_EXECUTABLE} ${DOXYFILE}
    WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
    COMMENT "Generating API documentation"
    VERBATIM )

else()
  MESSAGE("Doxygen not found; skipping")
endif()
