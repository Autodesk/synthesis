#include "Submesh.h"

using namespace BXDA;

SubMesh::SubMesh()
{}

SubMesh::~SubMesh()
{
	for (Vertex* vertex : vertices)
		delete vertex;
	for (Surface* surface : surfaces)
		delete surface;
}

SubMesh::SubMesh(SubMesh* s) : vertices(s->vertices.size()), surfaces(s->surfaces.size())
{
	for (Vertex* vertex : s->vertices)
		vertices.push_back(new Vertex(vertex));
	for (Surface* surface : s->surfaces)
		surfaces.push_back(new Surface(surface));
}

SubMesh::SubMesh(vector<Vertex*> vertices) : vertices(vertices.size())
{
	for (Vertex* vertex : vertices)
		this->vertices.push_back(new Vertex(vertex));
}

SubMesh::SubMesh(vector<Vertex*> vertices, vector<Surface*> surfaces) : SubMesh(vertices)
{
	for (Surface* surface : surfaces)
		this->surfaces.push_back(new Surface(surface));
}

std::ostream& BXDA::operator<<(std::ostream& output, const SubMesh& s)
{
	output << (int)s.vertices.size() * 3;
	for (Vertex* vertex : s.vertices)
		output << vertex->location;

	output << (int)s.vertices.size() * 3;
	for (Vertex* vertex : s.vertices)
		output << vertex->normal;

	return output;
}

void SubMesh::addSurface(Surface* s)
{
	surfaces.push_back(new Surface(s));
}
