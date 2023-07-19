#ifndef SYNTHESIS_CEF_CLIENT_MANAGER_H_
#define SYNTHESIS_CEF_CLIENT_MANAGER_H_
#pragma once

#include <include/base/cef_thread_checker.h>
#include <include/cef_browser.h>

#include <list>
#include <memory>

namespace synthesis {

class ClientManager {
private:
    using BrowserList = std::list<CefRefPtr<CefBrowser>>;

public:
    ClientManager();
    ~ClientManager();

    static std::shared_ptr<ClientManager> GetInstance();

    void OnAfterCreated(CefRefPtr<CefBrowser> browser);
    void DoClose(CefRefPtr<CefBrowser> browser);
    void OnBeforeClose(CefRefPtr<CefBrowser> browser);

    void CloseAllBrowsers(bool force_close);

    bool IsClosing() const;

private:
    base::ThreadChecker thread_checker;
    bool is_closing;

    BrowserList browser_list;
};

} // namespace synthesis

#endif // SYNTHESIS_CEF_CLIENT_MANAGER_H_
