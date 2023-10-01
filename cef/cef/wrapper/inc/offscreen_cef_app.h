#pragma once
#ifndef SYNTHESIS_OFFSCREEN_CEF_APP_H_
#define SYNTHESIS_OFFSCREEN_CEF_APP_H_

#include <include/cef_app.h>

namespace synthesis {

class OffscreenCefApp : public CefApp {
    IMPLEMENT_REFCOUNTING(OffscreenCefApp);
};

} // namespace synthesis

#endif // SYNTHESIS_OFFSCREEN_CEF_APP_H_
