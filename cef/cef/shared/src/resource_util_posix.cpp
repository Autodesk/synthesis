#include "resource_util.h"

#include <cstdio>

namespace synthesis {
namespace shared {
namespace {

bool FileExists(const char* path) {
    FILE* file = fopen(path, "rb");

    if (file) {
        fclose(file);
        return true;
    }

    return false;
}

bool ReadfileToString(const char* path, std::string& data) {
    FILE* file = fopen(path, "rb");

    if (!file) {
        return false;
    }

    char buffer[1 << 16];
    size_t len;

    while ((len = fread(buffer, 1, sizeof(buffer), file)) > 0) {
        data.append(buffer, len);
    }

    fclose(file);
    return true;
}

} // namespace

bool GetResourceString(const std::string& resourcePath, std::string& outData) {
    std::string path;

    if (!GetResourceDir(path)) {
        return false;
    }

    path.append("/");
    path.append(resourcePath);
    return ReadfileToString(path.c_str(), outData);
}

CefRefPtr<CefStreamReader> GetResourceReader(const std::string& resourcePath) {
    std::string path;

    if (!GetResourceDir(path)) {
        return nullptr;
    }

    path.append("/");
    path.append(resourcePath);

    if (!FileExists(path.c_str())) {
        return nullptr;
    }

    return CefStreamReader::CreateForFile(path);
}

} // namespace shared
} // namespace synthesis
