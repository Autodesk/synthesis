#ifndef SYNTHESIS_CEF_CLIENT_UTIL_H_
#define SYNTHESIS_CEF_CLIENT_UTIL_H_
#pragma once

#include <include/cef_client.h>

namespace synthesis {
namespace shared {

void OnTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title);

void OnAfterCreated(CefRefPtr<CefBrowser> browser);
bool DoClose(CefRefPtr<CefBrowser> browser);
void OnBeforeClose(CefRefPtr<CefBrowser> browser);

void PlatformTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title);

std::string DumpRequestContents(CefRefPtr<CefRequest> request);

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_CLIENT_UTIL_H_
