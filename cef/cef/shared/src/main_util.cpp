#include "main_util.h"

#if defined(OS_WIN)
#include <windows.h>
#endif // defined(OS_WIN)

namespace synthesis {
namespace shared {
namespace {

const char processType[] = "type";
const char renderProcess[] = "renderer";
#if defined(OS_LINUX)
const char zygoteProcess[] = "zygote";
#endif // defined(OS_LINUX)

} // namespace

CefRefPtr<CefCommandLine> CreateCommandLine(const CefMainArgs& mainArgs) {
    auto commandLine = CefCommandLine::CreateCommandLine();
#if defined(OS_WIN)
    commandLine->InitFromString(::GetCommandLineW());
#else // ^^^ defined(OS_WIN) ^^^ / vvv !defined(OS_WIN) vvv
    commandLine->InitFromArgv(mainArgs.argc, mainArgs.argv);
#endif // !defined(OS_WIN)
    return commandLine;
}

ProcessType GetProcessType(const CefRefPtr<CefCommandLine>& commandLine) {
    if (!commandLine->HasSwitch(processType)) {
        return BROWSER;
    }

    const std::string& process = commandLine->GetSwitchValue(processType);

    if (process == renderProcess) {
        return RENDERER;
    }

#if defined(OS_LINUX)
    if (process == zygoteProcess) {
        return RENDERER;
    }
#endif // defined(OS_LINUX)

    return OTHER;
}

} // namespace shared
} // namespace synthesis
