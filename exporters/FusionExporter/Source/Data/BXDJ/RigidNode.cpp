#include "RigidNode.h"
#include <Fusion/Components/Occurrences.h>
#include <Fusion/Components/OccurrenceList.h>
#include "Joint.h"
#include "../Filesystem.h"
#include "../BXDA/Mesh.h"

using namespace BXDJ;

RigidNode::RigidNode()
{
	parent = NULL;
}

RigidNode::RigidNode(const RigidNode & nodeToCopy) : guid(nodeToCopy.guid)
{
	configData = nodeToCopy.configData;
	jointSummary = nodeToCopy.jointSummary;

	for (core::Ptr<fusion::Occurrence> occurrence : nodeToCopy.fusionOccurrences)
		fusionOccurrences.push_back(occurrence);

	for (std::shared_ptr<Joint> joint : nodeToCopy.childrenJoints)
		childrenJoints.push_back(joint);

	parent = nodeToCopy.parent;
}

Guid BXDJ::RigidNode::getGUID() const
{
	return guid;
}

std::string RigidNode::getModelId() const
{
	if (fusionOccurrences.size() > 0)
		return fusionOccurrences[0]->fullPathName();
	else
		return "empty";
}

void RigidNode::write(XmlWriter & output) const
{
	// Generate filename
	std::string filename = "node_" + std::to_string(guid.getSeed()) + ".bxda";

	// Write node information to XML file
	output.startElement("Node");
	output.writeAttribute("GUID", guid.toString());

	output.writeElement("ParentID", (parent == NULL) ? "-1" : std::to_string(parent->getParent()->guid.getSeed()));
	output.writeElement("ModelFileName", filename);
	output.writeElement("ModelID", getModelId());

	if (parent != NULL)
		output.write(*parent);

	output.endElement();

	for (std::shared_ptr<Joint> joint : childrenJoints)
	{
		output.write(*joint->getChild());
	}
}

std::string RigidNode::JointSummary::toString() const
{
	std::string output;

	output += "CHILDREN:\n";
	for (auto childParent : children)
		output += childParent.first->fullPathName() + " <= " + childParent.second->fullPathName() + "\n";

	output += "PARENTS:\n";
	for (auto parentChildren : parents)
		output += parentChildren.first->fullPathName() + " => " + std::to_string(parentChildren.second.size()) + "\n";

	output += "RIGIDGROUPS:\n";
	for (auto group : rigidgroups)
		output += group.first->fullPathName() + " == " + std::to_string(group.second.size()) + "\n";

	return output;
}
