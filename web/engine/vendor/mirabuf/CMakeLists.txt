set(protobuf_BUILD_TESTS OFF)
set(protobuf_BUILD_LIBPROTOC OFF)
set(protobuf_BUILD_SHARED_LIBS OFF)
set(protobuf_BUILD_PROTOC_BINARIES OFF)

add_subdirectory(../protobuf ${CMAKE_BINARY_DIR}/protobuf)

file(GLOB_RECURSE SRC *.cc)
add_library(mirabuf ${SRC})
target_link_libraries(mirabuf libprotobuf)
target_include_directories(mirabuf PUBLIC "./")
target_include_directories(mirabuf PUBLIC ${PROTOBUF_INCLUDE})

# message(STATUS $ENV{PROTOBUF_INCLUDE})