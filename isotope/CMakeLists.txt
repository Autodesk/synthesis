cmake_minimum_required(VERSION 3.23)
set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

project(isotope LANGUAGES CXX)

if(NOT APPLE AND NOT WIN32)
    message(FATAL_ERROR "This project is only supported on macOS and Windows.")
endif()

if(APPLE)
    set(USER_APP_DATA_DIR "$ENV{HOME}/Library/Application Support")
elseif(WIN32)
    set(USER_APP_DATA_DIR "$ENV{APPDATA}")
endif()
set(USER_APP_DATA_RELATIVE_PATH "Autodesk/Autodesk Fusion 360/API")
set(FUSION_API_DIR "${USER_APP_DATA_DIR}/${USER_APP_DATA_RELATIVE_PATH}")
set(FUSION_API_INCLUDE_DIR "${FUSION_API_DIR}/CPP/include")
set(FUSION_API_LIBRARY_DIR "${FUSION_API_DIR}/CPP/lib")
set(FUSION_API_VERSION_FILE "${FUSION_API_DIR}/version.txt")

set(FUSION_API_ADDINS_DIR "${FUSION_API_DIR}/Addins")
set(PROJECT_INSTALL_DIR "${FUSION_API_ADDINS_DIR}/${PROJECT_NAME}")

file(READ ${FUSION_API_VERSION_FILE} FUSION_API_VERSION)
message(STATUS "Fusion 360 API Version: ${FUSION_API_VERSION}")

set(FUSION_API_TARGET "fusion360")
set(FUSION_API_NAMESPACE "autodesk")
set(FUSION_API_LIBRARY_TARGETS "core" "fusion")

foreach(target IN LISTS FUSION_API_LIBRARY_TARGETS)
    add_library(${target} SHARED IMPORTED)
    if(WIN32)
        set_target_properties(${target} PROPERTIES
            IMPORTED_IMPLIB "${FUSION_API_LIBRARY_DIR}/${target}${CMAKE_IMPORT_LIBRARY_SUFFIX}")
    else()
        set_target_properties(${target} PROPERTIES
            IMPORTED_LOCATION "${FUSION_API_LIBRARY_DIR}/${target}${CMAKE_SHARED_LIBRARY_SUFFIX}")
    endif()
endforeach()

add_library(${FUSION_API_TARGET} INTERFACE)
add_library(${FUSION_API_NAMESPACE}::${FUSION_API_TARGET} ALIAS ${FUSION_API_TARGET})
target_include_directories(${FUSION_API_TARGET} SYSTEM INTERFACE ${FUSION_API_INCLUDE_DIR})
target_link_libraries(${FUSION_API_TARGET} INTERFACE ${FUSION_API_LIBRARY_TARGETS})

file(GLOB SOURCES "${CMAKE_CURRENT_LIST_DIR}/src/*.cpp")
add_library(${PROJECT_NAME} SHARED ${SOURCES})

set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "")
target_link_libraries(${PROJECT_NAME} PRIVATE ${FUSION_API_NAMESPACE}::${FUSION_API_TARGET})
target_include_directories(${PROJECT_NAME} PRIVATE "${CMAKE_CURRENT_LIST_DIR}/inc")
target_compile_options(${PROJECT_NAME} PRIVATE $<$<CXX_COMPILER_ID:Clang,AppleClang,GNU>:-Wno-deprecated-declarations>)

set(ABSL_PROPAGATE_CXX_STD ON)
set(protobuf_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(protobuf_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(protobuf_BUILD_CONFORMANCE OFF CACHE BOOL "" FORCE)
set(protobuf_INSTALL OFF CACHE BOOL "" FORCE)
set(protobuf_MSVC_STATIC_RUNTIME OFF CACHE BOOL "" FORCE)

set(GOOGLE_PROTOBUF_SUBMODULE_DIR "${CMAKE_CURRENT_LIST_DIR}/../protobuf")
if(NOT EXISTS "${GOOGLE_PROTOBUF_SUBMODULE_DIR}/.git")
    message(FATAL_ERROR "google-protobuf is not checked out; make sure to run\n\tgit submodule update --init ../protobuf")
endif()

add_subdirectory(${GOOGLE_PROTOBUF_SUBMODULE_DIR} "${CMAKE_BINARY_DIR}/protobuf")

set(Protobuf_PROTOC_EXECUTABLE $<TARGET_FILE:protoc>)
target_link_libraries(${PROJECT_NAME} PRIVATE protobuf::libprotobuf)

set(PROTO_GEN_DIR "${CMAKE_CURRENT_BINARY_DIR}/protobuf_gen")
set(MIRABUF_SUBMODULE_DIR "${CMAKE_CURRENT_LIST_DIR}/../mirabuf")
file(MAKE_DIRECTORY ${PROTO_GEN_DIR})
file(GLOB PROTO_FILES "${MIRABUF_SUBMODULE_DIR}/*.proto")

set(PROTO_SRCS "")
set(PROTO_HDRS "")
foreach(_P IN LISTS PROTO_FILES)
    get_filename_component(_NWE "${_P}" NAME_WE)
    list(APPEND PROTO_SRCS "${PROTO_GEN_DIR}/${_NWE}.pb.cc")
    list(APPEND PROTO_HDRS "${PROTO_GEN_DIR}/${_NWE}.pb.h")
endforeach()

add_custom_command(
  OUTPUT ${PROTO_SRCS} ${PROTO_HDRS}
  COMMAND ${Protobuf_PROTOC_EXECUTABLE}
          --cpp_out=${PROTO_GEN_DIR}
          -I ${MIRABUF_SUBMODULE_DIR}
          ${PROTO_FILES}
  DEPENDS ${PROTO_FILES}
  COMMENT "Generating protobuf sources"
)

add_custom_target(generate_protos ALL DEPENDS ${PROTO_SRCS} ${PROTO_HDRS})
add_dependencies(${PROJECT_NAME} generate_protos)
target_sources(${PROJECT_NAME}
  PRIVATE
    ${PROTO_SRCS}
    ${PROTO_HDRS}
)
target_include_directories(${PROJECT_NAME} SYSTEM PRIVATE ${PROTO_GEN_DIR})

install(TARGETS ${PROJECT_NAME} DESTINATION ${PROJECT_INSTALL_DIR})
install(FILES "${CMAKE_CURRENT_LIST_DIR}/isotope.manifest" DESTINATION ${PROJECT_INSTALL_DIR})
install(DIRECTORY "${CMAKE_CURRENT_LIST_DIR}/resources/" DESTINATION "${PROJECT_INSTALL_DIR}/resources" FILES_MATCHING PATTERN "*.png")

find_program(CLANG_FORMAT clang-format)
if(CLANG_FORMAT)
    file(GLOB_RECURSE FORMAT_SOURCES
        "${CMAKE_CURRENT_LIST_DIR}/src/*.cpp"
        "${CMAKE_CURRENT_LIST_DIR}/inc/*.h"
    )
    add_custom_target(format
        COMMAND ${CLANG_FORMAT} -i -style=file ${FORMAT_SOURCES}
        COMMENT "Formatting sources with clang-format"
    )
endif()
