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

bool GlobalContext::isValid() const {
    return this->app && this->ui && this->app->isValid() && this->ui->isValid();
}
