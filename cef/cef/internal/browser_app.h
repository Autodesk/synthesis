#ifndef SYNTHESIS_CEF_BROWSER_APP_H_
#define SYNTHESIS_CEF_BROWSER_APP_H_
#pragma once

#include <include/cef_app.h>
#include <include/cef_client.h>

namespace synthesis {

void CreateBrowser(CefRefPtr<CefClient> client, const CefString& url, const CefBrowserSettings& settings);

CefRefPtr<CefApp> CreateBrowserProcessApp();

} // namespace synthesis

#endif // SYNTHESIS_CEF_BROWSER_APP_H_
