#include "offscreen_cef_client.h"

#include <mutex>

#include "debug.h"

namespace synthesis {

OffscreenCefClient::OffscreenCefClient(int width, int height) {
    renderHandler = new OffscreenCefRenderHandler(width, height);
    SYNTHESIS_DEBUG_LOG("Offscreen client created");
}

CefRefPtr<CefRenderHandler> OffscreenCefClient::GetRenderHandler() {
    return renderHandler;
}

CefRefPtr<CefLoadHandler> OffscreenCefClient::GetLoadHandler() {
    return loadHandler;
}

OffscreenCefRenderHandler::OffscreenCefRenderHandler(const int& width, const int& height) {
    this->m_width = width;
    this->m_height = height;

    browserTextureBuffer = new int8_t[width * height * 4];

    SYNTHESIS_DEBUG_LOG("Offscreen render handler created");
}

OffscreenCefRenderHandler::~OffscreenCefRenderHandler() {
    delete[] browserTextureBuffer;

    SYNTHESIS_DEBUG_LOG("Offscreen render handler destroyed");
}

bool OffscreenCefRenderHandler::GetRootScreenRect(CefRefPtr<CefBrowser> browser, CefRect& rect) {
    this->GetViewRect(browser, rect);
    return true;
}

void OffscreenCefRenderHandler::GetViewRect(CefRefPtr<CefBrowser> browser, CefRect& rect) {
    rect = CefRect(0, 0, m_width, m_height);
}

void OffscreenCefRenderHandler::OnPaint(CefRefPtr<CefBrowser> browser, PaintElementType type, const RectList& dirtyRects, const void* buffer, int width, int height) {
    
    m_width = width;
    m_height = height;

    std::lock_guard<std::mutex> lock(textureBufferGuard);
    memcpy(browserTextureBuffer, buffer, width * height * 4);
}

CefRefPtr<CefBrowser> OffscreenCefLoadHandler::GetBrowserReference() {
    return browserReference;
}

void OffscreenCefLoadHandler::OnLoadStart(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, TransitionType transitionType) {
    browserReference = browser;
}

} // namespace synthesis
