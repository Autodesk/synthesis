#include "shared_main.h"

#include <include/cef_sandbox_win.h>

#include <windows.h>

#include "app_factory.h"
#include "client_manager.h"
#include "main_util.h"

namespace synthesis {
namespace shared {

int APIENTRY wWinMain(HINSTANCE hInstance) {
    CefEnableHighDPISupport();

    void* sandboxInfo = nullptr;

#if defined(CEF_USE_SANDBOX)
    CefScopedSandboxInfo scopedSandbox;
    sandboxInfo = scopedSandbox.sandbox_info();
#endif // CEF_USE_SANDBOX

    CefMainArgs mainArgs(hInstance);
    CefRefPtr<CefCommandLine> commandLine = CreateCommandLine(mainArgs);
    CefRefPtr<CefApp> app;

    switch (GetProcessType(commandLine)) {
        case BROWSER:
            app = CreateBrowserProcessApp();
            break;
        case RENDERER:
            app = CreateRendererProcessApp();
            break;
        case OTHER:
            app = CreateOtherProcessApp();
            break;
    }

    int exitCode = CefExecuteProcess(mainArgs, app, sandboxInfo);

    if (exitCode >= 0) {
        return exitCode;
    }

    ClientManager manager;
    CefSettings settings;

#if !defined(CEF_USE_SANDBOX)
    settings.no_sandbox = true;
#endif // !CEF_USE_SANDBOX

    CefInitialize(mainArgs, settings, app, sandboxInfo);

    CefRunMessageLoop();
    CefShutdown();

    return 0;
}

} // namespace shared
} // namespace synthesis
