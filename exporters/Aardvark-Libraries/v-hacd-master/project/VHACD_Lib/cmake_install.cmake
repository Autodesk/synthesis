# Install script for directory: C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib

# Set the install prefix
if(NOT DEFINED CMAKE_INSTALL_PREFIX)
  set(CMAKE_INSTALL_PREFIX "C:/Program Files (x86)/VHACD")
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

# Is this installation the result of a crosscompile?
if(NOT DEFINED CMAKE_CROSSCOMPILING)
  set(CMAKE_CROSSCOMPILING "FALSE")
endif()

if("x${CMAKE_INSTALL_COMPONENT}x" STREQUAL "xUnspecifiedx" OR NOT CMAKE_INSTALL_COMPONENT)
  if("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Dd][Ee][Bb][Uu][Gg])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/Debug/vhacd.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Rr][Ee][Ll][Ee][Aa][Ss][Ee])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/Release/vhacd.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Mm][Ii][Nn][Ss][Ii][Zz][Ee][Rr][Ee][Ll])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/MinSizeRel/vhacd.lib")
  elseif("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Rr][Ee][Ll][Ww][Ii][Tt][Hh][Dd][Ee][Bb][Ii][Nn][Ff][Oo])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib" TYPE STATIC_LIBRARY FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/RelWithDebInfo/vhacd.lib")
  endif()
endif()

if("x${CMAKE_INSTALL_COMPONENT}x" STREQUAL "xUnspecifiedx" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/include" TYPE FILE FILES
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/FloatMath.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/btAlignedAllocator.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/btAlignedObjectArray.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/btConvexHullComputer.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/btMinMax.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/btScalar.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/btVector3.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdCircularList.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdICHull.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdManifoldMesh.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdMesh.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdMutex.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdRaycastMesh.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdSArray.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdTimer.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdVHACD.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdVector.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdVolume.h"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/public/VHACD.h"
    )
endif()

if("x${CMAKE_INSTALL_COMPONENT}x" STREQUAL "xUnspecifiedx" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/include" TYPE FILE FILES
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdCircularList.inl"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/src/VHACD_Lib/inc/vhacdVector.inl"
    )
endif()

if("x${CMAKE_INSTALL_COMPONENT}x" STREQUAL "xUnspecifiedx" OR NOT CMAKE_INSTALL_COMPONENT)
  if(EXISTS "$ENV{DESTDIR}${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd/vhacd-targets.cmake")
    file(DIFFERENT EXPORT_FILE_CHANGED FILES
         "$ENV{DESTDIR}${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd/vhacd-targets.cmake"
         "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/CMakeFiles/Export/lib/cmake/vhacd/vhacd-targets.cmake")
    if(EXPORT_FILE_CHANGED)
      file(GLOB OLD_CONFIG_FILES "$ENV{DESTDIR}${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd/vhacd-targets-*.cmake")
      if(OLD_CONFIG_FILES)
        message(STATUS "Old export file \"$ENV{DESTDIR}${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd/vhacd-targets.cmake\" will be replaced.  Removing files [${OLD_CONFIG_FILES}].")
        file(REMOVE ${OLD_CONFIG_FILES})
      endif()
    endif()
  endif()
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd" TYPE FILE FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/CMakeFiles/Export/lib/cmake/vhacd/vhacd-targets.cmake")
  if("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Dd][Ee][Bb][Uu][Gg])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd" TYPE FILE FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/CMakeFiles/Export/lib/cmake/vhacd/vhacd-targets-debug.cmake")
  endif()
  if("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Mm][Ii][Nn][Ss][Ii][Zz][Ee][Rr][Ee][Ll])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd" TYPE FILE FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/CMakeFiles/Export/lib/cmake/vhacd/vhacd-targets-minsizerel.cmake")
  endif()
  if("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Rr][Ee][Ll][Ww][Ii][Tt][Hh][Dd][Ee][Bb][Ii][Nn][Ff][Oo])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd" TYPE FILE FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/CMakeFiles/Export/lib/cmake/vhacd/vhacd-targets-relwithdebinfo.cmake")
  endif()
  if("${CMAKE_INSTALL_CONFIG_NAME}" MATCHES "^([Rr][Ee][Ll][Ee][Aa][Ss][Ee])$")
    file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd" TYPE FILE FILES "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/CMakeFiles/Export/lib/cmake/vhacd/vhacd-targets-release.cmake")
  endif()
endif()

if("x${CMAKE_INSTALL_COMPONENT}x" STREQUAL "xDevelx" OR NOT CMAKE_INSTALL_COMPONENT)
  file(INSTALL DESTINATION "${CMAKE_INSTALL_PREFIX}/lib/cmake/vhacd" TYPE FILE FILES
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/vhacd/vhacd-config.cmake"
    "C:/Users/Victo/Documents/Sythesis/synthesis/exporters/Aardvark-Libraries/v-hacd-master/project/VHACD_Lib/vhacd/vhacd-config-version.cmake"
    )
endif()

