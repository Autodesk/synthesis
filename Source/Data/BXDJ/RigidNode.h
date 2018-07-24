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

	// Stores the collection of component occurences that act as a single rigidbody in Synthesis
	class RigidNode
	{
	public:
		RigidNode();
		~RigidNode();
		RigidNode(const RigidNode &);
		RigidNode(core::Ptr<fusion::Component>);

		void getMesh(BXDA::Mesh &) const;

		void RigidNode::addJoint(std::shared_ptr<Joint> joint);
		
		std::string log = "";

	private:
		// Used for storing information about which occurences are parents or children in joints
		struct JointSummary
		{
			std::vector<core::Ptr<fusion::Occurrence>> children;
			std::map<core::Ptr<fusion::Occurrence>, std::vector<core::Ptr<fusion::Joint>>> parents;
		};

		// Stores all component occurences that are grouped into this node
		std::vector<core::Ptr<fusion::Occurrence>> fusionOccurrences;
		// Stores the joints that lead to this node's children
		std::vector<std::shared_ptr<Joint>> childrenJoints; 

		RigidNode(core::Ptr<fusion::Occurrence> occ, JointSummary & jSum) { buildTree(occ, jSum); }

		JointSummary getJointSummary(core::Ptr<fusion::Component>);
		void buildTree(core::Ptr<fusion::Occurrence>, JointSummary &);
		void addJoint(core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>, JointSummary &);

		// Gets the level of a component occurence in the heirarchy
		static int levelOfOccurrence(core::Ptr<fusion::Occurrence>);

	};
};
