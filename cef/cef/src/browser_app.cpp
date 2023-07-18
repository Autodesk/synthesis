#include "internal/browser_app.h"

#include <include/cef_app.h>
#include <include/cef_command_line.h>
#include <include/views/cef_browser_view.h>
#include <include/views/cef_window.h>
#include <include/wrapper/cef_helpers.h>

#include "internal/client.h"

namespace synthesis {
namespace {

const char url[] = "https://www.google.com";

class WindowDelegate : public CefWindowDelegate {
public:
    explicit WindowDelegate(CefRefPtr<CefBrowserView> browser_view) : browser_view(browser_view) {}

    void OnWindowCreated(CefRefPtr<CefWindow> window) override {
        window->AddChildView(browser_view);
        window->Show();

        browser_view->RequestFocus();
    }

    void OnWindowDestroyed(CefRefPtr<CefWindow> window) override {
        browser_view = nullptr;
    }

    bool CanClose(CefRefPtr<CefWindow> window) override {
        CefRefPtr<CefBrowser> browser = browser_view->GetBrowser();

        return browser ? browser->GetHost()->TryCloseBrowser() : true;
    }

    CefSize GetPreferredSize(CefRefPtr<CefView> view) override {
        return CefSize(800, 600);
    }

    CefSize GetMinimumSize(CefRefPtr<CefView> view) override {
        return CefSize(200, 100);
    }

private:
    CefRefPtr<CefBrowserView> browser_view;

    IMPLEMENT_REFCOUNTING(WindowDelegate);
    DISALLOW_COPY_AND_ASSIGN(WindowDelegate);
};

void CreateSynthesisBrowser(CefRefPtr<CefClient> client, const CefString& url, const CefBrowserSettings& settings) {
    CEF_REQUIRE_UI_THREAD();

#if defined(OS_WIN) || defined(OS_LINUX)
    CefRefPtr<CefCommandLine> command_line = CefCommandLine::GetGlobalCommandLine();
    const bool use_views = command_line->HasSwitch("use-views");
#else // ^^^ defined(OS_WIN) || defined(OS_LINUX) ^^^ // vvv !defined(OS_WIN) && !defined(OS_LINUX) vvv
    const bool use_views = false;
#endif // !defined(OS_WIN) && !defined(OS_LINUX)

    if (use_views) {
        CefRefPtr<CefBrowserView> browser_view = 
            CefBrowserView::CreateBrowserView(client, url, settings, nullptr, nullptr, nullptr);

        CefWindow::CreateTopLevelWindow(new WindowDelegate(browser_view));
    } else {
        CefWindowInfo window_info;

#if defined(OS_WIN)
        window_info.SetAsPopup(nullptr, "Synthesis");
#endif // defined(OS_WIN)

        CefBrowserHost::CreateBrowser(window_info, client, url, settings, nullptr, nullptr);
    }
}

} // namespace

class BrowserApp : public CefApp, public CefBrowserProcessHandler {
public:
    BrowserApp() {}

    CefRefPtr<CefBrowserProcessHandler> GetBrowserProcessHandler() override {
        return this;
    }

    void OnBeforeCommandLineProcessing(const CefString& process_type, CefRefPtr<CefCommandLine> command_line) override {
        if (process_type.empty()) {
#if defined(OS_MACOSX)
            command_line->AppendSwitch("use-mock-keychain");
#endif // defined(OS_MACOSX)
        }
    }

    void OnContextInitialized() override {
        CreateSynthesisBrowser(new Client(), url, CefBrowserSettings());
    }

private:
    IMPLEMENT_REFCOUNTING(BrowserApp);
    DISALLOW_COPY_AND_ASSIGN(BrowserApp);
};

CefRefPtr<CefApp> CreateBrowserProcessApp() {
    return new BrowserApp();
}

} // namespace synthesis
