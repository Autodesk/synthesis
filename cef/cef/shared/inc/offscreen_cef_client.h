#pragma once
#ifndef SYNTHESIS_OFFSCREEN_CEF_CLIENT_H_
#define SYNTHESIS_OFFSCREEN_CEF_CLIENT_H_

#include <include/cef_client.h>
#include <include/cef_load_handler.h>
#include <include/cef_render_handler.h>
#include <include/wrapper/cef_helpers.h>

#include <cstdint>
#include <mutex>
#include <vector>

namespace synthesis {

class OffscreenCefRenderHandler;
class OffscreenCefLoadHandler;

class OffscreenCefClient : public CefClient {
public:
    OffscreenCefClient(int width, int height);

    std::vector<int8_t> GetBrowserTextureBuffer();

protected:
    virtual CefRefPtr<CefRenderHandler> GetRenderHandler() override;
    virtual CefRefPtr<CefLoadHandler> GetLoadHandler() override;

private:
    CefRefPtr<OffscreenCefRenderHandler> renderHandler;
    CefRefPtr<OffscreenCefLoadHandler> loadHandler;

    IMPLEMENT_REFCOUNTING(OffscreenCefClient);
};

class OffscreenCefRenderHandler : public CefRenderHandler {
public:
    OffscreenCefRenderHandler(const int& width, const int& height);
    ~OffscreenCefRenderHandler();

    std::vector<int8_t> GetBrowserTextureBuffer();

protected:
    virtual bool GetRootScreenRect(CefRefPtr<CefBrowser> browser, CefRect& rect) override;
    virtual void GetViewRect(CefRefPtr<CefBrowser> browser, CefRect& rect) override;
    virtual void OnPaint(CefRefPtr<CefBrowser> browser, PaintElementType type, const RectList& dirtyRects, const void* buffer, int width, int height) override;
    virtual void OnAcceleratedPaint(CefRefPtr<CefBrowser> browser, PaintElementType type, const RectList& dirtyRects, void* shared_handle) override;

private:
    int width;
    int height;

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
