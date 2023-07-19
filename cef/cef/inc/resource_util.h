#ifndef SYNTHESIS_CEF_RESOURCE_UTIL_H_
#define SYNTHESIS_CEF_RESOURCE_UTIL_H_
#pragma once

#include <include/cef_resource_handler.h>
#include <include/cef_stream.h>

#if defined(OS_WIN)
#include <include/wrapper/cef_resource_manager.h>
#endif // defined(OS_WIN)

namespace synthesis {
namespace shared {

// ORigin for loading local test resources
extern const char testOrigin[];

#if defined(OS_POSIX)
bool GetResourceDir(std::string& dir);
#endif // defined(OS_POSIX)

std::string GetResourcePath(const std::string& url);
std::string GetMimeType(const std::string& resourcePath);

#if defined(OS_WIN)
int GetResourceId(const std::string& resourcePath);
CefResourceManager::Provider* CreateResourceManagerProvider(const std::string& rootUrl);
#endif // defined(OS_WIN)

bool GetResourceString(const std::string& resourcePath, std::string& outData);
CefRefPtr<CefStreamReader> GetResourceReader(const std::string& resourcePath);

CefRefPtr<CefResourceHandler> GetResourceHandler(const std::string& resourcePath);

} // namespace shared
} // namespace synthesis

#endif // SYNTHESIS_CEF_RESOURCE_UTIL_H_
