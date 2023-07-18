set(SYNTHESIS_CEF_CORE_DIR ${CMAKE_SOURCE_DIR}/build/cef/libsynthesis_cef.dylib)
add_library(synthesis_cef_core SHARED IMPORTED)
set_target_properties(synthesis_cef_core PROPERTIES IMPORTED_LOCATION ${SYNTHESIS_CEF_CORE_DIR})

add_executable(mac_open_browser ./open_browser_mac/test.mm)
target_link_libraries(mac_open_browser synthesis_cef_core)
target_link_libraries(mac_open_browser libcef_dll_wrapper ${CEF_STANDARD_LIBS})

include_directories(BEFORE ${CMAKE_SOURCE_DIR}/cef)
