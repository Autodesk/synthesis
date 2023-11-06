#include "offscreen_cef_client.h"

#include <mutex>

#include "debug.h"

namespace synthesis {

OffscreenCefClient::OffscreenCefClient(int width, int height) {
    renderHandler = new OffscreenCefRenderHandler(width, height);
    loadHandler = new OffscreenCefLoadHandler();

    SYNTHESIS_DEBUG_LOG("Offscreen client created");
}

std::vector<int8_t> OffscreenCefClient::GetBrowserTextureBuffer() {
    return renderHandler->GetBrowserTextureBuffer();
}

CefRefPtr<CefRenderHandler> OffscreenCefClient::GetRenderHandler() {
    SYNTHESIS_DEBUG_LOG("Offscreen client render handler requested");

    return renderHandler;
}

CefRefPtr<CefLoadHandler> OffscreenCefClient::GetLoadHandler() {
    return loadHandler;
}

OffscreenCefRenderHandler::OffscreenCefRenderHandler(const int& width_, const int& height_) {
    this->width = width_;
    this->height = height_;

    browserTextureBuffer = new int8_t[width_ * height_ * 4];

    SYNTHESIS_DEBUG_LOG("Offscreen render handler created");
}

OffscreenCefRenderHandler::~OffscreenCefRenderHandler() {
    delete[] browserTextureBuffer;

    SYNTHESIS_DEBUG_LOG("Offscreen render handler destroyed");
}

std::vector<int8_t> OffscreenCefRenderHandler::GetBrowserTextureBuffer() {
    std::lock_guard<std::mutex> lock(textureBufferGuard);
    std::vector<int8_t> buffer(browserTextureBuffer, browserTextureBuffer + width * height * 4);
    return buffer;
}

bool OffscreenCefRenderHandler::GetRootScreenRect(CefRefPtr<CefBrowser> browser, CefRect& rect) {
    this->GetViewRect(browser, rect);
    return true;
}

void OffscreenCefRenderHandler::GetViewRect(CefRefPtr<CefBrowser> browser, CefRect& rect) {
    rect = CefRect(0, 0, this->width, this->height);
}

void OffscreenCefRenderHandler::OnPaint(CefRefPtr<CefBrowser> browser, PaintElementType type, const RectList& dirtyRects, const void* buffer, int width_, int height_) {
    std::lock_guard<std::mutex> lock(textureBufferGuard);
    memcpy(browserTextureBuffer, buffer, width_ * height_ * 4);

    SYNTHESIS_DEBUG_LOG("Offscreen render handler painted");
}

void OffscreenCefRenderHandler::OnAcceleratedPaint(CefRefPtr<CefBrowser> browser, PaintElementType type, const RectList& dirtyRects, void* shared_handle) {
    SYNTHESIS_DEBUG_LOG("Offscreen render handler accelerated painted");
}

CefRefPtr<CefBrowser> OffscreenCefLoadHandler::GetBrowserReference() {
    return browserReference;
}

void OffscreenCefLoadHandler::OnLoadStart(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, TransitionType transitionType) {
    browserReference = browser;
}

} // namespace synthesis
