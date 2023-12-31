#pragma once

#include <memory>
#include <vector>
#include <assembly.pb.h>

#include "physics/physics_manager.h"
#include "mira_assembly.h"

namespace SYN {

    class Core {
    public:
        static Core *instance;
    public:
        Core();
        ~Core();

        std::weak_ptr<MiraAssembly> LoadAssembly(const void *binary, size_t size);

        PhysicsManager physics_manager;

        std::vector<std::shared_ptr<MiraAssembly>> assemblies;
    };

}
