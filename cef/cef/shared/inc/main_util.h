#pragma once
#ifndef SYNTHESIS_CEF_MAIN_UTIL_H_
#define SYNTHESIS_CEF_MAIN_UTIL_H_

#include <include/cef_command_line.h>

namespace synthesis {
namespace shared {

CefRefPtr<CefCommandLine> CreateCommandLine(const CefMainArgs& mainArgs);

enum ProcessType {
    BROWSER,
    RENDERER,
    OTHER
};

ProcessType GetProcessType(const CefRefPtr<CefCommandLine>& commandLine);

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_MAIN_UTIL_H_
