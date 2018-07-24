#include "RigidNode.h"

using namespace BXDJ;

RigidNode::RigidNode()
{}

RigidNode::~RigidNode()
{
	for (Joint * joint : childrenJoints)
		delete joint;
}

RigidNode::RigidNode(core::Ptr<fusion::Component> rootComponent)
{
	buildTree(rootComponent);
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
		if (physics->mass() > 0)
		{
			Vector3<float> centerOfMass(physics->centerOfMass()->x(), physics->centerOfMass()->y(), physics->centerOfMass()->z());
			mesh.addPhysics(BXDA::Physics(centerOfMass, occurence->physicalProperties()->mass()));
		}

		mesh.addSubMesh(subMesh);
	}

	return true;
}

void RigidNode::buildTree(core::Ptr<fusion::Component> rootComponent)
{
	std::map<core::Ptr<fusion::Occurrence>, std::vector<core::Ptr<fusion::Joint>>> jointedOccurences;

	// Find all joints in the structure
	for (core::Ptr<fusion::Joint> joint : rootComponent->allJoints())
	{
		if (jointedOccurences.find(joint->occurrenceOne()) == jointedOccurences.end)
			jointedOccurences[joint->occurrenceOne()] = std::vector<core::Ptr<fusion::Joint>>();
		jointedOccurences[joint->occurrenceOne()].push_back(joint);

		if (jointedOccurences.find(joint->occurrenceTwo()) == jointedOccurences.end)
			jointedOccurences[joint->occurrenceTwo()] = std::vector<core::Ptr<fusion::Joint>>();
		jointedOccurences[joint->occurrenceTwo()].push_back(joint);
	}

	// Add all unjointed occurences in the first level of the component to the root node
	for (core::Ptr<fusion::Occurrence> occurence : rootComponent->occurrences())
	{
		if (jointedOccurences.find[occurence] == jointedOccurences.end)
			fusionOccurences.push_back(occurence);
	}
}
