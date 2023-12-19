#pragma once

/**
 * Pretty much all of this is taken from the HelloWorld cpp file that Jolt has on their GitHub.
 * https://github.com/jrouwe/JoltPhysics/blob/021081fc7be176b7ad3fe5057e118b51f4e0d684/HelloWorld/HelloWorld.cpp
*/

#include <Jolt/Jolt.h>

#include <Jolt/RegisterTypes.h>
#include <Jolt/Core/Factory.h>
#include <Jolt/Core/TempAllocator.h>
#include <Jolt/Core/JobSystemThreadPool.h>
#include <Jolt/Physics/PhysicsSettings.h>
#include <Jolt/Physics/PhysicsSystem.h>
#include <Jolt/Physics/Collision/Shape/BoxShape.h>
#include <Jolt/Physics/Collision/Shape/SphereShape.h>
#include <Jolt/Physics/Body/BodyCreationSettings.h>
#include <Jolt/Physics/Body/BodyActivationListener.h>

// Layer that objects can be in, determines which other objects it can collide with
// Typically you at least want to have 1 layer for moving bodies and 1 layer for static bodies, but you can have more
// layers if you want. E.g. you could have a layer for high detail collision (which is not used by the physics simulation
// but only if you do collision testing).
namespace Layers {
	static constexpr JPH::ObjectLayer NON_MOVING = 0;
	static constexpr JPH::ObjectLayer MOVING = 1;
	static constexpr JPH::ObjectLayer NUM_LAYERS = 2;
};

// Each broadphase layer results in a separate bounding volume tree in the broad phase. You at least want to have
// a layer for non-moving and moving objects to avoid having to update a tree full of static objects every frame.
// You can have a 1-on-1 mapping between object layers and broadphase layers (like in this case) but if you have
// many object layers you'll be creating many broad phase trees, which is not efficient. If you want to fine tune
// your broadphase layers define JPH_TRACK_BROADPHASE_STATS and look at the stats reported on the TTY.
namespace BroadPhaseLayers {
	static constexpr JPH::BroadPhaseLayer NON_MOVING(0);
	static constexpr JPH::BroadPhaseLayer MOVING(1);
	static constexpr uint NUM_LAYERS(2);
};

class ObjectLayerPairFilterImpl : public JPH::ObjectLayerPairFilter {
public:
	virtual bool ShouldCollide(JPH::ObjectLayer inObject1, JPH::ObjectLayer inObject2) const override {
        // Non moving layers can't collide with each other.
        return !(inObject1 == Layers::NON_MOVING && inObject2 == Layers::NON_MOVING);
	}
};

class BPLayerInterfaceImpl final : public JPH::BroadPhaseLayerInterface {
public:
	virtual uint GetNumBroadPhaseLayers() const override {
		return BroadPhaseLayers::NUM_LAYERS;
	}

	virtual JPH::BroadPhaseLayer GetBroadPhaseLayer(JPH::ObjectLayer inLayer) const override {
		switch (inLayer) {
        case Layers::NON_MOVING:
            return BroadPhaseLayers::NON_MOVING;
        case Layers::MOVING:
            return BroadPhaseLayers::MOVING;
        default:
            JPH_ASSERT(false);
            return BroadPhaseLayers::NON_MOVING;
        }
	}

// #if defined(JPH_EXTERNAL_PROFILE) || defined(JPH_PROFILE_ENABLED)
// 	virtual const char *			GetBroadPhaseLayerName(BroadPhaseLayer inLayer) const override
// 	{
// 		switch ((BroadPhaseLayer::Type)inLayer)
// 		{
// 		case (BroadPhaseLayer::Type)BroadPhaseLayers::NON_MOVING:	return "NON_MOVING";
// 		case (BroadPhaseLayer::Type)BroadPhaseLayers::MOVING:		return "MOVING";
// 		default:													JPH_ASSERT(false); return "INVALID";
// 		}
// 	}
// #endif // JPH_EXTERNAL_PROFILE || JPH_PROFILE_ENABLED
};

/// Class that determines if an object layer can collide with a broadphase layer
class ObjectVsBroadPhaseLayerFilterImpl : public JPH::ObjectVsBroadPhaseLayerFilter {
public:
	virtual bool ShouldCollide(JPH::ObjectLayer inLayer1, JPH::BroadPhaseLayer inLayer2) const override {
		switch (inLayer1) {
		case Layers::NON_MOVING:
			return inLayer2 == BroadPhaseLayers::MOVING;
		case Layers::MOVING:
			return true;
		default:
			JPH_ASSERT(false);
			return false;
		}
	}
};