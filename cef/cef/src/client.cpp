#include "client.h"

#include "client_util.h"

namespace synthesis {

Client::Client() {}

CefRefPtr<CefDisplayHandler> Client::GetDisplayHandler() { 
    return this; 
}

CefRefPtr<CefLifeSpanHandler> Client::GetLifeSpanHandler() { 
    return this;
}

void Client::OnTitleChange(CefRefPtr<CefBrowser> browser, const CefString& title) {
    shared::OnTitleChange(browser, title);
}

void Client::OnAfterCreated(CefRefPtr<CefBrowser> browser) {
    shared::OnAfterCreated(browser);
}

bool Client::DoClose(CefRefPtr<CefBrowser> browser) {
    return shared::DoClose(browser);
}

void Client::OnBeforeClose(CefRefPtr<CefBrowser> browser) {
    shared::OnBeforeClose(browser);
}

} // namespace synthesis
