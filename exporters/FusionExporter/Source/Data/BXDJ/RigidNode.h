#pragma once

#include <vector>
#include <map>
#include <functional>
#include <Fusion/Components/Component.h>
#include <Fusion/Components/Occurrence.h>
#include "XmlWriter.h"
#include "../Guid.h"

using namespace adsk;

namespace BXDA
{
	class Mesh;
}

namespace BXDJ
{
	class ConfigData;
	class Joint;
	class Sensor;

	// Stores the collection of component occurences that act as a single rigidbody in Synthesis
	class RigidNode : public XmlWritable
	{
	public:
		RigidNode();
		RigidNode(const RigidNode &);
		RigidNode(core::Ptr<fusion::Component>, ConfigData config);
		RigidNode(core::Ptr<fusion::Occurrence>, Joint * parent);

		Guid getGUID() const;
		std::string getModelId() const;
		Joint * getParent() const;
		void getChildren(std::vector<std::shared_ptr<RigidNode>> &, bool recursive = false) const;
		int getOccurrenceCount() const;
		void getMesh(BXDA::Mesh &, bool ignorePhysics = false, std::function<void(double)> progressCallback = nullptr, bool * cancel = nullptr) const;

		void addJoint(std::shared_ptr<Joint>);
		void addSensor(const Sensor &);
		
		std::string log = "";

	private:
		// Used for storing information about which occurences are parents or children in joints
		struct JointSummary
		{
			std::vector<core::Ptr<fusion::Occurrence>> children;
			std::map<core::Ptr<fusion::Occurrence>, std::vector<core::Ptr<fusion::Joint>>> parents;
		};

		// Globally Unique Identifier
		Guid guid;
		// Stores information about which occurences are parents or children in joints
		std::shared_ptr<ConfigData> configData;
		// Stores information about which occurences are parents or children in joints
		std::shared_ptr<JointSummary> jointSummary;
		// Stores the joints that lead to this node's children
		std::vector<std::shared_ptr<Joint>> childrenJoints;
		// Stores a reference to the node's parent
		Joint * parent;
		// All sensors connected to this node, relative to the parent joint
		std::vector<std::shared_ptr<Sensor>> sensors;
		// Stores all component occurences that are grouped into this node
		std::vector<core::Ptr<fusion::Occurrence>> fusionOccurrences;

		JointSummary getJointSummary(core::Ptr<fusion::Component>);
		void buildTree(core::Ptr<fusion::Occurrence>);
		void addJoint(core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);
		void write(XmlWriter &) const;

	};
};
