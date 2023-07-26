set(WRAPPER_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/html_resource_handler.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/offscreen_cef_client.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/wrapper.cpp
)

set(WRAPPER_HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/inc/html_resource_handler.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/offscreen_cef_client.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/wrapper.h
)

SET_CEF_SOURCE_GROUPS(${WRAPPER_SOURCES})
SET_CEF_SOURCE_GROUPS(${WRAPPER_HEADERS})

set(WRAPPER synthesis_cef_wrapper)
add_library(${WRAPPER} SHARED ${WRAPPER_SOURCES} ${WRAPPER_HEADERS})
SET_CEF_LIBRARY_TARGET_PROPERTIES(${WRAPPER})

add_dependencies(${WRAPPER} shared)
target_link_libraries(${WRAPPER} shared)

add_dependencies(${WRAPPER} libcef_dll_wrapper)
target_link_libraries(${WRAPPER} libcef_dll_wrapper)

include_directories(BEFORE ${CMAKE_CURRENT_LIST_DIR}/inc)
include_directories(BEFORE ${CMAKE_CURRENT_LIST_DIR}/../shared/inc)
