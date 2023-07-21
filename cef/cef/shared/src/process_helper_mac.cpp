#include "process_helper_mac.h"

#include <include/cef_app.h>
#include <include/wrapper/cef_library_loader.h>

#include "app_factory.h"
#include "main_util.h"

#if defined(CEF_USE_SANDBOX)
#include <include/cef_sandbox_mac.h>
#endif // defined(CEF_USE_SANDBOX)

namespace synthesis {
namespace shared {

int main(int argc, char* argv[]) {
#if defined(CEF_USE_SANDBOX)
    CefScopedSandboxContext sandboxContext;

    if (!sandboxContext.Initialize(argc, argv)) {
        return 1;
    }
#endif // defined(CEF_USE_SANDBOX)

    CefScopedLibraryLoader libraryLoader;

    if (!libraryLoader.LoadInHelper()) {
        return 1;
    }

    CefMainArgs mainArgs(argc, argv);
    CefRefPtr<CefCommandLine> commandLine = CreateCommandLine(mainArgs);
    CefRefPtr<CefApp> app;

    switch (GetProcessType(commandLine)) {
        case RENDERER:
            app = CreateRendererProcessApp();
            break;
        case OTHER:
            app = CreateOtherProcessApp();
            break;
        default:
            break; // Browser process is handled by main_mac.mm
    }

    return CefExecuteProcess(mainArgs, app, nullptr);
}

} // namespace shared
} // namespace synthesis
