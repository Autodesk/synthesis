set(SHARED_COMMON_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/inc/app_factory.h
    ${CMAKE_CURRENT_LIST_DIR}/src/main_util.cpp
)

set(SHARED_COMMON_HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/inc/main_util.h
)

set(SHARED_SOURCES
    ${SHARED_COMMON_SOURCES}
    ${CMAKE_CURRENT_LIST_DIR}/src/client_manager.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client_util.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/browser_util.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/resource_util.cpp
)

set(SHARED_HEADERS
    ${SHARED_COMMON_HEADERS}
    ${CMAKE_CURRENT_LIST_DIR}/inc/client_manager.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/client_util.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/browser_util.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/shared_main.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/resource_util.h
)

set(SHARED_SOURCES_MAC
    ${CMAKE_CURRENT_LIST_DIR}/src/client_util_mac.mm
    ${CMAKE_CURRENT_LIST_DIR}/src/main_mac.mm
    ${CMAKE_CURRENT_LIST_DIR}/src/resource_util_mac.mm
    ${CMAKE_CURRENT_LIST_DIR}/src/resource_util_posix.cpp
)

set(SHARED_SOURCES_WINDOWS
    ${CMAKE_CURRENT_LIST_DIR}/src/client_util_win.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/main_win.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/resource_util_win.cpp
)

# Very important not to forget about this line
# Will append the platform sources to the shared sources
APPEND_PLATFORM_SOURCES(SHARED_SOURCES)

if(OS_MAC)
    set(SHARED_HELPER_SOURCES
        ${SHARED_COMMON_SOURCES}
        ${CMAKE_CURRENT_LIST_DIR}/src/process_helper_mac.cpp
        ${CMAKE_CURRENT_LIST_DIR}/inc/process_helper_mac.h
    )
endif()

set(SHARED_TARGET "shared")
if(OS_MAC)
    set(SHARED_HELPER_TARGET "shared_helper")
endif()

if(OS_LINUX OR OS_WINDOWS)
    # MacOS loades the CEF framework dynamically on startup unlike windows and linux
    ADD_LOGICAL_TARGET("libcef_lib" ${CEF_LIB_DEBUG} ${CEF_LIB_RELEASE})
endif()

if(OS_WINDOWS)
    SET_CEF_SOURCE_GROUPS(${SHARED_SOURCES})
    SET_CEF_SOURCE_GROUPS(${SHARED_HEADERS})
  
    add_library(${SHARED_TARGET} STATIC ${SHARED_SOURCES} ${SHARED_HEADERS})
    SET_CEF_LIBRARY_TARGET_PROPERTIES(${SHARED_TARGET})
    add_dependencies(${SHARED_TARGET} libcef_dll_wrapper)
    target_link_libraries(${SHARED_TARGET} libcef_lib libcef_dll_wrapper ${CEF_STANDARD_LIBS})
  
    if(USE_SANDBOX)
        ADD_LOGICAL_TARGET("cef_sandbox_lib" ${CEF_SANDBOX_LIB_DEBUG} ${CEF_SANDBOX_LIB_RELEASE})
        target_link_libraries(${SHARED_TARGET} cef_sandbox_lib ${CEF_SANDBOX_STANDARD_LIBS})
    endif()
  
    COPY_FILES(${SHARED_TARGET} ${CEF_BINARY_FILES} ${CEF_BINARY_DIR} ${CEF_TARGET_OUT_DIR})
    COPY_FILES(${SHARED_TARGET} ${CEF_RESOURCE_FILES} ${CEF_RESOURCE_DIR} ${CEF_TARGET_OUT_DIR})
endif()

if(OS_MAC)
    SET_CEF_SOURCE_GROUPS(${SHARED_SOURCES})
    SET_CEF_SOURCE_GROUPS(${SHARED_HEADERS})
    SET_CEF_SOURCE_GROUPS(${SHARED_HELPER_SOURCES})
  
    add_library(${SHARED_HELPER_TARGET} STATIC ${SHARED_HELPER_SOURCES})
    SET_CEF_LIBRARY_TARGET_PROPERTIES(${SHARED_HELPER_TARGET})
    add_dependencies(${SHARED_HELPER_TARGET} libcef_dll_wrapper)
    target_link_libraries(${SHARED_HELPER_TARGET} libcef_dll_wrapper ${CEF_STANDARD_LIBS})
  
    if(USE_SANDBOX)
        ADD_LOGICAL_TARGET("cef_sandbox_lib" ${CEF_SANDBOX_LIB_DEBUG} ${CEF_SANDBOX_LIB_RELEASE})
        target_link_libraries(${SHARED_HELPER_TARGET} cef_sandbox_lib)
    endif()

    add_library(${SHARED_TARGET} STATIC ${SHARED_SOURCES} ${SHARED_HEADERS})
    SET_CEF_LIBRARY_TARGET_PROPERTIES(${SHARED_TARGET})
    add_dependencies(${SHARED_TARGET} libcef_dll_wrapper ${SHARED_HELPER_TARGET})
    target_link_libraries(${SHARED_TARGET} libcef_dll_wrapper ${CEF_STANDARD_LIBS})
endif()

include_directories(BEFORE ${CMAKE_CURRENT_LIST_DIR}/inc)
