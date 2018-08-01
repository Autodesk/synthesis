#include "Mesh.h"
#include "Submesh.h"

//#define SHOW_COLLIDERS

using namespace BXDA;

Mesh::Mesh(Guid guid) : guid(guid)
{}

void Mesh::addSubMesh(const SubMesh & submesh)
{
	subMeshes.push_back(std::make_shared<SubMesh>(submesh));
}

void Mesh::addSubMesh(std::shared_ptr<SubMesh> submesh)
{
	subMeshes.push_back(submesh);
}

void BXDA::Mesh::addPhysics(const Physics & physics)
{
	this->physics += physics;
}

Guid Mesh::getGUID() const
{
	return guid;
}

int Mesh::getVersion() const
{
	return CURRENT_VERSION;
}

void Mesh::calculateWheelShape(Vector3<> axis, Vector3<> origin, double & minWidth, double & maxWidth, double & maxRadius) const
{
	minWidth = 0.0;
	maxWidth = 0.0;
	maxRadius = 0.0;

	bool first = true;

	for (std::shared_ptr<SubMesh> subMesh : subMeshes)
	{
		double radius;
		double lowerWidth;
		double upperWidth;
		
		subMesh->calculateWheelShape(axis, origin, lowerWidth, upperWidth, radius);

		if (first || lowerWidth < minWidth)
			minWidth = lowerWidth;
		if (first || upperWidth > maxWidth)
			maxWidth = upperWidth;
		if (first || radius > maxRadius)
			maxRadius = radius;

		first = false;
	}
}

std::string Mesh::toString()
{
	return "BXDA::Mesh: " + guid.toString() + ", Sub-Meshes: " + std::to_string(subMeshes.size()) + ", Physics Properties: (" + physics.toString() + ")";
}

void Mesh::write(BinaryWriter & output) const
{
	// Generate colliders
	std::vector<std::shared_ptr<SubMesh>> colliders;
	for (std::shared_ptr<SubMesh> submesh : subMeshes)
	{
		std::shared_ptr<SubMesh> tempColliderMesh = std::make_shared<SubMesh>();
		submesh->getConvexCollider(*tempColliderMesh);
		colliders.push_back(tempColliderMesh);
	}

	// Output general information
	output.write(CURRENT_VERSION);
	output.write(guid.toString());

	// Output meshes
#ifndef SHOW_COLLIDERS
	output.write((int)subMeshes.size());
	for (std::shared_ptr<SubMesh> submesh : subMeshes)
		output.write(*submesh);
#else
	// Output colliders as visual mesh as well
	output.write((int)subMeshes.size() + (int)colliders.size());
	for (std::shared_ptr<SubMesh> submesh : subMeshes)
		output.write(*submesh);
	for (std::shared_ptr<SubMesh> collider : colliders)
		output.write(*collider);
#endif

	// Output colliders
	output.write((int)colliders.size());
	for (std::shared_ptr<SubMesh> collider : colliders)
		output.write(*collider);

	// Output physics data
	output.write(physics);
}
