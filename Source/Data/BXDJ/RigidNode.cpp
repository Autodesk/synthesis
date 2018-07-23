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

		// Add physics properties to mesh
		core::Ptr<fusion::PhysicalProperties> physics = occurence->physicalProperties();
		if (physics->mass > 0)
		{
			Vector3<float> centerOfMass(physics->centerOfMass()->x(), physics->centerOfMass()->y(), physics->centerOfMass()->z());
			mesh.addPhysics(BXDA::Physics(centerOfMass, occurence->physicalProperties()->mass()));
		}

		mesh.addSubMesh(subMesh);
	}

	return true;
}

bool RigidNode::addOccurence(core::Ptr<fusion::Occurrence> occurence)
{
	evilGlobalVariableForPrinting += "Occurence \"" + occurence->fullPathName();

	// Check if the occurence already exists
	/*for (core::Ptr<fusion::Occurrence> existingOccurence : fusionOccurences)
	{
		if (existingOccurence == occurence)
		{
			evilGlobalVariableForPrinting += " skipped\n";
			return false;
		}
	}*/

	evilGlobalVariableForPrinting += " added\n";

	// Add it to the list of occurences
	fusionOccurences.push_back(occurence);

	for (core::Ptr<fusion::Occurrence> subOccurence : occurence->childOccurrences())
		addOccurence(subOccurence);

	// Add any occurences attached by rigid joints, and connect any occurences connected by other joints
	/*if (occurence->joints() != nullptr)
	{
		for (core::Ptr<fusion::Joint> joint : occurence->joints())
		{
			if (joint->jointMotion()->jointType() == fusion::JointTypes::RigidJointType)
			{
				if (joint->occurrenceOne()->fullPathName() != occurence->fullPathName())
				{
					if (parent == nullptr || joint->occurrenceOne()->fullPathName() != parent->fullPathName())
						addOccurence(joint->occurrenceOne(), occurence);
				}
				else
				{
					if (parent == nullptr || joint->occurrenceTwo()->fullPathName() != parent->fullPathName())
						addOccurence(joint->occurrenceTwo(), occurence);
				}
			}
			else if (joint->jointMotion()->jointType() == fusion::JointTypes::RevoluteJointType)
			{
				/*if (joint->occurrenceOne() != nullptr && joint->occurrenceOne()->name() != occurence->name())
				childrenJoints.push_back(new Joint(RigidNode(joint->occurrenceOne())));
				else if (joint->occurrenceTwo() != nullptr)
				childrenJoints.push_back(new Joint(RigidNode(joint->occurrenceTwo())));*//*
			}
		}
	}*/

	return true;
}
