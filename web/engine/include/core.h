#pragma once

#include "physics/physics_manager.h"

namespace SYN {

    class Core {
    public:
        Core();
        ~Core();

        static Core *instance;

        PhysicsManager physics_manager;
    };

}
