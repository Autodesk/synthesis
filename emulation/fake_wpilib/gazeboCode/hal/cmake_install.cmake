# Install script for directory: C:/Users/T_hics/Documents/GitHub/allwpilib/hal

# Set the install prefix
if(NOT DEFINED CMAKE_INSTALL_PREFIX)
  set(CMAKE_INSTALL_PREFIX "C:/Program Files/All-WPILib")
endif()
string(REGEX REPLACE "/$" "" CMAKE_INSTALL_PREFIX "${CMAKE_INSTALL_PREFIX}")

# Set the install configuration name.
if(NOT DEFINED CMAKE_INSTALL_CONFIG_NAME)
  if(BUILD_TYPE)
    string(REGEX REPLACE "^[^A-Za-z0-9_]+" ""
           CMAKE_INSTALL_CONFIG_NAME "${BUILD_TYPE}")
  else()
    set(CMAKE_INSTALL_CONFIG_NAME "Release")
  endif()
  message(STATUS "Install configuration: \"${CMAKE_INSTALL_CONFIG_NAME}\"")
endif()

# Set the component getting installed.
if(NOT CMAKE_INSTALL_COMPONENT)
  if(COMPONENT)
    message(STATUS "Install component: \"${COMPONENT}\"")
    set(CMAKE_INSTALL_COMPONENT "${COMPONENT}")
  else()
    set(CMAKE_INSTALL_COMPONENT)
  endif()
endif()

if(NOT CMAKE_INSTALL_COMPONENT OR "${CMAKE_INSTALL_COMPONENT}" STREQUAL "Unspecified")
  if("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Dd][Ee][Bb][Uu][Gg])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/build/hal/Debug/HALAthena.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Rr][Ee][Ll][Ee][Aa][Ss][Ee])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/build/hal/Release/HALAthena.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Mm][Ii][Nn][Ss][Ii][Zz][Ee][Rr][Ee][Ll])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/build/hal/MinSizeRel/HALAthena.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Rr][Ee][Ll][Ww][Ii][Tt][Hh][Dd][Ee][Bb][Ii][Nn][Ff][Oo])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/build/hal/RelWithDebInfo/HALAthena.lib")
  endif()
endif()

if(NOT CMAKE_INSTALL_COMPONENT OR "${CMAKE_INSTALL_COMPONENT}" STREQUAL "ni_lib")
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE FILE FILES
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libFRC_NetworkCommunication.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libFRC_NetworkCommunication.so.1"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libFRC_NetworkCommunication.so.1.5"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libFRC_NetworkCommunication.so.1.5.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libFRC_NetworkCommunicationLV.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libFRC_NetworkCommunicationLV.so.1"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libFRC_NetworkCommunicationLV.so.1.5"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libFRC_NetworkCommunicationLV.so.1.5.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libGCBase_gcc-4.4-arm_v2_3.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libGenApi_gcc-4.4-arm_v2_3.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libi2c.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libi2c.so.1"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libi2c.so.1.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libi2c.so.1.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/liblog4cpp_gcc-4.4-arm_v2_3.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libLog_gcc-4.4-arm_v2_3.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libMathParser_gcc-4.4-arm_v2_3.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiFpga.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiFpga.so.14"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiFpga.so.14.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiFpga.so.14.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiFpgaLv.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiFpgaLv.so.14"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiFpgaLv.so.14.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiFpgaLv.so.14.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libniimaqdx.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libniimaqdx.so.14"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libniimaqdx.so.14.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libniimaqdx.so.14.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiRioSrv.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiRioSrv.so.14"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiRioSrv.so.14.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libNiRioSrv.so.14.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnirio_emb_can.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnirio_emb_can.so.14"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnirio_emb_can.so.14.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnirio_emb_can.so.14.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnivision.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnivision.so.14"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnivision.so.14.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnivision.so.14.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnivissvc.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnivissvc.so.14"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnivissvc.so.14.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libnivissvc.so.14.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libni_emb.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libni_emb.so.7"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libni_emb.so.7.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libni_emb.so.7.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libni_rtlog.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libni_rtlog.so.2"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libni_rtlog.so.2.3"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libni_rtlog.so.2.3.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libRoboRIO_FRC_ChipObject.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libRoboRIO_FRC_ChipObject.so.1"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libRoboRIO_FRC_ChipObject.so.1.2"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libRoboRIO_FRC_ChipObject.so.1.2.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libspi.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libspi.so.1"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libspi.so.1.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libspi.so.1.0.0"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libvisa.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libwpi.so"
    "C:/Users/T_hics/Documents/GitHub/allwpilib/ni-libraries/libwpi_2015.so"
    )
endif()

if(NOT CMAKE_INSTALL_COMPONENT OR "${CMAKE_INSTALL_COMPONENT}" STREQUAL "headers")
  list(APPEND CMAKE_ABSOLUTE_DESTINATION_FILES
   "C:/Program Files/All-WPILib/include")
  if(CMAKE_WARN_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(WARNING "ABSOLUTE path INSTALL DESTINATION : ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
  if(CMAKE_ERROR_ON_ABSOLUTE_INSTALL_DESTINATION)
    message(FATAL_ERROR "ABSOLUTE path INSTALL DESTINATION forbidden (by caller): ${CMAKE_ABSOLUTE_DESTINATION_FILES}")
  endif()
file(INSTALL DESTINATION "C:/Program Files/All-WPILib" TYPE DIRECTORY FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/hal/include")
endif()

if(NOT CMAKE_INSTALL_COMPONENT OR "${CMAKE_INSTALL_COMPONENT}" STREQUAL "lib")
  if("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Dd][Ee][Bb][Uu][Gg])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/build/hal/Debug/HALAthena_shared.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Rr][Ee][Ll][Ee][Aa][Ss][Ee])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/build/hal/Release/HALAthena_shared.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Mm][Ii][Nn][Ss][Ii][Zz][Ee][Rr][Ee][Ll])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/build/hal/MinSizeRel/HALAthena_shared.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Rr][Ee][Ll][Ww][Ii][Tt][Hh][Dd][Ee][Bb][Ii][Nn][Ff][Oo])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY OPTIONAL FILES "C:/Users/T_hics/Documents/GitHub/allwpilib/build/hal/RelWithDebInfo/HALAthena_shared.lib")
  endif()
endif()

