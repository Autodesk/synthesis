# Shared static library between all operating system targets
set(SHARED_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/main_util.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client_manager.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client_util.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/browser_util.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/resource_util.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/renderer_app.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/other_app.cpp
)

set(SHARED_HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/inc/app_factory.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/main_util.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/client_manager.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/client_util.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/core.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/browser_util.h
    ${CMAKE_CURRENT_LIST_DIR}/inc/resource_util.h
)

set(MAC_SHARED_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/client_util_mac.mm
    ${CMAKE_CURRENT_LIST_DIR}/src/main_mac.mm
    ${CMAKE_CURRENT_LIST_DIR}/src/process_helper_mac.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/resource_util_mac.mm
    ${CMAKE_CURRENT_LIST_DIR}/src/resource_util_posix.cpp
)

set(MAC_SHARED_HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/inc/process_helper_mac.h
)

if(OS_MAC)
    set(SHARED_SOURCES ${SHARED_SOURCES} ${MAC_SHARED_SOURCES})
    set(SHARED_HEADERS ${SHARED_HEADERS} ${MAC_SHARED_HEADERS})
endif()

set(SHARED_TARGET "shared")

# Mac has a different process for creating the shared helper apps
if(OS_MAC)
    set(SHARED_HELPER_TARGET "shared_helper")
endif()

# TODO: Check the difference between os_mac and os_macosx
if(OS_MACOSX OR OS_WINDOWS)
    macro(SET_SOURCE_GROUPS srcs)
        foreach(file ${srcs})
            get_filename_component(ABS_FILE "${FILE}" ABSOLUTE)
            get_filename_component(PARENT_DIR "${ABS_FILE}" DIRECTORY)

            string(REPLACE "${CMAKE_SOURCE_DIR}/cef/" "" GROUP "${PARENT_DIR}")

            string(REPLACE "/" "\\" GROUP "${GROUP}")

            source_group("${GROUP}" FILES "${FILE}")
        endforeach()
    endmacro()
endif()

macro(SET_CEF_TARGET_PROPERTIES target)
    set_target_properties(${target} PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${SYNTHESIS_OUT_DIR}"
        LIBRARY_OUTPUT_DIRECTORY "${SYNTHESIS_OUT_DIR}"
        ARCHIVE_OUTPUT_DIRECTORY "${SYNTHESIS_OUT_DIR}"
    )

    if(OS_MACOSX OR OS_WINDOWS)
        set_property(TARGET ${target} PROPERTY FOLDER "cef")
    endif()
endmacro()

macro(SET_CEF_LIBRARY_TARGET_PROPERTIES target)
    SET_LIBRARY_TARGET_PROPERTIES(${target})
    SET_CEF_TARGET_PROPERTIES(${target})
endmacro()

macro(SET_CEF_EXECUTABLE_TARGET_PROPERTIES target)
    SET_EXECUTABLE_TARGET_PROPERTIES(${target})
    SET_CEF_TARGET_PROPERTIES(${target})
  
    if(OS_LINUX)
        set_target_properties(${target} PROPERTIES INSTALL_RPATH "$ORIGIN")
        set_target_properties(${target} PROPERTIES BUILD_WITH_INSTALL_RPATH TRUE)
    elseif(OS_WINDOWS)
        add_custom_command(
            TARGET ${target}
            POST_BUILD
            COMMAND "mt.exe" -nologo
                    -manifest \"${CMAKE_CURRENT_SOURCE_DIR}/${SHARED_EXE_MANIFEST}\"
                            \"${CMAKE_CURRENT_SOURCE_DIR}/${SHARED_COMPATIBILITY_MANIFEST}\"
                    -outputresource:"${EXAMPLE_SYNTHESIS_OUT_DIR}/${target}.exe"\;\#1
            COMMENT "Adding manifest..."
            )
    endif()
endmacro()

# TODO: Windows

if(OS_MAC)
    SET_SOURCE_GROUPS(${SHARED_SOURCES})

    add_library(${SHARED_HELPER_TARGET} STATIC ${SHARED_SOURCES} ${SHARED_HEADERS})
    SET_CEF_LIBRARY_TARGET_PROPERTIES(${SHARED_HELPER_TARGET})
    add_dependencies(${SHARED_HELPER_TARGET} libcef_dll_wrapper)
    target_link_libraries(${SHARED_HELPER_TARGET} libcef_dll_wrapper ${CEF_STANDARD_LIBS})

    if(USE_SANDBOX)
        ADD_LOGICAL_TARGET("cef_sandbox_lib" "${CEF_SANDBOX_LIB_DEBUG}" "${CEF_SANDBOX_LIB_RELEASE}")
        target_link_libraries(${SHARED_HELPER_TARGET} cef_sandbox_lib)
    endif()

    add_library(${SHARED_TARGET} STATIC ${SHARED_SOURCES} ${SHARED_HEADERS})
    SET_CEF_LIBRARY_TARGET_PROPERTIES(${SHARED_TARGET})
    add_dependencies(${SHARED_TARGET} libcef_dll_wrapper "${SHARED_HELPER_TARGET}")
    target_link_libraries(${SHARED_TARGET} libcef_dll_wrapper "${CEF_STANDARD_LIBS}")
endif()

# TODO: Linux

set(MAIN_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/browser_app.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/main.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client.cpp
)

set(MAIN_HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/inc/client.h
)

set(MAIN_MAC_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/other_app.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/renderer_app.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/browser_helper_mac.cpp
)

set(SHARED_MAC_RESOURCES
    ${CMAKE_CURRENT_LIST_DIR}/res/mac/English.lproj/InfoPlist.strings
    ${CMAKE_CURRENT_LIST_DIR}/res/mac/English.lproj/MainMenu.xib
    ${CMAKE_CURRENT_LIST_DIR}/res/mac/Info.plist
    ${CMAKE_CURRENT_LIST_DIR}/res/mac/shared.icns
)

set(SHARED_INFO_PLIST ./res/mac/Info.plist)

# TODO: Windows
set(SHARED_WINDOWS_RESCOURCES)

set(CURRENT_TARGET "synthesis_unity_cef")
if(OS_MAC)
    set(CURRENT_TARGET_HELPER "synthesis_unity_cef_helper")
    set(CURRENT_HELPER_OUTPUT_NAME "synthesis cef helper")
endif()

# TODO: Windows

if(OS_MAC)
    SET_SOURCE_GROUPS(${MAIN_SOURCES})
    SET_SOURCE_GROUPS(${MAIN_MAC_SOURCES})

    set(SYNTHESIS_APP "${SYNTHESIS_OUT_DIR}/${CURRENT_TARGET}.app")

    set(EXECUTABLE_NAME "${CURRENT_TARGET}")
    set(PRODUCT_NAME "${CURRENT_TARGET}")

    add_executable(${CURRENT_TARGET} MACOSX_BUNDLE ${MAIN_SOURCES} ${MAIN_HEADERS})
    SET_CEF_EXECUTABLE_TARGET_PROPERTIES(${CURRENT_TARGET})
    add_dependencies(${CURRENT_TARGET} ${SHARED_TARGET})
    target_link_libraries(${CURRENT_TARGET} ${SHARED_TARGET})
    set_target_properties(${CURRENT_TARGET} PROPERTIES
        RESOURCE "${SHARED_MAC_RESOURCES}"
        MACOSX_BUNDLE_INFO_PLIST "${CMAKE_CURRENT_SOURCE_DIR}/${SHARED_INFO_PLIST}"
    )

    add_custom_command(
        TARGET ${CURRENT_TARGET}
        POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
                "${CEF_BINARY_DIR}/Chromium Embedded Framework.framework"
                "${SYNTHESIS_APP}/Contents/Frameworks/Chromium Embedded Framework.framework"
        VERBATIM
    )

    foreach(suffix_list ${CEF_HELPER_APP_SUFfIXES})
        string(REPLACE ":" ";" suffix_list ${suffix_list})
        list(GET suffix_list 0 name_suffix)
        list(GET suffix_list 1 target_suffix)
        list(GET suffix_list 2 plist_suffix)
    
        set(helper_target "main_mac_helper${target_suffix}")
        set(helper_output_name "minimal_mac_helper${name_suffix}")

        set(helper_info_plist "${CMAKE_CURRENT_BINARY_DIR}/helper-Info${target_suffix}.plist")
        file(READ "${CMAKE_CURRENT_SOURCE_DIR}/${SHARED_HELPER_INFO_PLIST}" _plist_contents)
        string(REPLACE "\${EXECUTABLE_NAME}" "${helper_output_name}" _plist_contents ${_plist_contents})
        string(REPLACE "\${PRODUCT_NAME}" "${helper_output_name}" _plist_contents ${_plist_contents})
        string(REPLACE "\${BUNDLE_ID_SUFFIX}" "${_plist_suffix}" _plist_contents ${_plist_contents})
        file(WRITE ${helper_info_plist} ${_plist_contents})

        add_executable(${helper_target} MACOSX_BUNDLE ${MAIN_MAC_SOURCES})
        SET_CEF_EXECUTABLE_TARGET_PROPERTIES(${helper_target})
        add_dependencies(${helper_target} ${SHARED_HELPER_TARGET})
        target_link_libraries(${helper_target} ${SHARED_HELPER_TARGET})
        set_target_properties(${helper_target} PROPERTIES
            MACOSX_BUNDLE_INFO_PLIST ${helper_info_plist}
            OUTPUT_NAME ${helper_output_name}
        )

        if(USE_SANDBOX)
            target_link_libraries(${helper_target} cef_sandbox_lib)
        endif()

        add_dependencies(${CURRENT_TARGET} ${helper_target})

        add_custom_command(
            TARGET ${CURRENT_TARGET}
            POST_BUILD
            COMMAND ${CMAKE_COMMAND} -E copy_directory
                    "${SYNTHESIS_OUT_DIR}/${helper_output_name}.app"
                    "${SYNTHESIS_APP}/Contents/Frameworks/${helper_output_name}.app"
            VERBATIM
            )
    endforeach()
endif()

# TODO: Linux

include_directories(BEFORE ${CMAKE_CURRENT_LIST_DIR}/inc)
