#include "client_util.h"

#include <include/views/cef_browser_view.h>
#include <include/views/cef_window.h>
#include <include/wrapper/cef_helpers.h>

#include <sstream>
#include <string>

#include "client_manager.h"

namespace synthesis {
namespace shared {

void OnTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title) {
    CEF_REQUIRE_UI_THREAD();

#if defined(OS_WIN) || defined(OS_LINUX)
    // TODO
#endif
    {
        PlatformTitleChange(browser, title);
    }
}

void OnAfterCreated(CefRefPtr<CefBrowser> browser) {
    ClientManager::GetInstance()->OnAfterCreated(browser);
}

bool DoClose(CefRefPtr<CefBrowser> browser) {
    ClientManager::GetInstance()->DoClose(browser);

    return false;
}

void OnBeforeClose(CefRefPtr<CefBrowser> browser) {
    CEF_REQUIRE_UI_THREAD();

    ClientManager::GetInstance()->OnBeforeClose(browser);
}

std::string DumpRequestContents(CefRefPtr<CefRequest> request) {
    std::stringstream ss;

    ss << "URL: " << std::string(request->GetURL());
    ss << "\nMethod: " << std::string(request->GetMethod());

    CefRequest::HeaderMap headerMap;
    request->GetHeaderMap(headerMap);

    if (headerMap.size() > 0) {
        ss << "\nHeaders:";

        for (auto it = headerMap.begin(); it != headerMap.end(); ++it) {
            ss << "\n\t" << std::string((*it).first) << ": " << std::string((*it).second);
        }
    }

    // TODO: Post data dump?

    return ss.str();
}

} // namespace shared
} // namespace synthesis
