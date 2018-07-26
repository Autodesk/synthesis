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

set (WPILIB_DIRECTORY "${CMAKE_BINARY_DIR}/lib/wpilib/src/wpilib")
set (ASIO_DIRECTORY "${CMAKE_BINARY_DIR}/lib/ASIO/asio")
set (CTRE_DIRECTORY "${CMAKE_BINARY_DIR}/lib/ctre/src/ctre")

set (VM_BUILD_COMMAND "./run_vm.sh")
set (CLONE_ASIO_COMMAND "${CMAKE_BINARY_DIR}/scripts/clone_asio.sh")

set (CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -pedantic -Wextra -std=c++14")

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/build)

set(EXTERNAL_INSTALL_LOCATION ${CMAKE_BINARY_DIR}/lib)

find_package(Doxygen)

if(CMAKE_BUILD_TYPE MATCHES "[Dd][Ee][Bb][Uu][Gg]")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O0 -g")
elseif(CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]")
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3 -Werror")
else()
  set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O3")
endif()

add_custom_target ( generate_libwpi
	COMMAND bash -c "g++ ${CMAKE_BINARY_DIR}/src/generate_libwpi.cpp -o build/generate_libwpi && ./build/generate_libwpi"
	WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
	COMMENT "Generate libwpi.so object file"
	VERBATIM )

if(ARCH MATCHES "[Aa][Rr][Mm]")
  set (CMAKE_C_COMPILER arm-frc-linux-gnueabi-gcc)
  set (CMAKE_CXX_COMPILER arm-frc-linux-gnueabi-g++)

  if(NOT NO_UNIT MATCHES "[Tt][Rr][Uu][Ee]" OR NOT NO_UNIT MATCHES "[Oo][Nn]")
    set(NO_UNIT "TRUE")
  endif()

  add_custom_command(OUTPUT ${CMAKE_BINARY_DIR}/lib/libwpi.so
    COMMAND ./build/generate_libwpi
    WORKING_DIRECTORY ${CMAKE_BINARY_DIR}/build
    DEPENDS generate_libwpi
    VERBATIM)

  link_directories ( ${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/ni-libraries/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/athena ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/athena ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/athena ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/athena ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/athena ${WPILIB_DIRECTORY}/cameraserver/build/libs/cameraserver/shared/athena/ ${WPILIB_DIRECTORY}/build/tmp/expandedArchives/opencv-cpp-3.2.0-linuxathena.zip_882ce6d6786024fd1378ddee15c75ec3/linux/athena/shared/ ${CTRE_DIRECTORY}/arm/cpp/build/libs/cTRE_Phoenix/static/ ${CTRE_DIRECTORY}/libraries/driver/lib/)
elseif(ARCH MATCHES "([Xx]86([-_]64)?)")

  set (CMAKE_C_COMPILER gcc)
  set (CMAKE_CXX_COMPILER g++)
  add_custom_command(OUTPUT ${CMAKE_BINARY_DIR}/lib/libwpi.so
    COMMAND ./build/generate_libwpi
    WORKING_DIRECTORY ${CMAKE_BINARY_DIR}/build
    DEPENDS generate_libwpi
    VERBATIM)

  link_directories (${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/ni-libraries/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86-64 ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86-64 ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64 ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64 ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/x86-64 ${WPILIB_DIRECTORY}/cameraserver/build/libs/cameraserver/shared/x86-64/ ${WPILIB_DIRECTORY}/build/tmp/expandedArchives/opencv-cpp-3.2.0-linuxx86-64.zip_999e5b59f8220bfc2a18df6ce4bd57fb/linux/x86-64/shared/)

else()
  message(WARNING "No architecture specified; defaulting to x86.")
  set (CMAKE_C_COMPILER gcc)
  set (CMAKE_CXX_COMPILER g++)
  set (ARCH "x86_64")
  add_custom_command(OUTPUT ${CMAKE_BINARY_DIR}/lib/libwpi.so x86
    COMMAND ./build/generate_libwpi
    DEPENDS generate_libwpi
    VERBATIM)

  link_directories (${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/ni-libraries/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86-64 ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86-64 ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64 ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64 ${WPILIB_DIRECTORY}/cscore/build/libs/cscore/shared/x86-64 ${WPILIB_DIRECTORY}/cameraserver/build/libs/cameraserver/shared/x86-64/ ${WPILIB_DIRECTORY}/build/tmp/expandedArchives/opencv-cpp-3.2.0-linuxx86-64.zip_999e5b59f8220bfc2a18df6ce4bd57fb/linux/x86-64/shared/)
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

ExternalProject_Add(wpilib
  PREFIX lib/wpilib
  GIT_REPOSITORY https://www.github.com/wpilibsuite/allwpilib.git
  CONFIGURE_COMMAND ""
  BUILD_COMMAND ${CMAKE_BINARY_DIR}/scripts/check_if_built.sh ${WPILIB_DIRECTORY} ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86-64/libwpilibc.so  ./gradlew :wpilibc:build
  INSTALL_COMMAND ""
  )

ExternalProject_Add(ctre
  PREFIX lib/ctre
  GIT_REPOSITORY https://github.com/CrossTheRoadElec/Phoenix-frc-lib
  CONFIGURE_COMMAND cd ${CTRE_DIRECTORY} && chmod +x ./gradlew
  BUILD_COMMAND ${CMAKE_BINARY_DIR}/scripts/check_if_built.sh  ${CTRE_DIRECTORY} {$CTRE_DIRECTORY}/arm/cpp/build/libs/cTRE_Phoenix/static/libCTRE_Phoenix.a   ./gradlew buildneeded
  INSTALL_COMMAND ""
  )

add_library (hel SHARED src/init.cpp src/analog_input.cpp src/analog_output.cpp src/pwm.cpp src/digital_io.cpp src/can.cpp src/relay.cpp src/global.cpp src/comm.cpp src/counter.cpp src/accelerometer.cpp src/accumulator.cpp src/encoder.cpp src/power.cpp src/sys_watchdog.cpp src/spi.cpp src/net_comm.cpp src/mxp_data.cpp src/error.cpp src/util.cpp src/json_util.cpp src/send_data.cpp src/receive_data.cpp src/sync_server.cpp src/alarm.cpp src/system.cpp src/sync_client.cpp)
add_dependencies(hel asio wpilib generate_libwpi)

target_include_directories( hel SYSTEM PRIVATE "${WPILIB_DIRECTORY}/hal/src/main/native" "${WPILIB_DIRECTORY}/wpiutil/src/main/native/include" "${WPILIB_DIRECTORY}/hal/src/main/native/include" "${WPILIB_DIRECTORY}/ni-libraries/include" "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include" "${ASIO_DIRECTORY}/include" "./include")
target_link_libraries(hel wpi)


if(TESTING MATCHES "^[Tt][Rr][Uu][Ee]" OR TESTING MATCHES "^[Oo][Nn]")
   link_directories("lib/lib64")

  if(NOT NO_ROBOT MATCHES "[Tt][Rr][Uu][Ee]" OR NOT NO_ROBOT MATCHES "[Oo][Nn]")
    if(NOT ARCH MATCHES "([Xx]86([-_]64)?)")
      add_executable(FRCUserProgram tests/test_projects/robot_teleop.cpp)
      add_dependencies(FRCUserProgram wpilib generate_libwpi ctre)
      target_include_directories(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include")
      target_include_directories(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/cameraserver/src/main/native/include")
      target_include_directories(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/cscore/src/main/native/include")
      target_include_directories(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/ntcore/src/main/native/include")
      target_include_directories(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/hal/src/main/native/include")
      target_include_directories(FRCUserProgram SYSTEM PRIVATE "${WPILIB_DIRECTORY}/wpiutil/src/main/native/include")
      target_include_directories(FRCUserProgram SYSTEM PRIVATE "${CTRE_DIRECTORY}/cpp/include/")
      set_target_properties(FRCUserProgram PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/user_code")
      target_compile_options(FRCUserProgram PUBLIC -pthread)
      target_link_libraries(FRCUserProgram wpi)
      target_link_libraries(FRCUserProgram libCTRE_Phoenix.a libCTRE_PhoenixCCI.a)
    else()
      message(WARNING "Robot code generation not supported on x86_64.")
    endif()
  else()
    message(STATUS "Skipping test robot code generation.")
  endif()

  if(NOT NO_UNIT MATCHES "[Tt][Rr][Uu][Ee]" OR NOT NO_UNIT MATCHES "[Oo][Nn]")
    if(ARCH MATCHES "[Aa][Rr][Mm]")
      message(WARNING "Unit tests not currently available in ARM configuration")
      return()
    endif()
    ExternalProject_Add(GoogleTest
      PREFIX lib/google_test
      DOWNLOAD_DIR ${EXTERNAL_INSTALL_LOCATION}
      GIT_REPOSITORY https://www.github.com/google/googletest
      CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION}
      )

   MESSAGE("Disovering test files:")

    file(GLOB test_list tests/*.cpp)
    FOREACH(file ${test_list})
      string(REPLACE ".cpp" "" test_name_unclean ${file})
      get_filename_component( test_name ${test_name_unclean} NAME)

      MESSAGE("Test file discovered: ${test_name}. Generating build target ${test_name}_test")

      add_executable( ${test_name} ${file} tests/main_stub.cpp)

      add_dependencies(${test_name} hel)
      add_dependencies(${test_name} GoogleTest)
      add_dependencies(${test_name} asio)
      add_dependencies(${test_name} wpilib)

      link_directories (${CMAKE_BINARY_DIR}/lib ${WPILIB_DIRECTORY}/wpilibc/build/libs/wpilibc/shared/x86_64 ${WPILIB_DIRECTORY}/wpiutil/build/libs/wpiutil/shared/x86_64 ${WPILIB_DIRECTORY}/ntcore/build/libs/ntcore/shared/x86-64 ${WPILIB_DIRECTORY}/hal/build/libs/hal/shared/x86-64 )

      target_link_libraries(${test_name} gtest gtest_main pthread hel)
      include_directories(SYSTEM "lib/include")
      target_include_directories(${test_name} SYSTEM PRIVATE "${ASIO_DIRECTORY}/include" "${WPILIB_DIRECTORY}/hal/src/main/native" "${WPILIB_DIRECTORY}/wpiutil/src/main/native/include" "${WPILIB_DIRECTORY}/hal/src/main/native/include" "${WPILIB_DIRECTORY}/ni-libraries/include" "${WPILIB_DIRECTORY}/wpilibc/src/main/native/include" "./include")

      set_target_properties( ${test_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/tests")

      #target_link_libraries(${test_name} hel)

    ENDFOREACH( file test_list)

  else()
    message(STATUS "Skipped generation of unit tests.")
  endif()
endif()

if(BENCHMARK MATCHES "^[Tt][Rr][Uu][Ee]" OR BENCHMARK MATCHES "^[Oo][Nn]")
  if(ARCH MATCHES "[Aa][Rr][Mm]")
    message(WARNING "Benchamrking is not currently available while compliling for ARM. If you wish to build benchmarks, please change the architecture to x86_64")
    return()
  endif()

  ExternalProject_Add(GoogleBench
    PREFIX lib/google_bench
    GIT_REPOSITORY https://github.com/google/benchmark
    CMAKE_ARGS -DCMAKE_INSTALL_PREFIX=${EXTERNAL_INSTALL_LOCATION}
    )

  MESSAGE("Discovering benchmark files")

  file(GLOB benchmark_list benchmarks/*.cpp)
  FOREACH(file ${benchmark_list})
    string(REPLACE ".cpp" "" benchmark_name_unclean ${file})
    get_filename_component( benchmark_name ${benchmark_name_unclean} NAME)

    MESSAGE("Benchmark file discovered: ${benchmark_name}. Generating build target ${benchmark_name}_bench")

    add_executable( ${benchmark_name} ${file})

    add_dependencies(${benchmark_name} GoogleBench)
    add_dependencies(${benchmark_name} hel)

    target_link_libraries(${benchmark_name} benchmark pthread)

    set_target_properties( ${benchmark_name} PROPERTIES RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin/benchmarks")

  ENDFOREACH(file in test_list)

endif()

if(DOXYGEN_FOUND AND CMAKE_BUILD_TYPE MATCHES "^[Rr][Ee][Ll][Ee][Aa][Ss][Ee]" OR BUILD_DOC MATCHES "^[Oo][Nn]" OR BUILD_DOC MATCHES "^[Tt][Rr][Uu][Ee]")

  set(DOXYFILE ${CMAKE_BINARY_DIR}/docs/Doxyfile)

  add_custom_target( doxygen ALL
    COMMAND ${DOXYGEN_EXECUTABLE} ${DOXYFILE}
    WORKING_DIRECTORY ${CMAKE_BINARY_DIR}
    COMMENT "Generating API documentation"
    VERBATIM )

else()
  MESSAGE("Doxygen not found; skipping")
endif()
