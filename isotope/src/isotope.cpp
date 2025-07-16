#include <Core/CoreAll.h>
#include <Fusion/FusionAll.h>

using namespace adsk;

core::Ptr<core::Application> app;
core::Ptr<core::UserInterface> ui;

extern "C" XI_EXPORT bool run(const char* context) {
    app = core::Application::get();
    if (!app) {
        return false;
    }

    ui = app->userInterface();
    if (!ui) {
        return false;
    }

    ui->messageBox("Hello from Isotope!");
    return true;
}

extern "C" XI_EXPORT bool stop() {
    if (ui) {
        ui->messageBox("Goodbye from Isotope!");
    }

    app = nullptr;
    ui = nullptr;
    return true;
}
