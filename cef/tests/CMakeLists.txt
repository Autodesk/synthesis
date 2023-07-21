add_subdirectory(cef)

find_package(Python "3.11" REQUIRED COMPONENTS Interpreter)

message(STATUS "Generating CMake tests")
execute_process(COMMAND "${Python_EXECUTABLE}" "${CMAKE_CURRENT_SOURCE_DIR}/generate_tests.py" "${CMAKE_CURRENT_SOURCE_DIR}")
add_test(NAME synthesis_cef_static_testing COMMAND "${Python_EXECUTABLE}" "${CMAKE_CURRENT_SOURCE_DIR}/run_tests.py" "${CMAKE_CURRENT_SOURCE_DIR}")
