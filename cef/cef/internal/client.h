#ifndef SYNTHESIS_CEF_CLIENT_H_
#define SYNTHESIS_CEF_CLIENT_H_
#pragma once

#include <include/cef_client.h>

namespace synthesis {

class Client: public CefClient, public CefDisplayHandler, public CefLifeSpanHandler {
public:
    Client();

    CefRefPtr<CefDisplayHandler> GetDisplayHandler() override { return this; }
    CefRefPtr<CefLifeSpanHandler> GetLifeSpanHandler() override { return this; }

    void OnTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title) override;

    void OnAfterCreated(CefRefPtr<CefBrowser> browser) override;
    bool DoClose(CefRefPtr<CefBrowser> browser) override;
    void OnBeforeClose(CefRefPtr<CefBrowser> browser) override;

private:
    IMPLEMENT_REFCOUNTING(Client);
    DISALLOW_COPY_AND_ASSIGN(Client);
};

// TODO: Needed?
// void OnTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title);

// void OnAfterCreated(CefRefPtr<CefBrowser> browser);
// bool DoClose(CefRefPtr<CefBrowser> browser);
// void OnBeforeClose(CefRefPtr<CefBrowser> browser);

// Def needed

// Implemented based on platform `client_[platform][.cpp/.mm]`
void PlatformTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title);

// std::string DumpRequestContents(CefRefPtr<CefRequest> request);

} // namespace synthesis

#endif // SYNTHESIS_CEF_CLIENT_H_
