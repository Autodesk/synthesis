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

	for (SubMesh * submesh : m.subMeshes)
		output << *submesh;

	return output;
}

void Mesh::addSubmesh(SubMesh * submesh)
{
	subMeshes.push_back(new SubMesh(submesh));
}

string Mesh::getGUID()
{
	return guid;
}

int Mesh::getVersion()
{
	return CURRENT_VERSION;
}
