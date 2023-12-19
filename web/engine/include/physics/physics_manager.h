#pragma once

namespace SYN {

    class PhysicsManager {
    public:
        PhysicsManager();
        ~PhysicsManager();

        void Step(const float deltaT, const int substeps = 1);
    private:
    };

}