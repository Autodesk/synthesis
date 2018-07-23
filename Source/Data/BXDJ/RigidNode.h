#pragma once

#include <vector>
#include "../BXDA/Mesh.h"

namespace BXDJ
{
	class Joint;

	// RIGID NODE
	class RigidNode
	{
	public:
		RigidNode();
		~RigidNode();
		RigidNode(const BXDA::Mesh &);
		
		template<typename JointVariant>
		void AddJoint(const JointVariant &);
		void AddMesh(const BXDA::Mesh &);

	private:
		std::vector<Joint *> childrenJoints;

		BXDA::Mesh * mesh;

	};

	template<typename JointVariant>
	void RigidNode::AddJoint(const JointVariant & joint)
	{
		childrenJoints.push_back(new JointVariant(joint));
	}

	// JOINT
	class Joint
	{
	public:
		Joint(const RigidNode &);
		~Joint();

	private:
		RigidNode * child;

	};
}
