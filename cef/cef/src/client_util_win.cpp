#include "client_util.h"

#include <include/cef_browser.h>

#include <windows.h>
#include <string>

namespace synthesis {
namespace shared {

void PlatformTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title) {
    CefWindowHandle hand = browser->GetHost()->GetWindowHandle();
    SetWindowText(hand, std::wstring(title).c_str());
}

} // namespace shared
} // namespace synthesis
