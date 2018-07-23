#include "RigidNode.h"

using namespace BXDJ;

RigidNode::RigidNode()
{}

RigidNode::~RigidNode()
{
	for (Joint * joint : childrenJoints)
		delete joint;
}

RigidNode::RigidNode(core::Ptr<fusion::Occurrence> occurence)
{
	addOccurence(occurence);
}

bool RigidNode::addOccurence(core::Ptr<fusion::Occurrence> occurence)
{
	// Check if the occurence already exists
	for (core::Ptr<fusion::Occurrence> existingOccurence : fusionOccurences)
		if (existingOccurence->name() == occurence->name())
			return false;

	// Add it to the list of occurences
	fusionOccurences.push_back(occurence);

	// Add any attached occurences to the list (Rigid Group or Rigid Joints)
	for (core::Ptr<fusion::RigidGroup> rigidGroup : occurence->rigidGroups())
		for (core::Ptr<fusion::Occurrence> subOccurence : rigidGroup->occurrences())
			addOccurence(subOccurence);

	for (core::Ptr<fusion::Joint> joint : occurence->joints())
	{
		if (joint->jointMotion()->jointType() == fusion::JointTypes::RigidJointType)
		{
			if (!addOccurence(joint->occurrenceOne())) // If the first one succeeds, we know the second one will fail (it is this occurence)
				addOccurence(joint->occurrenceTwo());
		}
		else if (joint->jointMotion()->jointType() == fusion::JointTypes::RevoluteJointType) // Add any other joints as children
		{
			if (joint->occurrenceOne() != nullptr && joint->occurrenceOne()->name() != occurence->name())
				childrenJoints.push_back(new Joint(RigidNode(joint->occurrenceOne())));
			else if (joint->occurrenceTwo() != nullptr)
				childrenJoints.push_back(new Joint(RigidNode(joint->occurrenceTwo())));
		}
	}

	return true;
}

bool RigidNode::getMesh(BXDA::Mesh & mesh)
{
	// Each occurence is a submesh
	for (core::Ptr<fusion::Occurrence> occurence : fusionOccurences)
	{
		BXDA::SubMesh subMesh = BXDA::SubMesh();

		for (core::Ptr<fusion::BRepBody> body : occurence->bRepBodies())
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

		mesh.addSubMesh(subMesh);
	}

	return true;
}
