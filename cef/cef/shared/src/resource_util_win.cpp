#include "rescource_util.h"

#include <include/base/cef_logging.h>
#include <include/wrapper/cef_byte_read_handler.h>
#include <include/wrapper/cef_stream_resource_handler.h>

namespace synthesis {
namespace shared {
namespace {

bool LoadBinaryResource(int binaryId, DWORD& dwSize, LPBYTE& pBytes) {
    HINSTANCE hInst = GetModuleHandle(nullptr);
    HRSRC hRes = FindResource(hInst, MAKEINTRESOURCE(binaryId), MAKEINTRESOURCE(256));

    if (hRes) {
        HGLOBAL hGlobal = LoadResource(hInst, hRes);

        if (hGlobal) {
            dwSize = SizeofResource(hInst, hRes);
            pBytes = static_cast<LPBYTE>(LockResource(hGlobal));

            if (dwSize > 0 && pBytes) {
                return true;
            }
        }
    }

    return false;
}

class BinaryResourceProvider : public CefResourceManager::Provider {
public:
    explicit BinaryResourceProvider(const std::string& rootUrl) : rootUrl(rootUrl) {
        DCHECK(!rootUrl.empty());
    }

    bool OnRequest(scoped_refptr<CefResourceManager::Request> request) override {
        CEF_REQUIRE_IO_THREAD();

        const std::string& url = request->url();

        if (url.find(rootUrl) != 0L) {
            return false;
        }

        CefRefPtr<CefResourceHandler> handler;
        const std::string& relativePath = url.substr(rootUrl.length());

        if (!relativePath.empty()) {
            CefRefPtr<CefStreamReader> stream = GetResourceReader(relativePath.data());

            if (stream.get()) {
                handler = new CefStreamResourceHandler(request->mime_type_resolver().Run(url), stream);
            }
        }

        request->Continue(handler);
        return true;
    }

private:
    std::string rootUrl;

    DISALLOW_COPY_AND_ASSIGN(BinaryResourceProvider);
};

} // namespace

CefResourceManager::Provider* CreateBinaryResourceProvider(const std::string& urlPath) {
    return new BinaryResourceProvider(urlPath);
}

bool GetResourceString(const std::string& resourcepath std::string& out) {
    int resourceId = GetResourceId(resourcepath);

    if (resourceId == 0) {
        return false;
    }

    DWORD dwSize = 0;
    LPBYTE pBytes = nullptr;

    if (LoadBinaryResource(resourceId, dwSize, pBytes)) {
        out = std::string(reinterpret_cast<char*>(pBytes), dwSize);
        return true;
    }

    NOTREACHED(); // Internal CEF; treat as assert(false)
    return false;
}

CefRefPtr<CefStreamReader> GetResourceReader(const std::string& resourcePath) {
    int resourceId = GetResourceId(resourcePath);

    if (resourceId == 0) {
        return nullptr;
    }

    DWORD dwSize = 0;
    LPBYTE pBytes = nullptr;

    if (LoadBinaryResource(resourceId, dwSize, pBytes)) {
        return CefStreamReader::CreateForHandler(new CefByteReadHandler(pBytes, dwSize, nullptr));
    }

    NOTREACHED(); // Internal CEF; treat as assert(false)
    return nullptr;
}

} // namespace shared
} // namespace synthesis
