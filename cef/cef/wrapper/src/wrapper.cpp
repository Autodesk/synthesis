#include "wrapper.h"

#include <memory>

#include "core.h"
#include "debug.h"
#include "offscreen_cef_client.h"

struct OffscreenCefClientInterop {
    synthesis::OffscreenCefClient* instance;
};

SYNTHESIS_EXPORT OffscreenCefClientInterop* CreateOffscreenCefClientInterop(int width, int height) {
    OffscreenCefClientInterop* interop = new OffscreenCefClientInterop();
    interop->instance = new synthesis::OffscreenCefClient(width, height);
    return interop;
}

SYNTHESIS_EXPORT void DestroyOffscreenCefClientInterop(OffscreenCefClientInterop* interop) {
    delete interop->instance;
    delete interop;
}
