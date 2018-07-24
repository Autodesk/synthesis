#include "RigidNode.h"

using namespace BXDJ;

RigidNode::RigidNode()
{}

RigidNode::~RigidNode()
{
	for (Joint * joint : childrenJoints)
		delete joint;
}

RigidNode::RigidNode(const RigidNode & nodeToCopy)
{
	for (core::Ptr<fusion::Occurrence> occurence : nodeToCopy.fusionOccurrences)
		fusionOccurrences.push_back(occurence);

	for (Joint * joint : nodeToCopy.childrenJoints)
		childrenJoints.push_back(new Joint(*joint));
}

RigidNode::RigidNode(core::Ptr<fusion::Component> rootComponent)
{
	JointSummary jointSummary = getJointSummary(rootComponent);
	
	for (core::Ptr<fusion::Occurrence> occurence : rootComponent->occurrences()->asList())
		buildTree(occurence, jointSummary);
}

void RigidNode::getMesh(BXDA::Mesh & mesh) const
{
	// Each occurrence is a submesh
	for (core::Ptr<fusion::Occurrence> occurrence : fusionOccurrences)
	{
		BXDA::SubMesh subMesh = BXDA::SubMesh();

		for (core::Ptr<fusion::BRepBody> body : occurrence->bRepBodies())
		{
			core::Ptr<fusion::TriangleMeshCalculator> meshCalculator = body->meshManager()->createMeshCalculator();
			meshCalculator->setQuality(fusion::LowQualityTriangleMesh);

			core::Ptr<fusion::TriangleMesh> fusionMesh = meshCalculator->calculate();

			// Add vertices to sub-mesh
			std::vector<BXDA::Vertex> vertices(fusionMesh->nodeCount());
			std::vector<double> coords = fusionMesh->nodeCoordinatesAsDouble();
			std::vector<double> norms = fusionMesh->normalVectorsAsDouble();

			for (int v = 0; v < coords.size(); v += 3)
				vertices[v / 3] = BXDA::Vertex(Vector3<>(coords[v], coords[v + 1], coords[v + 2]), Vector3<>(norms[v], norms[v + 1], norms[v + 2]));

			subMesh.addVertices(vertices);

			// Add faces to sub-mesh
			std::vector<int> indices = fusionMesh->nodeIndices();
			subMesh.addSurface(BXDA::Surface(indices));
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
		jointSummary.parents[upperOccurrence].push_back(lowerOccurrence);
	}

	return jointSummary;
}

void RigidNode::buildTree(core::Ptr<fusion::Occurrence> rootOccurrence, JointSummary & jointSummary)
{
	// Add the occurence to this node
	fusionOccurrences.push_back(rootOccurrence);

	// Create a joint from this occurence if it is the parent of a joint
	if (jointSummary.parents.find(rootOccurrence) != jointSummary.parents.end())
	{
		for (core::Ptr<fusion::Occurrence> subOccurrence : jointSummary.parents[rootOccurrence])
		{
			log += "Jointing occurence \"" + subOccurrence->fullPathName() + "\"\n";
			RigidNode subNode(subOccurrence, jointSummary);
			addJoint(Joint(subNode));
		}
	}

	// Add all occurrences without joints or that are only parents in joints to the root node
	for (core::Ptr<fusion::Occurrence> occurrence : rootOccurrence->childOccurrences())
	{
		// Add the occurence to this node if it is not the child of a joint
		if (std::find(jointSummary.children.begin(), jointSummary.children.end(), occurrence) == jointSummary.children.end())
		{
			log += "Adding occurence \"" + occurrence->fullPathName() + "\"\n";
			buildTree(occurrence, jointSummary);
		}
	}

	log += "\n";
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
