cmake_minimum_required(VERSION 3.20.0)
project(SynthesisWASM)

# Setup toolchain and some initial build settings
set(CMAKE_TOOLCHAIN_FILE "$ENV{EMSDK}/upstream/emscripten/cmake/Modules/Platform/Emscripten.cmake")
set(CMAKE_BUILD_TYPE Debug)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Setup emcc specific settings
set(WASM_EXPORTED_SYMBOLS_LIST
    _proto_test
    _parse_assembly _destroy_assembly _debug_print_assembly
    _make_obj _get_obj_value _free_obj
    _test_phys

    _core_init _core_destroy
    _physics_step _physics_create_ball _physics_get_position

    _malloc _free
)
list(JOIN WASM_EXPORTED_SYMBOLS_LIST "," WASM_EXPORTED_SYMBOLS)
message(STATUS "Exported symbols: [${WASM_EXPORTED_SYMBOLS}]")

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/out/")
set(EMCC_FLAGS "\
    --no-entry \
    --pre-js ../js/locateFile.js \
    -sEXPORTED_FUNCTIONS=[${WASM_EXPORTED_SYMBOLS}] \
    -sUSE_ES6_IMPORT_META=0 \
    -sEXPORT_ES6=1 \
    -sEXPORT_NAME='createModule' \
    -sEXPORTED_RUNTIME_METHODS=['ccall','cwrap'] \
    -sMODULARIZE=1 \
    -sENVIRONMENT=web \
    -sTOTAL_STACK=1048576 \
    -sTOTAL_MEMORY=67108864 \
    -sALLOW_MEMORY_GROWTH=1 \
    -sALLOW_TABLE_GROWTH=1 \
    -std=c++17 \
    -O2 \
    --bind \
")

# Add dependencies
add_subdirectory(vendor/mirabuf/)

add_subdirectory(vendor/JoltPhysics/Build/)

file(GLOB_RECURSE SRC src/*.cpp)
add_executable(synthesis-web ${SRC})

message(STATUS "Setting compilation target to WASM")
set(CMAKE_EXECUTABLE_SUFFIX ".mjs")
set_target_properties(synthesis-web PROPERTIES LINK_FLAGS ${EMCC_FLAGS})

target_include_directories(synthesis-web PUBLIC include/)
target_include_directories(synthesis-web PUBLIC vendor/JoltPhysics/)
target_link_libraries(synthesis-web mirabuf)
target_link_libraries(synthesis-web Jolt)

# ALL THE COMMANDS!!!!
add_custom_command(TARGET synthesis-web POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E rename ${CMAKE_RUNTIME_OUTPUT_DIRECTORY}synthesis-web.mjs ../../synthesis-app/src/emsc/synthesis-web.mjs
)
add_custom_command(TARGET synthesis-web POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E rename ${CMAKE_RUNTIME_OUTPUT_DIRECTORY}synthesis-web.wasm ../../synthesis-app/public/synthesis-web.wasm
)
