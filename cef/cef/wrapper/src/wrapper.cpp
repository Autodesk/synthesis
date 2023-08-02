#include "wrapper.h"

#include <include/cef_app.h>
#include <include/cef_browser.h>

#include "core.h"
#include "offscreen_cef_client.h"
#include "offscreen_cef_app.h"

SYNTHESIS_EXPORT void RunCefInterop() {
    CefMainArgs mainArgs;
    CefSettings settings;
    CefRefPtr<synthesis::OffscreenCefApp> app(new synthesis::OffscreenCefApp());
    CefInitialize(mainArgs, settings, app, nullptr);
    CefRunMessageLoop();
    CefShutdown();
}
