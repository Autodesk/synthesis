#include "RigidNode.h"
#include <Fusion/Components/Occurrence.h>
#include <Fusion/BRep/BRepBody.h>
#include <Fusion/BRep/BRepBodies.h>
#include <Fusion/MeshData/MeshManager.h>
#include <Fusion/MeshData/TriangleMeshCalculator.h>
#include <Fusion/MeshData/TriangleMesh.h>
#include <Fusion/Fusion/PhysicalProperties.h>
#include <Core/Materials/Material.h>
#include <Core/Geometry/Point3D.h>
#include "../BXDA/Mesh.h"
#include "../BXDA/SubMesh.h"
#include "../BXDA/Surface.h"
#include "../BXDA/Physics.h"

using namespace BXDJ;

void RigidNode::getMesh(BXDA::Mesh & mesh, bool ignorePhysics, std::function<void(double)> progressCallback, const bool * cancel) const
{
	if (progressCallback)
		progressCallback(0);

	// Each occurrence is a submesh
	for (int occ = 0; occ < fusionOccurrences.size(); occ++)
	{
		core::Ptr<fusion::Occurrence> occurrence = fusionOccurrences[occ];
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
			surface->setColor(body->material(), body->appearance());
			subMesh->addSurface(surface);

			// Add vertices to sub-mesh
			std::vector<double> coords = fusionMesh->nodeCoordinatesAsDouble();
			std::vector<double> norms = fusionMesh->normalVectorsAsDouble();
			subMesh->addVertices(coords, norms);
		}

		if (subMesh->getVertCount() > 0)
		{
			if (!ignorePhysics)
			{
				// Add physics properties to mesh
				core::Ptr<fusion::PhysicalProperties> physics = occurrence->physicalProperties();
				double mass = physics->mass();
				if (mass > 0)
				{
					core::Ptr<core::Point3D> com = physics->centerOfMass();
					Vector3<float> centerOfMass((float)com->x(), (float)com->y(), (float)com->z());
					mesh.addPhysics(BXDA::Physics(centerOfMass, (float)mass));
				}
			}

			mesh.addSubMesh(subMesh);
		}

		// Check if thread is being canceled
		if (cancel != nullptr)
			if (*cancel)
				return;

		// Update progress
		if (progressCallback)
			progressCallback((double)(occ + 1) / fusionOccurrences.size());
	}

	// Close progress bar
	if (progressCallback)
		progressCallback(1);
}
