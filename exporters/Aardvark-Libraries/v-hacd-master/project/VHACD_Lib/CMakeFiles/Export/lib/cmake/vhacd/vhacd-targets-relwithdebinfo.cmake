#----------------------------------------------------------------
# Generated CMake target import file for configuration "RelWithDebInfo".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "::vhacd" for configuration "RelWithDebInfo"
set_property(TARGET ::vhacd APPEND PROPERTY IMPORTED_CONFIGURATIONS RELWITHDEBINFO)
set_target_properties(::vhacd PROPERTIES
  IMPORTED_LINK_INTERFACE_LANGUAGES_RELWITHDEBINFO "CXX"
  IMPORTED_LOCATION_RELWITHDEBINFO "${_IMPORT_PREFIX}/lib/vhacd.lib"
  )

list(APPEND _IMPORT_CHECK_TARGETS ::vhacd )
list(APPEND _IMPORT_CHECK_FILES_FOR_::vhacd "${_IMPORT_PREFIX}/lib/vhacd.lib" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
