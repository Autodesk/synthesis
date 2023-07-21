#ifndef SYNTHESIS_CEF_CLIENT_MANAGER_H_
#define SYNTHESIS_CEF_CLIENT_MANAGER_H_
#pragma once

#include <include/base/cef_thread_checker.h>
#include <include/cef_browser.h>

#include <list>

namespace synthesis {
namespace shared {

class ClientManager {
private:
    using BrowserList = std::list<CefRefPtr<CefBrowser>>;

public:
    ClientManager();
    ~ClientManager();

    static ClientManager* GetInstance();

    void OnAfterCreated(CefRefPtr<CefBrowser> browser);
    void DoClose(CefRefPtr<CefBrowser> browser);
    void OnBeforeClose(CefRefPtr<CefBrowser> browser);

    void CloseAllBrowsers(bool forceClose);

    bool IsClosing() const;

private:
    base::ThreadChecker threadChecker;
    bool isClosing;

    BrowserList browserList;
};

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_CLIENT_MANAGER_H_
