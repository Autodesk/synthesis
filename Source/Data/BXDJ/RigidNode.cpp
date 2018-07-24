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

RigidNode::RigidNode(core::Ptr<fusion::Occurrence> occurrence) : RigidNode(occurrence->component())
{
	fusionOccurrences.push_back(occurrence);
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

void RigidNode::buildTree(core::Ptr<fusion::Component> rootComponent)
{
	std::vector<core::Ptr<fusion::Occurrence>> jointedOccurrencesChildren;
	std::map<core::Ptr<fusion::Occurrence>, std::vector<core::Ptr<fusion::Occurrence>>> jointedOccurrencesParents;

	// Find all jointed occurrences in the structure
	for (core::Ptr<fusion::Joint> joint : rootComponent->allJoints())
	{
		core::Ptr<fusion::Joint> lowerOccurrence;
		core::Ptr<fusion::Joint> upperOccurrence;
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

		jointedOccurrencesChildren.push_back(lowerOccurrence);
		jointedOccurrencesParents[upperOccurrence].push_back(lowerOccurrence);
	}

	// Add all occurrences without joints or that are only parents in joints to the root node
	for (core::Ptr<fusion::Occurrence> occurrence : rootComponent->occurrences()->asList())
	{
		if (std::find(jointedOccurrencesChildren.begin(), jointedOccurrencesChildren.end(), occurrence) == jointedOccurrencesChildren.end())
		{
			fusionOccurrences.push_back(occurrence);
			buildTree(occurrence->component());
		}
		else if (jointedOccurrencesParents.find(occurrence) != jointedOccurrencesParents.end())
		{
			for (core::Ptr<fusion::Occurrence> subOccurrence : jointedOccurrencesParents[occurrence])
			{
				RigidNode subNode(subOccurrence);
				addJoint(Joint(subNode));
			}
		}
	}
}

int RigidNode::levelOfOccurrence(core::Ptr<fusion::Occurrence> occurrence)
{
	return std::count(occurrence->fullPathName().begin(), occurrence->fullPathName().end(), '+');
}
