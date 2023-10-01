#include "browser_util.h"

#include <include/cef_command_line.h>
#include <include/views/cef_browser_view.h>
#include <include/views/cef_window.h>
#include <include/wrapper/cef_helpers.h>

namespace synthesis {
namespace shared {
namespace {

class WindowDelegate : public CefWindowDelegate {
public:
    explicit WindowDelegate(CefRefPtr<CefBrowserView> browserView) : browserView(browserView) {}

    void OnWindowCreated(CefRefPtr<CefWindow> window) override {
        window->AddChildView(browserView);
        window->Show();

        browserView->RequestFocus();
    }

    void OnWindowDestroyed(CefRefPtr<CefWindow> window) override {
        browserView = nullptr;
    }

    bool CanClose(CefRefPtr<CefWindow> window) override {
        CefRefPtr<CefBrowser> browser = browserView->GetBrowser();

        return browser ? browser->GetHost()->TryCloseBrowser() : true;
    }

    CefSize GetPreferredSize(CefRefPtr<CefView> view) override {
        return CefSize(800, 600);
    }

    CefSize GetMinimumSize(CefRefPtr<CefView> view) override {
        return CefSize(200, 100);
    }

private:
    CefRefPtr<CefBrowserView> browserView;

    IMPLEMENT_REFCOUNTING(WindowDelegate);
    DISALLOW_COPY_AND_ASSIGN(WindowDelegate);
};

} // namespace

void CreateBrowser(CefRefPtr<CefClient> client, const CefString& url, const CefBrowserSettings& settings) {
    CEF_REQUIRE_UI_THREAD();

#if defined(OS_WINDOWS) || defined(OS_LINUX)
    CefRefPtr<CefCommandLine> command_line = CefCommandLine::GetGlobalCommandLine();
    const bool use_views = command_line->HasSwitch("use-views");
#else // ^^^ defined(OS_WINDOWS) || defined(OS_LINUX) ^^^ // vvv !defined(OS_WINDOWS) && !defined(OS_LINUX) vvv
    const bool use_views = false;
#endif // !defined(OS_WINDOWS) && !defined(OS_LINUX)

    if (use_views) {
        CefRefPtr<CefBrowserView> browser_view = 
            CefBrowserView::CreateBrowserView(client, url, settings, nullptr, nullptr, nullptr);

        CefWindow::CreateTopLevelWindow(new WindowDelegate(browser_view));
    } else {
        CefWindowInfo window_info;

#if defined(OS_WINDOWS)
        window_info.SetAsPopup(nullptr, "Synthesis");
#endif // defined(OS_WINDOWS)

        CefBrowserHost::CreateBrowser(window_info, client, url, settings, nullptr, nullptr);
    }
}

} // namespace shared
} // namespace synthesis
