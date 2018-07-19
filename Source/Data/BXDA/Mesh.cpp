#include "Mesh.h"

using namespace BXDA;

Mesh::Mesh()
{
	guid = "aaaaaaaaaaa";
}

Mesh::~Mesh()
{
	for (Submesh * submesh : submeshes)
		delete submesh;
}

void Mesh::addSubmesh(Submesh * submesh)
{
	submeshes.push_back(new Submesh(submesh));
}

string Mesh::getGUID()
{
	return guid;
}

int Mesh::getVersion()
{
	return CURRENT_VERSION;
}
