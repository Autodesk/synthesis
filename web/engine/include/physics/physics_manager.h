#pragma once

#include <vector>

#include "physics/physics_impl.h"

namespace SYN {

    class PhysicsManager {
    public:
        PhysicsManager();
        ~PhysicsManager();

        void Step(const float deltaT, const int substeps);

        JPH::Body *CreateSphere();

        JPH::Vec3 GetBodyCOMPos(const JPH::Body &body);
    private:
        JPH::TempAllocatorImpl *temp_allocator;
        JPH::JobSystemThreadPool *job_system;
        BPLayerInterfaceImpl broad_phase_layer_interface;
        ObjectVsBroadPhaseLayerFilterImpl object_vs_broadphase_layer_filter;
        ObjectLayerPairFilterImpl object_vs_object_layer_filter;

        JPH::PhysicsSystem *physics_system;

        std::vector<JPH::Body *> bodies;
    };

}