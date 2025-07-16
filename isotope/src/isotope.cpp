#include <Core/Application/Application.h>
#include <Core/UserInterface/UserInterface.h>
#include <Core/Memory.h>

#include "context.h"

GlobalContext gctx;

extern "C" XI_EXPORT bool run(const char* context) {
    if (!gctx.configure()) {
        return false;
    }

    gctx.ui->messageBox("Hello from Isotope! lets go");
    return true;
}

extern "C" XI_EXPORT bool stop() {
    gctx.ui->messageBox("Stopping Isotope... Who yae");
    return true;
}
