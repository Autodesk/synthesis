#pragma once
#ifndef SYNTHESIS_CEF_WRAPPER_H_
#define SYNTHESIS_CEF_WRAPPER_H_

#include "core.h"

SYNTHESIS_BEGIN_EXTERN_C

typedef struct OffscreenCefClientInterop OffscreenCefClientInterop;

SYNTHESIS_EXPORT OffscreenCefClientInterop* CreateOffscreenCefClientInterop(int width, int height);

SYNTHESIS_EXPORT void DestroyOffscreenCefClientInterop(OffscreenCefClientInterop* interop);

SYNTHESIS_END_EXTERN_C

#endif // SYNTHESIS_CEF_WRAPPER_H_
