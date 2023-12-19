#include "core.h"

namespace SYN {
    Core::Core() {

    }

    Core::~Core() {

    }
}

extern "C" {

    void *core_init() {
        return SYN::Core::Init();
    }

    int core_destroy() {
        return SYN::Core::Destroy();
    }

    void core_update_physics() {
        
    }

}