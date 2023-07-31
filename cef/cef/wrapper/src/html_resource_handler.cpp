#include "html_resource_handler.h"

#include <include/wrapper/cef_stream_resource_handler.h>

#include <string>

#include "core.h"

namespace synthesis {

HTMLResourceHandler::HTMLResourceHandler() {}

bool HTMLResourceHandler::Open(CefRefPtr<CefRequest> request, bool& handleRequest, CefRefPtr<CefCallback> callback) {
    CefString url = request->GetURL();

    if (url == std::string(SYNTHESIS_HTML_SCHEME) + "index.html") {
        std::string htmlFilePath = "index.html";
        fileStream = CefStreamReader::CreateForFile(htmlFilePath);

        if (fileStream) {
            callback->Continue();
            handleRequest = true;
            return true;
        }
    }

    handleRequest = false;
    return false;
}

void HTMLResourceHandler::GetResponseHeaders(CefRefPtr<CefResponse> response, int64& responseLength, CefString& redirectUrl) {
    response->SetMimeType("text/html");
    responseLength = fileStream->Seek(0, SEEK_END);
    fileStream->Seek(0, SEEK_SET);
}

bool HTMLResourceHandler::Read(void* dataOut, int bytesToRead, int& bytesRead, CefRefPtr<CefResourceReadCallback> callback) {
    bytesRead = static_cast<int>(fileStream->Read(dataOut, bytesToRead, offset));
    offset += bytesRead;

    if (offset >= fileStream->Seek(0, SEEK_END)) {
        bytesRead = 0;
        offset = 0;
        fileStream->Seek(0, SEEK_SET);
    }

    return true;
}

void HTMLResourceHandler::Cancel() {
    fileStream = nullptr;
}

CefRefPtr<CefResourceHandler> HTMLSchemeHandlerFactory::Create(CefRefPtr<CefBrowser> browser, CefRefPtr<CefFrame> frame, const CefString& schemeName, CefRefPtr<CefRequest> request) {
    return new HTMLResourceHandler();
}

} // namespace synthesis
