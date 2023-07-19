#ifndef SYNTHESIS_CEF_CLIENT_H_
#define SYNTHESIS_CEF_CLIENT_H_
#pragma once

#include <include/cef_client.h>

namespace synthesis {

class Client: public CefClient, public CefDisplayHandler, public CefLifeSpanHandler {
public:
    Client();

    CefRefPtr<CefDisplayHandler> GetDisplayHandler() override;
    CefRefPtr<CefLifeSpanHandler> GetLifeSpanHandler() override;

    void OnTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title) override;

    void OnAfterCreated(CefRefPtr<CefBrowser> browser) override;
    bool DoClose(CefRefPtr<CefBrowser> browser) override;
    void OnBeforeClose(CefRefPtr<CefBrowser> browser) override;

private:
    IMPLEMENT_REFCOUNTING(Client);
    DISALLOW_COPY_AND_ASSIGN(Client);
};

} // namespace synthesis

#endif // SYNTHESIS_CEF_CLIENT_H_
