#include "Mesh.h"

using namespace BXDA;

Mesh::Mesh()
{
	guid = "aaaaaaaaaaa";
}

Mesh::~Mesh()
{
	for (SubMesh * submesh : subMeshes)
		delete submesh;
}

std::ostream& BXDA::operator<<(std::ostream& output, const Mesh& m)
{
	output << m.guid << m.CURRENT_VERSION;

	// Output meshes
	for (SubMesh * submesh : m.subMeshes)
		output << *submesh;

	// Output colliders
	SubMesh * tempColliderMesh = new SubMesh;
	for (SubMesh * submesh : m.subMeshes)
	{
		submesh->getConvexCollider(*tempColliderMesh);
		output << tempColliderMesh;
	}

	// Output physics data
	return output << m.physics;
}

void Mesh::addSubMesh(const SubMesh & submesh)
{
	subMeshes.push_back(new SubMesh(submesh));
}

std::string Mesh::getGUID() const
{
	return guid;
}

int Mesh::getVersion() const
{
	return CURRENT_VERSION;
}
