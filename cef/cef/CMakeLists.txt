if(OS_MAC)
    set(SHARED_RESOURCES_SRCS
        ../res/mac/English.lproj/InfoPlist.strings
        ../res/mac/English.lproj/MainMenu.xib
        ../res/mac/Info.plist
        ../res/mac/shared.icns
    )

    set(SHARED_INFO_PLIST ../res/mac/Info.plist)
    set(SHARED_HELPER_INFO_PLIST ../res/mac/helper-Info.plist)
elseif(OS_WINDOWS)
    set(SHARED_RESOURCES_SRCS
        ../res/win/big.ico
        ../res/win/resource.h
        ../res/win/shared.rc
        ../res/win/small.ico
    )

    set(SHARED_EXE_MANIFEST ../res/win/shared.exe.manifest)
    set(SHARED_COMPATIBILITY_MANIFEST ../res/win/compatibility.manifest)
endif()

if(OS_MACOSX OR OS_WINDOWS)
    set_property(GLOBAL PROPERTY USE_FOLDERS ON)

    macro(SET_CEF_SOURCE_GROUPS srcs)
        foreach(FILE ${srcs}) 
            get_filename_component(ABS_FILE ${FILE} ABSOLUTE)
            get_filename_component(PARENT_DIR ${ABS_FILE} DIRECTORY)

            string(REPLACE "${CMAKE_SOURCE_DIR}/cef/" "" GROUP "${PARENT_DIR}")
            string(REPLACE "/" "\\" GROUP "${GROUP}")

            source_group(${GROUP} FILES ${FILE})
        endforeach()
    endmacro()
endif()

if(GEN_NINJA OR GEN_MAKEFILES)
    set(CEF_TARGET_OUT_DIR "${CMAKE_BINARY_DIR}/${CMAKE_BUILD_TYPE}")
else()
    set(CEF_TARGET_OUT_DIR "${CMAKE_BINARY_DIR}/$<CONFIGURATION>")
endif()

macro(SET_CEF_PROPERTIES target)
    set_target_properties(${target} PROPERTIES
                        ARCHIVE_OUTPUT_DIRECTORY ${CEF_TARGET_OUT_DIR}
                        RUNTIME_OUTPUT_DIRECTORY ${CEF_TARGET_OUT_DIR}
                        LIBRARY_OUTPUT_DIRECTORY ${CEF_TARGET_OUT_DIR})

    if(OS_MACOSX OR OS_WINDOWS)
        set_property(TARGET ${target} PROPERTY FOLDER "cef")
    endif()
endmacro()

macro(SET_CEF_LIBRARY_TARGET_PROPERTIES target)
      SET_LIBRARY_TARGET_PROPERTIES(${target})
    SET_CEF_PROPERTIES(${target})
endmacro()

macro(SET_CEF_EXECUTABLE_TARGET_PROPERTIES target)
    SET_EXECUTABLE_TARGET_PROPERTIES(${target})
    SET_CEF_PROPERTIES(${target})

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
                -outputresource:"${CEF_TARGET_OUT_DIR}/${target}.exe"\;\#1
            COMMENT "Adding manifest..."
        )
  endif()
endmacro()

add_subdirectory(shared)
add_subdirectory(standalone)
