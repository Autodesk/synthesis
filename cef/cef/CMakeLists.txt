set(SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/client_manager.cpp
    ${CMAKE_CURRENT_LIST_DIR}/src/client.cpp
)

set(HEADERS
    ${CMAKE_CURRENT_LIST_DIR}/internal/client_manager.h
    ${CMAKE_CURRENT_LIST_DIR}/internal/client.h
)

set(MAC_SOURCES
    ${CMAKE_CURRENT_LIST_DIR}/src/client_mac.mm
)

if(OS_MAC)
    set(SOURCES ${SOURCES} ${MAC_SOURCES})
endif()

add_library(${PROJECT_NAME} SHARED ${SOURCES} ${HEADERS})
include_directories(BEFORE ${CMAKE_CURRENT_LIST_DIR})
target_link_libraries(${PROJECT_NAME} libcef_dll_wrapper ${CEF_STANDARD_LIBS})
