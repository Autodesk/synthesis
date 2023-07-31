#pragma once
#ifndef SYNTHESIS_OFFSCREEN_CEF_CLIENT_H_
#define SYNTHESIS_OFFSCREEN_CEF_CLIENT_H_

#include <include/cef_client.h>
#include <include/cef_load_handler.h>
#include <include/cef_render_handler.h>
#include <include/wrapper/cef_helpers.h>

#include <cstdint>
#include <mutex>

namespace synthesis {

class OffscreenCefClient : public CefClient {
public:
    OffscreenCefClient(int width, int height);

protected:
    virtual CefRefPtr<CefRenderHandler> GetRenderHandler() override;
    virtual CefRefPtr<CefLoadHandler> GetLoadHandler() override;

private:
    CefRefPtr<CefRenderHandler> renderHandler;
    CefRefPtr<CefLoadHandler> loadHandler;

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
    int m_width;
    int m_height;

    std::mutex textureBufferGuard;
    int8_t* browserTextureBuffer;

    IMPLEMENT_REFCOUNTING(OffscreenCefRenderHandler);
};

class OffscreenCefLoadHandler : public CefLoadHandler {
public:
    CefRefPtr<CefBrowser> GetBrowserReference();

private:
    virtual void OnLoadStart(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, TransitionType transitionType) override;

    CefRefPtr<CefBrowser> browserReference;

    IMPLEMENT_REFCOUNTING(OffscreenCefLoadHandler);
};

} // namespace synthesis

#endif // SYNTHESIS_OFFSCREEN_CEF_CLIENT_H_
