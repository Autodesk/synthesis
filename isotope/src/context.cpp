#include "context.h"

bool GlobalContext::configure() {
    this->app = adsk::core::Application::get();
    if (!this->app) {
        return false;
    }

    this->ui = app->userInterface();
    if (!this->ui) {
        return false;
    }

    return true;
}
