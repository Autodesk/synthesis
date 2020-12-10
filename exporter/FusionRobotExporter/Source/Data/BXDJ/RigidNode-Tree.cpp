#include "RigidNode.h"
#include <Fusion/Components/Occurrences.h>
#include <Fusion/Components/OccurrenceList.h>
#include <Fusion/Components/RigidGroup.h>
#include <Fusion/Components/AsBuiltJoint.h>
#include "Utility.h"
#include "ConfigData.h"
#include "Joint.h"
#include "Joints/RotationalJoint.h"
#include "Joints/SliderJoint.h"
#include "Joints/CylindricalJoint.h"
#include "Joints/BallJoint.h"

using namespace BXDJ;

#if _DEBUG
std::string RigidNode::log = "";
int RigidNode::depth = 0;
#endif

RigidNode::RigidNode(core::Ptr<fusion::Component> rootComponent, ConfigData config)
{
#if _DEBUG
	log = "";
	depth = 0;
#endif

	parent = nullptr;
	configData = std::make_shared<ConfigData>(config);
	jointSummary = std::make_shared<JointSummary>(getJointSummary(rootComponent));

	for (core::Ptr<fusion::Occurrence> occurrence : rootComponent->occurrences()->asList())
		if (jointSummary->children.find(occurrence) == jointSummary->children.end())
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

void RigidNode::getChildren(std::vector<std::shared_ptr<RigidNode>> & children, bool recursive) const
{
	for (std::shared_ptr<Joint> joint : childrenJoints)
	{
		children.push_back(joint->getChild());

		if (recursive)
			joint->getChild()->getChildren(children, true);
	}
}

RigidNode::JointSummary RigidNode::getJointSummary(core::Ptr<fusion::Component> rootComponent)
{
	JointSummary jointSummary;

	// Find all jointed occurrences in the design
	for (core::Ptr<fusion::Joint> joint : rootComponent->allJoints())
	{
		if (joint->occurrenceOne() != nullptr && joint->occurrenceTwo() != nullptr)
		{
			core::Ptr<fusion::Occurrence> lowerOccurrence = joint->occurrenceOne();
			core::Ptr<fusion::Occurrence> upperOccurrence = joint->occurrenceTwo();

			jointSummary.children[lowerOccurrence] = upperOccurrence;
			jointSummary.parents[upperOccurrence].push_back(joint);
		}
	}

	// Find all as-built jointed occurrences in the design
	for (core::Ptr<fusion::AsBuiltJoint> joint : rootComponent->allAsBuiltJoints())
	{
		if (joint->occurrenceOne() != nullptr && joint->occurrenceTwo() != nullptr)
		{
			core::Ptr<fusion::Occurrence> lowerOccurrence = joint->occurrenceOne();
			core::Ptr<fusion::Occurrence> upperOccurrence = joint->occurrenceTwo();

			jointSummary.children[lowerOccurrence] = upperOccurrence;
			jointSummary.asBuiltParents[upperOccurrence].push_back(joint);
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
		{
			if (topOccurrence == nullptr || Utility::levelOfOccurrence(topOccurrence) > Utility::levelOfOccurrence(occurrence))
				topOccurrence = occurrence;
			else if (jointSummary.children.find(occurrence) != jointSummary.children.end())
			{
				topOccurrence = occurrence; // If occurrence is the child of a joint, it must be the parent of the rigid group to maintain jointing
				break;
			}
		}

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

#if _DEBUG
	log += "SUMMARY:\n" + jointSummary.toString() + "\nLOG:\n";
#endif

	return jointSummary;
}

void RigidNode::buildTree(core::Ptr<fusion::Occurrence> rootOccurrence)
{
#if _DEBUG
	log += std::string(depth, '\t') + "Adding occurrence \"" + rootOccurrence->fullPathName() + "\"\n";
	depth++;
#endif

	// Add the occurrence to this node
	fusionOccurrences.push_back(rootOccurrence);

	// Create a joint from this occurrence if it is the parent of any joints
	if (jointSummary->parents.find(rootOccurrence) != jointSummary->parents.end())
	{
#if _DEBUG
		log += std::string(depth - 1, '\t') + "Joints:\n";
#endif

		for (core::Ptr<fusion::Joint> joint : jointSummary->parents[rootOccurrence])
			addJoint(joint, rootOccurrence);
	}

	// Create a joint from this occurrence if it is the parent of any as-built joints
	if (jointSummary->asBuiltParents.find(rootOccurrence) != jointSummary->asBuiltParents.end())
	{
#if _DEBUG
		log += std::string(depth - 1, '\t') + "As-Built Joints:\n";
#endif

		for (core::Ptr<fusion::AsBuiltJoint> joint : jointSummary->asBuiltParents[rootOccurrence])
			addJoint(joint, rootOccurrence);
	}

	// Merge this occurrence with any occurrences rigidgrouped to it
	if (jointSummary->rigidgroups.find(rootOccurrence) != jointSummary->rigidgroups.end())
	{
#if _DEBUG
		log += std::string(depth - 1, '\t') + "Rigidgroups:\n";
#endif

		for (core::Ptr<fusion::Occurrence> occurrence : jointSummary->rigidgroups[rootOccurrence])
			buildTree(occurrence);
	}

	// Add all occurrences without joints or that are only parents in joints to the root node
#if _DEBUG
	if (rootOccurrence->childOccurrences()->count() > 0)
		log += std::string(depth - 1, '\t') + "Children:\n";
#endif

	for (core::Ptr<fusion::Occurrence> occurrence : rootOccurrence->childOccurrences())
	{
		// Add the occurrence to this node if it is not the child of a joint
		if (jointSummary->children.find(occurrence) == jointSummary->children.end())
			buildTree(occurrence);
#if _DEBUG
		else
			log += std::string(depth, '\t') + "\"" + occurrence->fullPathName() + "\" is connected separately by joint\n";
#endif
	}

#if _DEBUG
	depth--;
#endif
}

void RigidNode::addJoint(core::Ptr<fusion::Joint> joint, core::Ptr<fusion::Occurrence> parent)
{
	core::Ptr<fusion::Occurrence> child = (joint->occurrenceOne() != parent) ? joint->occurrenceOne() : joint->occurrenceTwo();

	// Do not add joint if child has changed parents
	if (jointSummary->children[child] != parent)
	{
#if _DEBUG
		log += std::string(depth, '\t') + "Cannot joint \"" + child->fullPathName() + "\", parent has changed\n";
#endif
		return;
	}

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
		// If joint type is unsupported, add as if occurrence is attached by rigid joint (same rigidNode)
		buildTree(child);
		return;
	}

	// Apply user configuration to joint
	newJoint->applyConfig(*configData);
	childrenJoints.push_back(newJoint);
}

void RigidNode::addJoint(core::Ptr<fusion::AsBuiltJoint> joint, core::Ptr<fusion::Occurrence> parent)
{
	core::Ptr<fusion::Occurrence> child = (joint->occurrenceOne() != parent) ? joint->occurrenceOne() : joint->occurrenceTwo();

	// Do not add joint if child has changed parents
	if (jointSummary->children[child] != parent)
	{
#if _DEBUG
		log += std::string(depth, '\t') + "Cannot joint \"" + child->fullPathName() + "\", parent has changed\n";
#endif
		return;
	}

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
		// If joint type is unsupported, add as if occurrence is attached by rigid joint (same rigidNode)
		buildTree(child);
		return;
	}

	// Apply user configuration to joint
	newJoint->applyConfig(*configData);
	childrenJoints.push_back(newJoint);
}

#if _DEBUG
std::string RigidNode::getLog() const
{
	return log;
}
#endif
