#include "Mesh.h"

using namespace BXDA;

Mesh::Mesh()
{
	guid = "0ba8e1ce-1004-4523-b844-9bfa69efada9";
}

void Mesh::addSubMesh(const SubMesh & submesh)
{
	subMeshes.push_back(std::make_shared<SubMesh>(submesh));
}

void BXDA::Mesh::addPhysics(const Physics & physics)
{
	this->physics += physics;
}

std::string Mesh::getGUID() const
{
	return guid;
}

int Mesh::getVersion() const
{
	return CURRENT_VERSION;
}

std::string Mesh::toString()
{
	return "BXDA::Mesh: " + guid + ", Sub-Meshes: " + std::to_string(subMeshes.size()) + ", Physics Properties: (" + physics.toString() + ")";
}

void Mesh::write(BinaryWriter & output) const
{
	// Output general information
	output.write(CURRENT_VERSION);
	output.write(guid);

	// Output meshes
	output.write((int)subMeshes.size());
	for (std::shared_ptr<SubMesh> submesh : subMeshes)
		output.write(*submesh);

	// Output colliders
	output.write((int)subMeshes.size());
	SubMesh tempColliderMesh = SubMesh();
	for (std::shared_ptr<SubMesh> submesh : subMeshes)
	{
		submesh->getConvexCollider(tempColliderMesh);
		output.write(tempColliderMesh);
	}

	// Output physics data
	output.write(physics);
}
