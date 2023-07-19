set(SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/browser_app.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client_manager.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client.cpp
)

set(HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/include/run_browser.h
    ${CMAKE_CURRENT_LIST_DIR}/internal/browser_app.h
    ${CMAKE_CURRENT_LIST_DIR}/internal/client_manager.h
    ${CMAKE_CURRENT_LIST_DIR}/internal/client.h
    ${CMAKE_CURRENT_LIST_DIR}/internal/core.h
)

set(MAC_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/include/mac_main.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client_mac.mm
    ${CMAKE_CURRENT_LIST_DIR}/src/run_browser_mac.mm
)

if(OS_MAC)
    set(SOURCES ${SOURCES} ${MAC_SOURCES})

    if(USE_SANDBOX)
        ADD_LOGICAL_TARGET("cef_sandbox_lib" "${CEF_SANDBOX_LIB_DEBUG}" "${CEF_SANDBOX_LIB_RELEASE}")
    endif()
endif()

add_library(${PROJECT_NAME} SHARED ${SOURCES} ${HEADERS})
include_directories(BEFORE ${CMAKE_CURRENT_LIST_DIR})
target_link_libraries(${PROJECT_NAME} libcef_dll_wrapper ${CEF_STANDARD_LIBS})

if(USE_SANDBOX)
    message(STATUS "Linking CEF Sandbox")
    target_link_libraries(${PROJECT_NAME} cef_sandbox_lib)
endif()

if(NOT OS_MAC)
    message(FATAL_ERROR "This CMake file currently contains mac only code")
endif()

add_executable(main_mac MACOSX_BUNDLE ${CMAKE_CURRENT_LIST_DIR}/include/mac_main.cpp)
target_link_libraries(main_mac ${PROJECT_NAME})
target_link_libraries(main_mac libcef_dll_wrapper ${CEF_STANDARD_LIBS})

foreach(suffix_list ${CEF_HELPER_APP_SUFFIXES})
    string(REPLACE ":" ";" suffix_list ${suffix_list})
    list(GET suffix_list 0 name_suffix)
    list(GET suffix_list 1 target_suffix)
    list(GET suffix_list 2 plist_suffix)

    set(helper_target "main_mac_helper${target_suffix}")
    set(helper_output_name "minimal_mac_helper${name_suffix}")

    add_executable(${helper_target} MACOSX_BUNDLE ${SOURCES} ${CMAKE_CURRENT_LIST_DIR}/internal/shared_main.h)
    add_dependencies(${helper_target} libcef_dll_wrapper)
    target_link_libraries(${helper_target} libcef_dll_wrapper ${CEF_STANDARD_LIBS})

    if(USE_SANDBOX)
        target_link_libraries(${helper_target} cef_sandbox_lib)
    endif()

    add_dependencies(main_mac ${helper_target})
endforeach()
