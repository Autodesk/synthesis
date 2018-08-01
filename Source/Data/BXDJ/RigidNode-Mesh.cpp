#include "RigidNode.h"
#include <Fusion/Components/Occurrence.h>
#include <Fusion/BRep/BRepBody.h>
#include <Fusion/BRep/BRepBodies.h>
#include <Fusion/MeshData/MeshManager.h>
#include <Fusion/MeshData/TriangleMeshCalculator.h>
#include <Fusion/MeshData/TriangleMesh.h>
#include <Fusion/Fusion/PhysicalProperties.h>
#include <Core/Geometry/Point3D.h>
#include "../BXDA/Mesh.h"
#include "../BXDA/SubMesh.h"
#include "../BXDA/Surface.h"
#include "../BXDA/Physics.h"

using namespace BXDJ;

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
			std::shared_ptr<BXDA::Surface> surface = std::make_shared<BXDA::Surface>(fusionMesh->nodeIndices(), subMesh->getVertCount());
			
			// Add color to surface
			core::Ptr<core::Appearance> appearance = nullptr;

			if (occurrence->appearance() != nullptr)
				appearance = occurrence->appearance();
			else if (body->appearance() != nullptr)
				appearance = body->appearance();

			surface->setColor(appearance);
			subMesh->addSurface(surface);

			// Add vertices to sub-mesh
			std::vector<double> coords = fusionMesh->nodeCoordinatesAsDouble();
			std::vector<double> norms = fusionMesh->normalVectorsAsDouble();
			subMesh->addVertices(coords, norms);
		}

		if (subMesh->getVertCount() > 0)
		{
			// Add physics properties to mesh
			core::Ptr<fusion::PhysicalProperties> physics = occurrence->physicalProperties();
			if (physics->mass() > 0)
			{
				Vector3<float> centerOfMass((float)physics->centerOfMass()->x(), (float)physics->centerOfMass()->y(), (float)physics->centerOfMass()->z());
				mesh.addPhysics(BXDA::Physics(centerOfMass, (float)occurrence->physicalProperties()->mass()));
			}

			mesh.addSubMesh(subMesh);
		}
	}
}
