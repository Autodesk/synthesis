cmake_minimum_required(VERSION 3.23)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

project(isotope LANGUAGES CXX)

if(NOT APPLE)
    message(FATAL_ERROR "This project is only supported on macOS.")
endif()

set(USER_APP_DATA_DIR "$ENV{HOME}/Library/Application Support")
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
    set(libpath "${FUSION_API_LIBRARY_DIR}/${target}${CMAKE_SHARED_LIBRARY_SUFFIX}")
    add_library(${target} SHARED IMPORTED)
    set_target_properties(${target} PROPERTIES IMPORTED_LOCATION ${libpath})
endforeach()

add_library(${FUSION_API_TARGET} INTERFACE)
add_library(${FUSION_API_NAMESPACE}::${FUSION_API_TARGET} ALIAS ${FUSION_API_TARGET})
target_include_directories(${FUSION_API_TARGET} INTERFACE ${FUSION_API_INCLUDE_DIR})
target_link_libraries(${FUSION_API_TARGET} INTERFACE ${FUSION_API_LIBRARY_TARGETS})

file(GLOB SOURCES "${CMAKE_CURRENT_LIST_DIR}/src/*.cpp")
add_library(${PROJECT_NAME} SHARED ${SOURCES})

set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "")
target_link_libraries(${PROJECT_NAME} PRIVATE ${FUSION_API_NAMESPACE}::${FUSION_API_TARGET})
target_include_directories(${PROJECT_NAME} PRIVATE "${CMAKE_CURRENT_LIST_DIR}/inc")

set(ABSL_PROPAGATE_CXX_STD ON)
set(protobuf_BUILD_TESTS OFF CACHE BOOL "" FORCE)
set(protobuf_BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(protobuf_BUILD_CONFORMANCE OFF CACHE BOOL "" FORCE)
set(protobuf_INSTALL OFF CACHE BOOL "" FORCE)

include(FetchContent)
FetchContent_Declare(
    protobuf
    GIT_REPOSITORY https://github.com/protocolbuffers/protobuf.git
    GIT_TAG        v23.0
)
FetchContent_MakeAvailable(protobuf)
find_package(Protobuf REQUIRED)

set(Protobuf_PROTOC_EXECUTABLE $<TARGET_FILE:protoc>)

target_link_libraries(${PROJECT_NAME} PRIVATE protobuf::libprotobuf)
file(GLOB PROTO_FILES "${CMAKE_CURRENT_LIST_DIR}/../mirabuf/*.proto")
set(PROTO_GEN_DIR "${CMAKE_CURRENT_BINARY_DIR}/protobuf_gen")
file(MAKE_DIRECTORY ${PROTO_GEN_DIR})
protobuf_generate_cpp(PROTO_SRCS PROTO_HDRS ${PROTO_FILES} GENERATE_EXTENSIONS ON PROTOC_OUT_DIR ${PROTO_GEN_DIR})
target_sources(${PROJECT_NAME} PRIVATE ${PROTO_SRCS} ${PROTO_HDRS})
target_include_directories(${PROJECT_NAME} BEFORE PRIVATE ${PROTO_GEN_DIR})

install(TARGETS ${PROJECT_NAME} DESTINATION ${PROJECT_INSTALL_DIR})
install(FILES "${CMAKE_CURRENT_LIST_DIR}/isotope.manifest" DESTINATION ${PROJECT_INSTALL_DIR})
install(DIRECTORY "${CMAKE_CURRENT_LIST_DIR}/resources/" DESTINATION "${PROJECT_INSTALL_DIR}/resources" FILES_MATCHING PATTERN "*.png") 
