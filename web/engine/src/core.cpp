#include "core.h"

namespace SYN {

    Core *Core::instance = nullptr;

    Core::Core(): physics_manager(PhysicsManager()) { }

    Core::~Core() { }
}

extern "C" {

    void core_init() {
        if (SYN::Core::instance != nullptr) {
            delete SYN::Core::instance;
        }
        SYN::Core::instance = new SYN::Core();
    }

    void core_destroy() {
        if (SYN::Core::instance != nullptr) {
            delete SYN::Core::instance;
            SYN::Core::instance = nullptr;
        }
    }

}