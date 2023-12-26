#include "physics/physics_manager.h"

#include "core.h"

#include <thread>
#include <iostream>
#include <cstdio>

SYN::PhysicsManager::PhysicsManager()
		: broad_phase_layer_interface(BPLayerInterfaceImpl()),
			object_vs_broadphase_layer_filter(ObjectVsBroadPhaseLayerFilterImpl()),
			object_vs_object_layer_filter(ObjectLayerPairFilterImpl()),
			bodies(std::vector<Body *>())
{
    JPH::RegisterDefaultAllocator();

    JPH::Factory::sInstance = new JPH::Factory();

    JPH::RegisterTypes();

    // We need a temp allocator for temporary allocations during the physics update. We're
	// pre-allocating 10 MB to avoid having to do allocations during the physics update.
	// B.t.w. 10 MB is way too much for this example but it is a typical value you can use.
	// If you don't want to pre-allocate you can also use TempAllocatorMalloc to fall back to
	// malloc / free.
	this->temp_allocator = new JPH::TempAllocatorImpl(10 * 1024 * 1024);

    std::cout << "std::thread::hardware_concurrency() -> " << std::thread::hardware_concurrency() << std::endl;

	// We need a job system that will execute physics jobs on multiple threads. Typically
	// you would implement the JobSystem interface yourself and let Jolt Physics run on top
	// of your own job scheduler. JobSystemThreadPool is an example implementation.
	this->job_system = new JPH::JobSystemThreadPool(JPH::cMaxPhysicsJobs, JPH::cMaxPhysicsBarriers, std::thread::hardware_concurrency() - 1);

	// This is the max amount of rigid bodies that you can add to the physics system. If you try to add more you'll get an error.
	// Note: This value is low because this is a simple test. For a real project use something in the order of 65536.
	const uint cMaxBodies = 1024;

	// This determines how many mutexes to allocate to protect rigid bodies from concurrent access. Set it to 0 for the default settings.
	const uint cNumBodyMutexes = 0;

	// This is the max amount of body pairs that can be queued at any time (the broad phase will detect overlapping
	// body pairs based on their bounding boxes and will insert them into a queue for the narrowphase). If you make this buffer
	// too small the queue will fill up and the broad phase jobs will start to do narrow phase work. This is slightly less efficient.
	// Note: This value is low because this is a simple test. For a real project use something in the order of 65536.
	const uint cMaxBodyPairs = 1024;

	// This is the maximum size of the contact constraint buffer. If more contacts (collisions between bodies) are detected than this
	// number then these contacts will be ignored and bodies will start interpenetrating / fall through the world.
	// Note: This value is low because this is a simple test. For a real project use something in the order of 10240.
	const uint cMaxContactConstraints = 1024;

	// Create mapping table from object layer to broadphase layer
	// Note: As this is an interface, PhysicsSystem will take a reference to this so this instance needs to stay alive!
	// BPLayerInterfaceImpl broad_phase_layer_interface;

	// Create class that filters object vs broadphase layers
	// Note: As this is an interface, PhysicsSystem will take a reference to this so this instance needs to stay alive!
	// ObjectVsBroadPhaseLayerFilterImpl object_vs_broadphase_layer_filter;

	// Create class that filters object vs object layers
	// Note: As this is an interface, PhysicsSystem will take a reference to this so this instance needs to stay alive!
	// ObjectLayerPairFilterImpl object_vs_object_layer_filter;

	// Now we can create the actual physics system.
	// JPH::PhysicsSystem physics_system;
	this->physics_system = new JPH::PhysicsSystem();
	this->physics_system->Init(
		cMaxBodies,
		cNumBodyMutexes,
		cMaxBodyPairs,
		cMaxContactConstraints,
		broad_phase_layer_interface,
		object_vs_broadphase_layer_filter,
		object_vs_object_layer_filter
	);

	// Create Ground Body

	auto groundCreationSettings = JPH::BodyCreationSettings(
		new JPH::BoxShape(Vec3(100.0, 1.0, 100.0)),
		JPH::RVec3(0.0_r, -3.0_r, 0.0_r),
		JPH::Quat::sIdentity(),
		JPH::EMotionType::Static,
		Layers::NON_MOVING
	);

	JPH::Body *body = this->physics_system->GetBodyInterface().CreateBody(groundCreationSettings);
	this->physics_system->GetBodyInterface().AddBody(body->GetID(), JPH::EActivation::DontActivate);

	this->bodies.push_back(body);
}

SYN::PhysicsManager::~PhysicsManager() {
	
	for (auto body = this->bodies.begin(); body != this->bodies.end(); ++body) {
		auto id = (*body)->GetID();
		this->physics_system->GetBodyInterface().RemoveBody(id);
		this->physics_system->GetBodyInterface().DestroyBody(id);
	}

	this->bodies.clear();

    JPH::UnregisterTypes();

    delete JPH::Factory::sInstance;
    JPH::Factory::sInstance = nullptr;

	delete this->physics_system;
	delete this->job_system;
	delete this->temp_allocator;
}

void SYN::PhysicsManager::Step(const float deltaT, const int substeps = 1) {
	// Step the world
	this->physics_system->Update(deltaT, substeps, this->temp_allocator, this->job_system);
}

JPH::Body *SYN::PhysicsManager::CreateSphere() {
	JPH::BodyCreationSettings sphere_settings(
		new SphereShape(0.5f),
		JPH::RVec3(0.0_r, 2.0_r, 0.0_r),
		JPH::Quat::sIdentity(),
		JPH::EMotionType::Dynamic,
		Layers::MOVING
	);
	sphere_settings.mRestitution = 0.995;
	JPH::Body *body = this->physics_system->GetBodyInterface().CreateBody(sphere_settings);

	this->bodies.push_back(body);

	this->physics_system->GetBodyInterface().AddBody(body->GetID(), JPH::EActivation::Activate);

	return body;
}

JPH::Vec3 SYN::PhysicsManager::GetBodyCOMPos(const JPH::Body &body) {
	return body.GetCenterOfMassPosition();
}

extern "C" {
	void *physics_create_ball() {
		return SYN::Core::instance->physics_manager.CreateSphere();
	}

	void physics_get_position(const void *ball, float *res) {
		auto pos = SYN::Core::instance->physics_manager.GetBodyCOMPos(*((JPH::Body *)ball));

		// printf("(%.2f, %.2f, %.2f)\n", pos.GetX(), pos.GetY(), pos.GetZ());

		res[0] = pos.GetX();
		res[1] = pos.GetY();
		res[2] = pos.GetZ();
	}

	void physics_step(float deltaT, int substeps) {

		printf("DeltaT: %.1fms, Substeps: %d\n", deltaT * 1000.0, substeps);

		SYN::Core::instance->physics_manager.Step(deltaT, substeps);
    }
}