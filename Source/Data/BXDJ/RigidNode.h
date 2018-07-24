#pragma once

#include <vector>
#include <map>
#include <algorithm>
#include <Fusion/FusionAll.h>
#include <Core/Geometry/Point3D.h>
#include "../BXDA/Mesh.h"
#include "../BXDA/Physics.h"

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
		RigidNode(core::Ptr<fusion::Component>);
		RigidNode(core::Ptr<fusion::Occurrence>);

		void getMesh(BXDA::Mesh &) const;
		
		template<typename JointVariant>
		void addJoint(const JointVariant &);

	private:
		std::vector<core::Ptr<fusion::Occurrence>> fusionOccurrences;
		std::vector<Joint *> childrenJoints;

		void buildTree(core::Ptr<fusion::Component>);

		static int levelOfOccurrence(core::Ptr<fusion::Occurrence>);

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
};
