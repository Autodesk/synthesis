#include "resource_util.h"

#include <include/cef_parser.h>
#include <include/wrapper/cef_stream_resource_handler.h>

namespace synthesis {
namespace shared {

const char testOrigin[] = "http://example.com/";

namespace {

std::string GetUrlWithoutQueryOrFragment(const std::string& url) {
    const size_t pos = std::min(url.find('?'), url.find('#'));

    if (pos != std::string::npos) {
        return url.substr(0, pos);
    } else {
        return url;
    }
}

} // namespace

std::string GetResourcePath(const std::string& url) {
    if (url.find(testOrigin) != 0U) {
        return std::string();
    }

    const std::string& urlNoQuery = GetUrlWithoutQueryOrFragment(url);
    return urlNoQuery.substr(sizeof(testOrigin) - 1);
}

std::string GetMimeType(const std::string& resourcePath) {
    std::string mimeType;
    const size_t sep = resourcePath.find_last_of(".");

    if (sep != std::string::npos) {
        mimeType = CefGetMimeType(resourcePath.substr(sep + 1));

        if (!mimeType.empty()) {
            return mimeType;
        }
    }

    return "text/html";
}

CefRefPtr<CefResourceHandler> GetResourceHandler(const std::string& resourcePath) {
    CefRefPtr<CefStreamReader> reader = GetResourceReader(resourcePath);

    if (!reader.get()) {
        return nullptr;
    }

    return new CefStreamResourceHandler(GetMimeType(resourcePath), reader);
}

} // namespace shared
} // namespace synthesis
