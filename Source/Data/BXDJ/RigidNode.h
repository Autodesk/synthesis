#pragma once

#include <vector>
#include <Fusion/FusionAll.h>
#include "../BXDA/Mesh.h"

using namespace adsk;

namespace BXDJ
{
	class Joint;

	// RIGID NODE
	class RigidNode
	{
	public:
		RigidNode();
		~RigidNode();
		RigidNode(core::Ptr<fusion::Occurrence>);
		
		bool addOccurence(core::Ptr<fusion::Occurrence>);
		template<typename JointVariant>
		void addJoint(const JointVariant &);

	private:
		std::vector<core::Ptr<fusion::Occurrence>> fusionOccurences;
		std::vector<Joint *> childrenJoints;

		bool getMesh(BXDA::Mesh &);

	};

	template<typename JointVariant>
	void RigidNode::addJoint(const JointVariant & joint)
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
