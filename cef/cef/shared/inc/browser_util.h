#ifndef SYNTHESIS_CEF_BROWSER_UTIL_H_
#define SYNTHESIS_CEF_BROWSER_UTIL_H_
#pragma once

#include <include/cef_client.h>

namespace synthesis {
namespace shared {

void CreateBrowser(CefRefPtr<CefClient> client, const CefString& url, const CefBrowserSettings& settings);

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_BROWSER_UTIL_H_
