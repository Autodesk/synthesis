set(CEF_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/app_browser_minimal.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/main_minimal.cpp
)

set(CEF_HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/inc/client.h
)

if(OS_LINUX OR OS_WINDOWS)
    set(CEF_SOURCES
        ${CEF_SOURCES}
        ${CMAKE_CURRENT_LIST_DIR}/src/app_other_minimal.cpp
        ${CMAKE_CURRENT_LIST_DIR}/src/app_renderer_minimal.cpp
    )
elseif(OS_MAC)
    set(CEF_HELPER_SOURCES
        ${CMAKE_CURRENT_LIST_DIR}/src/app_other_minimal.cpp
        ${CMAKE_CURRENT_LIST_DIR}/src/app_renderer_minimal.cpp
        ${CMAKE_CURRENT_LIST_DIR}/src/browser_helper_mac.cpp
    )
endif()

if(OS_MACOSX OR OS_WINDOWS)
  set(CEF_RESOURCES_SOURCES
    ${SHARED_RESOURCES_SOURCES}
    )
endif()

set(CEF_TARGET "synthesis_cef")
if(OS_MAC)
  set(CEF_HELPER_TARGET "synthesis_cef_Helper")
  set(CEF_HELPER_OUTPUT_NAME "synthesis_cef Helper")
endif()

if(OS_WINDOWS)
    set(CEF_SOURCES
        ${CEF_SOURCES}
        ${CEF_RESOURCES_SOURCES}
    )

    SET_CEF_SOURCE_GROUPS(${CEF_SOURCES})
    SET_CEF_SOURCE_GROUPS(${CEF_HEADERS})

    add_executable(${CEF_TARGET} WIN32 ${CEF_SOURCES} ${CEF_HEADERS})
    SET_CEF_EXECUTABLE_TARGET_PROPERTIES(${CEF_TARGET})
    add_dependencies(${CEF_TARGET} shared)
    target_link_libraries(${CEF_TARGET} shared)
endif()

if(OS_MAC)
    SET_CEF_SOURCE_GROUPS(${CEF_SOURCES})
    SET_CEF_SOURCE_GROUPS(${CEF_HEADERS})
    SET_CEF_SOURCE_GROUPS(${CEF_HELPER_SOURCES})

    set(CEF_APP "${CEF_TARGET_OUT_DIR}/${CEF_TARGET}.app")
    set(EXECUTABLE_NAME ${CEF_TARGET})
    set(PRODUCT_NAME ${CEF_TARGET})

    if(USE_SANDBOX)
        ADD_LOGICAL_TARGET("cef_sandbox_lib" ${CEF_SANDBOX_LIB_DEBUG} ${CEF_SANDBOX_LIB_RELEASE})
    endif()

    add_executable(${CEF_TARGET} MACOSX_BUNDLE ${CEF_SOURCES} ${CEF_HEADERS} ${CEF_RESOURCES_SOURCES})
    SET_CEF_EXECUTABLE_TARGET_PROPERTIES(${CEF_TARGET})
    add_dependencies(${CEF_TARGET} shared)
    target_link_libraries(${CEF_TARGET} shared)
    set_target_properties(${CEF_TARGET} PROPERTIES
        RESOURCE "${CEF_RESOURCES_SOURCES}"
        MACOSX_BUNDLE_INFO_PLIST "${CMAKE_CURRENT_SOURCE_DIR}/${SHARED_INFO_PLIST}"
    )

    add_custom_command(
        TARGET ${CEF_TARGET}
        POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
                "${CEF_BINARY_DIR}/Chromium Embedded Framework.framework"
                "${CEF_APP}/Contents/Frameworks/Chromium Embedded Framework.framework"
        VERBATIM
    )

    foreach(_suffix_list ${CEF_HELPER_APP_SUFFIXES})
        string(REPLACE ":" ";" _suffix_list ${_suffix_list})
        list(GET _suffix_list 0 _name_suffix)
        list(GET _suffix_list 1 _target_suffix)
        list(GET _suffix_list 2 _plist_suffix)

        set(_helper_target "${CEF_HELPER_TARGET}${_target_suffix}")
        set(_helper_output_name "${CEF_HELPER_OUTPUT_NAME}${_name_suffix}")

        set(_helper_info_plist "${CMAKE_CURRENT_BINARY_DIR}/helper-Info${_target_suffix}.plist")
        file(READ "${CMAKE_CURRENT_SOURCE_DIR}/${SHARED_HELPER_INFO_PLIST}" _plist_contents)
        string(REPLACE "\${EXECUTABLE_NAME}" "${_helper_output_name}" _plist_contents ${_plist_contents})
        string(REPLACE "\${PRODUCT_NAME}" "${_helper_output_name}" _plist_contents ${_plist_contents})
        string(REPLACE "\${BUNDLE_ID_SUFFIX}" "${_plist_suffix}" _plist_contents ${_plist_contents})
        file(WRITE ${_helper_info_plist} ${_plist_contents})

        add_executable(${_helper_target} MACOSX_BUNDLE ${CEF_HELPER_SOURCES})
        SET_CEF_EXECUTABLE_TARGET_PROPERTIES(${_helper_target})
        add_dependencies(${_helper_target} shared_helper)
        target_link_libraries(${_helper_target} shared_helper)
        set_target_properties(${_helper_target} PROPERTIES
            MACOSX_BUNDLE_INFO_PLIST ${_helper_info_plist}
            OUTPUT_NAME ${_helper_output_name}
        )

        if(USE_SANDBOX)
            target_link_libraries(${_helper_target} cef_sandbox_lib)
        endif()

        add_dependencies(${CEF_TARGET} ${_helper_target})

        add_custom_command(
            TARGET ${CEF_TARGET}
            POST_BUILD
            COMMAND ${CMAKE_COMMAND} -E copy_directory
                    "${CEF_TARGET_OUT_DIR}/${_helper_output_name}.app"
                    "${CEF_APP}/Contents/Frameworks/${_helper_output_name}.app"
            VERBATIM
        )
    endforeach()

  set(PREFIXES
    "../res/mac/"
    )
  COPY_MAC_RESOURCES("${CEF_RESOURCES_SOURCES}" "${PREFIXES}" "${CEF_TARGET}" "${CMAKE_CURRENT_SOURCE_DIR}" "${CEF_APP}")
endif()

include_directories(BEFORE ${CMAKE_CURRENT_LIST_DIR}/../shared/inc)
include_directories(BEFORE ${CMAKE_CURRENT_LIST_DIR}/inc)
