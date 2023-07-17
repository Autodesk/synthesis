#include "internal/client.h"

#include <include/views/cef_browser_view.h>
#include <include/views/cef_window.h>
#include <include/wrapper/cef_helpers.h>

#include <sstream>
#include <string>

#include "internal/client_manager.h"

Client::Client() {}

void Client::OnTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title) {
    CEF_REQUIRE_UI_THREAD();

#if defined(OS_WIN) || defined(OS_LINUX)
    CefRefPtr<CefBrowserView> browser_view = CefBrowserView::GetForBrowser(browser);

    if (browser_view) {
        CefRefPtr<CefWindow> window = browser_view->GetWindow();

        if (window) {
            window->SetTitle(title);
        }
    } else
#endif // defined(OS_WIN) || defined(OS_LINUX)
    {
        PlatformTitleChange(browser, title);
    }
}

void Client::OnAfterCreated(CefRefPtr<CefBrowser> browser) {
    CEF_REQUIRE_UI_THREAD();

    ClientManager::GetInstance()->OnAfterCreated(browser);
}

bool Client::DoClose(CefRefPtr<CefBrowser> browser) {
    CEF_REQUIRE_UI_THREAD();

    ClientManager::GetInstance()->DoClose(browser);
    return false;
}

void Client::OnBeforeClose(CefRefPtr<CefBrowser> browser) {
    CEF_REQUIRE_UI_THREAD();

    ClientManager::GetInstance()->OnBeforeClose(browser);
}

// TODO: DumpRequestContents
