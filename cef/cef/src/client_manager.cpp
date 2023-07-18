#include "internal/client_manager.h"

#include <include/cef_app.h>
#include <include/wrapper/cef_helpers.h>

namespace synthesis {
namespace {
    ClientManager* manager = nullptr;
} // namespace

ClientManager::ClientManager() : is_closing(false) {
    manager = this;
}

ClientManager::~ClientManager() {
    DCHECK(thread_checker.CalledOnValidThread());
    DCHECK(browser_list.empty());
    manager = nullptr;
}

ClientManager* ClientManager::GetInstance() /* static */ {
    CEF_REQUIRE_UI_THREAD();
    DCHECK(manager);
    return manager;
}

void ClientManager::OnAfterCreated(CefRefPtr<CefBrowser> browser) {
    DCHECK(thread_checker.CalledOnValidThread());
    browser_list.push_back(browser);
}

void ClientManager::DoClose(CefRefPtr<CefBrowser> browser) {
    DCHECK(thread_checker.CalledOnValidThread());

    if (browser_list.size() == 1) {
        CefQuitMessageLoop();
    }
}

void ClientManager::OnBeforeClose(CefRefPtr<CefBrowser> browser) {
    DCHECK(thread_checker.CalledOnValidThread());

    for (auto bit = browser_list.begin(); bit != browser_list.end(); ++bit) {
        if ((*bit)->IsSame(browser)) {
            browser_list.erase(bit);
            break;
        }
    }

    if (browser_list.empty()) {
        CefQuitMessageLoop();
    }
}

void ClientManager::CloseAllBrowsers(bool force) {
    DCHECK(thread_checker.CalledOnValidThread());

    if (browser_list.empty()) {
        return;
    }

    for (auto it = browser_list.begin(); it != browser_list.end(); ++it) {
        (*it)->GetHost()->CloseBrowser(force);
    }
}

bool ClientManager::IsClosing() const {
    DCHECK(thread_checker.CalledOnValidThread());
    return is_closing;
}

} // namespace synthesis
