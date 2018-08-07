#include "RigidNode.h"
#include <Fusion/Components/Occurrences.h>
#include <Fusion/Components/OccurrenceList.h>
#include <Fusion/Components/RigidGroup.h>
#include "Utility.h"
#include "ConfigData.h"
#include "Joint.h"
#include "Joints/RotationalJoint.h"
#include "Joints/SliderJoint.h"
#include "Joints/CylindricalJoint.h"
#include "Joints/BallJoint.h"

using namespace BXDJ;

RigidNode::RigidNode(core::Ptr<fusion::Component> rootComponent, ConfigData config) : RigidNode()
{
	configData = std::make_shared<ConfigData>(config);
	jointSummary = std::make_shared<JointSummary>(getJointSummary(rootComponent));

	for (core::Ptr<fusion::Occurrence> occurrence : rootComponent->occurrences()->asList())
		if (std::find(jointSummary->children.begin(), jointSummary->children.end(), occurrence) == jointSummary->children.end())
			buildTree(occurrence);
}

RigidNode::RigidNode(core::Ptr<fusion::Occurrence> occ, Joint * parent)
{
	if (parent == NULL)
		throw "Parent node cannot be NULL!";

	this->parent = parent;
	configData = parent->getParent()->configData;
	jointSummary = parent->getParent()->jointSummary;
	buildTree(occ);
}

Joint * RigidNode::getParent() const
{
	return parent;
}

void BXDJ::RigidNode::getChildren(std::vector<std::shared_ptr<RigidNode>> & children, bool recursive) const
{
	for (std::shared_ptr<Joint> joint : childrenJoints)
	{
		children.push_back(joint->getChild());

		if (recursive)
			joint->getChild()->getChildren(children);
	}
}

int BXDJ::RigidNode::getOccurrenceCount() const
{
	return fusionOccurrences.size();
}

void RigidNode::buildTree(core::Ptr<fusion::Occurrence> rootOccurrence)
{
	// Add the occurence to this node
	log += "Adding occurence \"" + rootOccurrence->fullPathName() + "\"\n";
	fusionOccurrences.push_back(rootOccurrence);

	// Create a joint from this occurrence if it is the parent of any joints
	if (jointSummary->parents.find(rootOccurrence) != jointSummary->parents.end())
		for (core::Ptr<fusion::Joint> joint : jointSummary->parents[rootOccurrence])
			addJoint(joint, rootOccurrence);

	// Merge this occurrence with any occurrences rigidgrouped to it
	if (jointSummary->rigidgroups.find(rootOccurrence) != jointSummary->rigidgroups.end())
		for (core::Ptr<fusion::Occurrence> occurrence : jointSummary->rigidgroups[rootOccurrence])
			buildTree(occurrence);

	// Add all occurrences without joints or that are only parents in joints to the root node
	for (core::Ptr<fusion::Occurrence> occurrence : rootOccurrence->childOccurrences())
		// Add the occurence to this node if it is not the child of a joint
		if (jointSummary->children.find(occurrence) == jointSummary->children.end())
			buildTree(occurrence);

	log += "\n";
}

RigidNode::JointSummary RigidNode::getJointSummary(core::Ptr<fusion::Component> rootComponent)
{
	JointSummary jointSummary;

	// Find all jointed occurrences in the design
	for (core::Ptr<fusion::Joint> joint : rootComponent->allJoints())
	{
		if (joint->occurrenceOne() != nullptr && joint->occurrenceTwo() != nullptr)
		{
			core::Ptr<fusion::Occurrence> lowerOccurrence = Utility::lowerOccurrence(joint);
			core::Ptr<fusion::Occurrence> upperOccurrence = Utility::upperOccurrence(joint);

			jointSummary.children[lowerOccurrence] = upperOccurrence;
			jointSummary.parents[upperOccurrence].push_back(joint);
		}
		else if (joint->occurrenceOne() != nullptr || joint->occurrenceTwo() != nullptr)
		{
			core::Ptr<fusion::Occurrence> lowerOccurrence = (joint->occurrenceOne() != nullptr) ? joint->occurrenceOne() : joint->occurrenceTwo();
			core::Ptr<fusion::OccurrenceList> upperOccurrences = rootComponent->allOccurrencesByComponent(joint->parentComponent());

			jointSummary.children[lowerOccurrence] = nullptr;
			for (core::Ptr<fusion::Occurrence> upperOccurrence : upperOccurrences)
				jointSummary.parents[upperOccurrence].push_back(joint);
		}
	}

	// Find all rigid groups in the design
	for (core::Ptr<fusion::RigidGroup> rgdGroup : rootComponent->allRigidGroups())
	{
		core::Ptr<fusion::Occurrence> topOccurrence = nullptr;

		// An occurrence in the rigid group will be marked as the top occurrence if:
		//  - It is the highest in the heirarchy, or
		//  - It is the child of a joint
		// If multiple occurrences are the children of joints, then Houston we have a problem
		for (core::Ptr<fusion::Occurrence> occurrence : rgdGroup->occurrences())
			if (topOccurrence == nullptr || Utility::levelOfOccurrence(topOccurrence) > Utility::levelOfOccurrence(occurrence) ||
				std::find(jointSummary.children.begin(), jointSummary.children.end(), occurrence) != jointSummary.children.end())
				topOccurrence = occurrence;

		// All occurrences that are not the top are now children, while the top stores references to all of them
		for (core::Ptr<fusion::Occurrence> occurrence : rgdGroup->occurrences())
		{
			if (occurrence != topOccurrence)
			{
				jointSummary.children[occurrence] = topOccurrence;
				jointSummary.rigidgroups[topOccurrence].push_back(occurrence);
			}
		}
	}

	return jointSummary;
}

void RigidNode::addJoint(core::Ptr<fusion::Joint> joint, core::Ptr<fusion::Occurrence> parent)
{
	core::Ptr<fusion::Occurrence> child = (joint->occurrenceOne() != parent) ? joint->occurrenceOne() : joint->occurrenceTwo();
	
	// Do not add joint if child has changed parents
	if (jointSummary->children[child] != parent)
		return;
	
	log += "Jointing occurence \"" + child->fullPathName() + "\"\n";

	std::shared_ptr<Joint> newJoint = nullptr;

	fusion::JointTypes jointType = joint->jointMotion()->jointType();

	// Create joint based on the type of joint in Fusion
	if (jointType == fusion::JointTypes::RevoluteJointType)
		newJoint = std::make_shared<RotationalJoint>(this, joint, parent);
	else if (jointType == fusion::JointTypes::SliderJointType)
		newJoint = std::make_shared<SliderJoint>(this, joint, parent);
	else if (jointType == fusion::JointTypes::CylindricalJointType)
		newJoint = std::make_shared<CylindricalJoint>(this, joint, parent);
	else if (jointType == fusion::JointTypes::BallJointType)
		newJoint = std::make_shared<BallJoint>(this, joint, parent);
	else 
	{
		// If joint type is unsupported, add as if occurence is attached by rigid joint (same rigid node)
		buildTree(child);
		return;
	}

	// Apply user configuration to joint
	newJoint->applyConfig(*configData);
	addJoint(newJoint);
}

void RigidNode::addJoint(std::shared_ptr<Joint> joint)
{
	childrenJoints.push_back(joint);
}
