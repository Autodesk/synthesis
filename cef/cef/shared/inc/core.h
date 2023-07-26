#pragma once
#ifndef SYNTHESIS_CEF_CORE_H_
#define SYNTHESIS_CEF_CORE_H_

#ifdef __cplusplus
#define SYNTHESIS_BEGIN_EXTERN_C extern "C" {
#define SYNTHESIS_END_EXTERN_C }
#else
#define SYNTHESIS_BEGIN_EXTERN_C
#define SYNTHESIS_END_EXTERN_C
#endif

#if __WIN32
#define SYNTHESIS_EXPORT __declspec(dllexport)
#else
#define SYNTHESIS_EXPORT __attribute__((visibility("default")))
#endif

constexpr const char* SYNTHESIS_HTML_SCHEME = "synthesis://";

#endif // SYNTHESIS_CEF_CORE_H_
