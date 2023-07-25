#ifndef SYNTHESIS_CEF_HTML_RESOURCE_HANDLER_H_
#define SYNTHESIS_CEF_HTML_RESOURCE_HANDLER_H_
#pragma once

#include <include/cef_resource_handler.h>
#include <include/cef_scheme.h>
#include <include/wrapper/cef_helpers.h>

namespace synthesis {
namespace shared {

class HTMLResourceHandler : public CefResourceHandler {
public:
    HTMLResourceHandler();

    virtual bool Open(CefRefPtr<CefRequest> request, bool& handleRequest, CefRefPtr<CefCallback> callback) override;
    virtual void GetResponseHeaders(CefRefPtr<CefResponse> response, int64& responseLength, CefString& redirectUrl) override;
    virtual bool Read(void* dataOut, int bytesToRead, int& bytesRead, CefRefPtr<CefResourceReadCallback> callback) override;
    virtual void Cancel() override;

private:
    CefRefPtr<CefStreamReader> fileStream;
    int offset = 0;

    IMPLEMENT_REFCOUNTING(HTMLResourceHandler);
};

class HTMLSchemeHandlerFactory : public CefSchemeHandlerFactory {
public:
    virtual CefRefPtr<CefResourceHandler> Create(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, const CefString& schemeName, CefRefPtr<CefRequest> request) override;

private:
    IMPLEMENT_REFCOUNTING(HTMLSchemeHandlerFactory);
};

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_HTML_RESOURCE_HANDLER_H_
