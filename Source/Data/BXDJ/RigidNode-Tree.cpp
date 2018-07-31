#include "RigidNode.h"
#include "Joint.h"
#include "Joints/RotationalJoint.h"
#include "Joints/SliderJoint.h"
#include "Joints/CylindricalJoint.h"
#include "Joints/BallJoint.h"

using namespace BXDJ;

Joint * RigidNode::getParent() const
{
	return parent;
}

void RigidNode::buildTree(core::Ptr<fusion::Occurrence> rootOccurrence)
{
	// Add the occurence to this node
	log += "Adding occurence \"" + rootOccurrence->fullPathName() + "\"\n";
	fusionOccurrences.push_back(rootOccurrence);

	// Create a joint from this occurence if it is the parent of a joint
	if (jointSummary->parents.find(rootOccurrence) != jointSummary->parents.end())
		for (core::Ptr<fusion::Joint> joint : jointSummary->parents[rootOccurrence])
			addJoint(joint, rootOccurrence);

	// Add all occurrences without joints or that are only parents in joints to the root node
	for (core::Ptr<fusion::Occurrence> occurrence : rootOccurrence->childOccurrences())
		// Add the occurence to this node if it is not the child of a joint
		if (std::find(jointSummary->children.begin(), jointSummary->children.end(), occurrence) == jointSummary->children.end())
			buildTree(occurrence);

	log += "\n";
}

RigidNode::JointSummary RigidNode::getJointSummary(core::Ptr<fusion::Component> rootComponent)
{
	JointSummary jointSummary;

	// Find all jointed occurrences in the structure
	for (core::Ptr<fusion::Joint> joint : rootComponent->allJoints())
	{
		if (joint->occurrenceOne() != nullptr && joint->occurrenceTwo() != nullptr)
		{
			core::Ptr<fusion::Occurrence> lowerOccurrence;
			core::Ptr<fusion::Occurrence> upperOccurrence;

			// Find which occurence is higher in the heirarchy
			if (levelOfOccurrence(joint->occurrenceOne()) >= levelOfOccurrence(joint->occurrenceTwo()))
			{
				lowerOccurrence = joint->occurrenceOne();
				upperOccurrence = joint->occurrenceTwo();
			}
			else
			{
				upperOccurrence = joint->occurrenceOne();
				lowerOccurrence = joint->occurrenceTwo();
			}

			jointSummary.children.push_back(lowerOccurrence);
			jointSummary.parents[upperOccurrence].push_back(joint);
		}
		else if (joint->occurrenceOne() != nullptr || joint->occurrenceTwo() != nullptr)
		{
			core::Ptr<fusion::Occurrence> lowerOccurrence = (joint->occurrenceOne() != nullptr) ? joint->occurrenceOne() : joint->occurrenceTwo();
			core::Ptr<fusion::OccurrenceList> upperOccurrences = rootComponent->allOccurrencesByComponent(joint->parentComponent());

			jointSummary.children.push_back(lowerOccurrence);
			for (core::Ptr<fusion::Occurrence> upperOccurrence : upperOccurrences)
				jointSummary.parents[upperOccurrence].push_back(joint);
		}
	}

	return jointSummary;
}

void BXDJ::RigidNode::addJoint(core::Ptr<fusion::Joint> joint, core::Ptr<fusion::Occurrence> parent)
{
	core::Ptr<fusion::Occurrence> child = (joint->occurrenceOne() != parent) ? joint->occurrenceOne() : joint->occurrenceTwo();
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

int RigidNode::levelOfOccurrence(core::Ptr<fusion::Occurrence> occurrence)
{
	std::string pathName = occurrence->fullPathName();

	int count = 0;
	for (char c : pathName)
		if (c == '+')
			count++;

	return count;
}
