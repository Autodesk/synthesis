#include "RigidNode.h"
#include <Fusion/Components/Occurrences.h>
#include <Fusion/Components/OccurrenceList.h>
#include "ConfigData.h"
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

	for (core::Ptr<fusion::Occurrence> occurence : nodeToCopy.fusionOccurrences)
		fusionOccurrences.push_back(occurence);

	for (std::shared_ptr<Joint> joint : nodeToCopy.childrenJoints)
		childrenJoints.push_back(joint);

	parent = nodeToCopy.parent;

	log = nodeToCopy.log;
}

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

	// Write mesh to binary file (use pointers to dispose of mesh before recursing
	BXDA::BinaryWriter * binary = new BXDA::BinaryWriter(Filesystem::getCurrentRobotDirectory(configData->robotName) + filename);
	BXDA::Mesh * mesh = new BXDA::Mesh(guid);
	getMesh(*mesh);
	binary->write(*mesh);
	delete mesh; delete binary;

	for (std::shared_ptr<Joint> joint : childrenJoints)
	{
		output.write(*joint->getChild());
	}
}
