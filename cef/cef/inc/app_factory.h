#ifndef SYNTHESIS_CEF_APP_FACTORY_H_
#define SYNTHESIS_CEF_APP_FACTORY_H_
#pragma once

#include <include/cef_app.h>

namespace synthesis {
namespace shared {

CefRefPtr<CefApp> CreateBrowserProcessApp();
CefRefPtr<CefApp> CreateRendererProcessApp();
CefRefPtr<CefApp> CreateOtherProcessApp();

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_APP_FACTORY_H_
