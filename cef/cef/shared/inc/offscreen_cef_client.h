#ifndef SYNTHESIS_OFFSCREEN_CEF_CLIENT_H_
#define SYNTHESIS_OFFSCREEN_CEF_CLIENT_H_
#pragma once

#include <include/cef_client.h>
#include <include/cef_render_handler.h>
#include <include/wrapper/cef_helpers.h>

#include <cstdint>
#include <mutex>

namespace synthesis {
namespace shared {

class OffscreenCefClient : public CefClient {
public:
    OffscreenCefClient(int width, int height);

protected:
    virtual CefRefPtr<CefRenderHandler> GetRenderHandler() override;

private:
    CefRefPtr<CefRenderHandler> renderHandler;

    IMPLEMENT_REFCOUNTING(OffscreenCefClient);
};

class OffscreenCefRenderHandler : public CefRenderHandler {
public:
    OffscreenCefRenderHandler(const int& width, const int& height);
    ~OffscreenCefRenderHandler();

protected:
    virtual bool GetRootScreenRect(CefRefPtr<CefBrowser> browser, CefRect& rect) override;
    virtual void GetViewRect(CefRefPtr<CefBrowser> browser, CefRect& rect) override;
    virtual void OnPaint(CefRefPtr<CefBrowser> browser, PaintElementType type, const RectList& dirtyRects, const void* buffer, int width, int height) override;

private:
    int width;
    int height;

    static std::mutex textureBufferGuard;
    static int8_t* browserTextureBuffer;
};

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_OFFSCREEN_CEF_CLIENT_H_
