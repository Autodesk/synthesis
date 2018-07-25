#pragma once

#include <vector>
#include <map>
#include <algorithm>
#include <Fusion/FusionAll.h>
#include <Core/Geometry/Point3D.h>
#include "XmlWriter.h"
#include "../Guid.h"
#include "../BXDA/BinaryWriter.h"
#include "../BXDA/Mesh.h"
#include "../BXDA/Physics.h"

using namespace adsk;

namespace BXDJ
{
	class Joint;

	// Stores the collection of component occurences that act as a single rigidbody in Synthesis
	class RigidNode : public XmlWritable
	{
	public:
		RigidNode();
		RigidNode(const RigidNode &);
		RigidNode(core::Ptr<fusion::Component>);

		std::string getModelId() const;
		Joint * getParent() const;
		void getMesh(BXDA::Mesh &) const;

		void connectToJoint(Joint *);
		void addJoint(std::shared_ptr<Joint> joint);
		
		std::string log = "";

	private:
		// Used for storing information about which occurences are parents or children in joints
		struct JointSummary
		{
			std::vector<core::Ptr<fusion::Occurrence>> children;
			std::map<core::Ptr<fusion::Occurrence>, std::vector<core::Ptr<fusion::Joint>>> parents;
		};

		// Contructor used for building node tree
		RigidNode(core::Ptr<fusion::Occurrence> occ, JointSummary & jSum) : RigidNode() { buildTree(occ, jSum); }

		// Globally Unique Identifier
		Guid guid;
		// Stores the joints that lead to this node's children
		std::vector<std::shared_ptr<Joint>> childrenJoints;
		// Stores a reference to the node's parent
		Joint * parent;
		// Stores all component occurences that are grouped into this node
		std::vector<core::Ptr<fusion::Occurrence>> fusionOccurrences;

		JointSummary getJointSummary(core::Ptr<fusion::Component>);
		void buildTree(core::Ptr<fusion::Occurrence>, JointSummary &);
		void addJoint(core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>, JointSummary &);
		void write(XmlWriter &) const;

		// Gets the level of a component occurence in the heirarchy
		static int levelOfOccurrence(core::Ptr<fusion::Occurrence>);

	};
};
