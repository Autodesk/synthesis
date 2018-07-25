#include "RigidNode.h"
#include "Joint.h"
#include "Joints/RotationalJoint.h"

using namespace BXDJ;

RigidNode::RigidNode()
{
	guid = "0ba8e1ce-1004-4523-b844-9bfa69efada9";
	parent = NULL;
}

RigidNode::RigidNode(const RigidNode & nodeToCopy) : RigidNode()
{
	for (core::Ptr<fusion::Occurrence> occurence : nodeToCopy.fusionOccurrences)
		fusionOccurrences.push_back(occurence);

	for (std::shared_ptr<Joint> joint : nodeToCopy.childrenJoints)
		childrenJoints.push_back(joint);

	parent = nodeToCopy.parent;

	log = nodeToCopy.log;
}

RigidNode::RigidNode(core::Ptr<fusion::Component> rootComponent) : RigidNode()
{
	JointSummary jointSummary = getJointSummary(rootComponent);
	
	for (core::Ptr<fusion::Occurrence> occurence : rootComponent->occurrences()->asList())
		buildTree(occurence, jointSummary);
}

Joint * RigidNode::getParent()
{
	return parent;
}

void RigidNode::getMesh(BXDA::Mesh & mesh) const
{
	// Each occurrence is a submesh
	for (core::Ptr<fusion::Occurrence> occurrence : fusionOccurrences)
	{
		std::shared_ptr<BXDA::SubMesh> subMesh = std::make_shared<BXDA::SubMesh>();

		// Each body of the mesh is a sub-mesh
		for (core::Ptr<fusion::BRepBody> body : occurrence->bRepBodies())
		{
			core::Ptr<fusion::TriangleMeshCalculator> meshCalculator = body->meshManager()->createMeshCalculator();
			meshCalculator->setQuality(fusion::LowQualityTriangleMesh);

			core::Ptr<fusion::TriangleMesh> fusionMesh = meshCalculator->calculate();

			// Add faces to sub-mesh
			std::vector<int> indices = fusionMesh->nodeIndices();
			subMesh->addSurface(std::make_shared<BXDA::Surface>(indices, subMesh->getVertCount()));

			// Add vertices to sub-mesh
			std::vector<double> coords = fusionMesh->nodeCoordinatesAsDouble();
			std::vector<double> norms = fusionMesh->normalVectorsAsDouble();
			subMesh->addVertices(coords, norms);
		}

		// Add physics properties to mesh
		core::Ptr<fusion::PhysicalProperties> physics = occurrence->physicalProperties();
		if (physics->mass() > 0)
		{
			Vector3<float> centerOfMass(physics->centerOfMass()->x(), physics->centerOfMass()->y(), physics->centerOfMass()->z());
			mesh.addPhysics(BXDA::Physics(centerOfMass, occurrence->physicalProperties()->mass()));
		}

		mesh.addSubMesh(subMesh);
	}
}

void RigidNode::connectToJoint(Joint * parent)
{
	this->parent = parent;
}

void RigidNode::addJoint(std::shared_ptr<Joint> joint)
{
	childrenJoints.push_back(joint);
}

RigidNode::JointSummary RigidNode::getJointSummary(core::Ptr<fusion::Component> rootComponent)
{
	JointSummary jointSummary;

	// Find all jointed occurrences in the structure
	for (core::Ptr<fusion::Joint> joint : rootComponent->allJoints())
	{
		core::Ptr<fusion::Occurrence> lowerOccurrence;
		core::Ptr<fusion::Occurrence> upperOccurrence;

		// Find which occurence is higher in the heirarchy
		if (levelOfOccurrence(joint->occurrenceOne()) > levelOfOccurrence(joint->occurrenceTwo()))
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

	return jointSummary;
}

void RigidNode::buildTree(core::Ptr<fusion::Occurrence> rootOccurrence, JointSummary & jointSummary)
{
	// Add the occurence to this node
	log += "Adding occurence \"" + rootOccurrence->fullPathName() + "\"\n";
	fusionOccurrences.push_back(rootOccurrence);

	// Create a joint from this occurence if it is the parent of a joint
	if (jointSummary.parents.find(rootOccurrence) != jointSummary.parents.end())
		for (core::Ptr<fusion::Joint> joint : jointSummary.parents[rootOccurrence])
			addJoint(joint, rootOccurrence, jointSummary);

	// Add all occurrences without joints or that are only parents in joints to the root node
	for (core::Ptr<fusion::Occurrence> occurrence : rootOccurrence->childOccurrences())
		// Add the occurence to this node if it is not the child of a joint
		if (std::find(jointSummary.children.begin(), jointSummary.children.end(), occurrence) == jointSummary.children.end())
			buildTree(occurrence, jointSummary);

	log += "\n";
}

void BXDJ::RigidNode::addJoint(core::Ptr<fusion::Joint> joint, core::Ptr<fusion::Occurrence> parent, JointSummary & jointSummary)
{
	core::Ptr<fusion::Occurrence> child = (joint->occurrenceOne() != parent) ? joint->occurrenceOne() : joint->occurrenceTwo();
	log += "Jointing occurence \"" + child->fullPathName() + "\"\n";

	switch (joint->jointMotion()->jointType())
	{
	case fusion::JointTypes::RevoluteJointType:
		addJoint(std::make_shared<RotationalJoint>(RigidNode(child, jointSummary), this, joint->jointMotion()));
		break;

	default:
		buildTree(child, jointSummary);
	}
}

void RigidNode::write(XmlWriter & output) const
{
	output.startElement("Node");
	output.writeAttribute("GUID", guid);

	output.writeElement("ParentID", parent->getParent()->guid);
	output.writeElement("ModelFileName", guid + ".bxda");
	output.writeElement("ModelID", "modelID");

	output.endElement();

	for (std::shared_ptr<Joint> joint : childrenJoints)
	{
		output.write(*joint);
	}
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
