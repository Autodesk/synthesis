#include "client_manager.h"

#include <include/cef_app.h>
#include <include/wrapper/cef_helpers.h>

#include <memory>

namespace synthesis {
namespace shared {
namespace {

    ClientManager* manager = nullptr;

} // namespace

ClientManager::ClientManager() : isClosing(false) {
    manager = this;
}

ClientManager::~ClientManager() {
    DCHECK(threadChecker.CalledOnValidThread());
    DCHECK(browserList.empty());
    manager = nullptr;
}

ClientManager* ClientManager::GetInstance() /* static */ {
    CEF_REQUIRE_UI_THREAD();
    DCHECK(manager);
    return manager;
}

void ClientManager::OnAfterCreated(CefRefPtr<CefBrowser> browser) {
    DCHECK(threadChecker.CalledOnValidThread());
    browserList.push_back(browser);
}

void ClientManager::DoClose(CefRefPtr<CefBrowser> browser) {
    DCHECK(threadChecker.CalledOnValidThread());

    if (browserList.size() == 1) {
        CefQuitMessageLoop();
    }
}

void ClientManager::OnBeforeClose(CefRefPtr<CefBrowser> browser) {
    DCHECK(threadChecker.CalledOnValidThread());

    for (auto bit = browserList.begin(); bit != browserList.end(); ++bit) {
        if ((*bit)->IsSame(browser)) {
            browserList.erase(bit);
            break;
        }
    }

    if (browserList.empty()) {
        CefQuitMessageLoop();
    }
}

void ClientManager::CloseAllBrowsers(bool force) {
    DCHECK(threadChecker.CalledOnValidThread());

    if (browserList.empty()) {
        return;
    }

    for (auto it = browserList.begin(); it != browserList.end(); ++it) {
        (*it)->GetHost()->CloseBrowser(force);
    }
}

bool ClientManager::IsClosing() const {
    DCHECK(threadChecker.CalledOnValidThread());
    return isClosing;
}

} // namespace shared
} // namespace synthesis
