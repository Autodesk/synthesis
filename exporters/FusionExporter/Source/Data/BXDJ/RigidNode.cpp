#include "RigidNode.h"
#include <Fusion/Components/Occurrences.h>
#include <Fusion/Components/OccurrenceList.h>
#include "Joint.h"
#include "../Filesystem.h"
#include "../BXDA/Mesh.h"

using namespace BXDJ;

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

int BXDJ::RigidNode::getOccurrenceCount() const
{
	return fusionOccurrences.size();
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
using json = nlohmann::json;

json RigidNode::GetJson()
{
	json nodeArrayJson = json::array();
	for (std::shared_ptr<Joint> joint : childrenJoints)
	{	

		RigidNode* node = joint->getParent();
		std::string filename = "node_" + std::to_string(node->guid.getSeed()) + ".bxda";
		json nodeJson;
		nodeJson["GUID"] = node->guid.toString();
		nodeJson["ParentID"] = (node->parent == NULL) ? "-1" : std::to_string(node->parent->getParent()->guid.getSeed());
		nodeJson["ModelFileName"] = filename;
		nodeJson["ModelID"] = node->getModelId();
		nodeJson["joint"] = joint->GetJson();
		nodeArrayJson.push_back(nodeJson);
	}

	

	
	
	

	return nodeArrayJson;
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
