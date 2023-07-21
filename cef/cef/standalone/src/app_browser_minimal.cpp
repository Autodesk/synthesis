#include "client.h"
#include "app_factory.h"
#include "browser_util.h"

namespace synthesis {
namespace {

const char url[] = "https://www.google.com";

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
        shared::CreateBrowser(new Client(), url, CefBrowserSettings());
    }

private:
    IMPLEMENT_REFCOUNTING(BrowserApp);
    DISALLOW_COPY_AND_ASSIGN(BrowserApp);
};

} // namespace

CefRefPtr<CefApp> shared::CreateBrowserProcessApp() {
    return new BrowserApp();
}

} // namespace synthesis
