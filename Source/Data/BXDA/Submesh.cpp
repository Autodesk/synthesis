#include "Submesh.h"

using namespace BXDA;

Submesh::Submesh()
{

}

Submesh::~Submesh()
{
	for (Vertex* vertex : vertices)
		delete vertex;
	for (Surface* surface : surfaces)
		delete surface;
}

Submesh::Submesh(Submesh* s) : vertices(s->vertices.size()), surfaces(s->surfaces.size())
{
	for (Vertex* vertex : s->vertices)
		vertices.push_back(new Vertex(vertex));
	for (Surface* surface : s->surfaces)
		surfaces.push_back(new Surface(surface));
}

Submesh::Submesh(vector<Vertex*> vertices) : vertices(vertices.size())
{
	for (Vertex* vertex : vertices)
		this->vertices.push_back(new Vertex(vertex));
}

Submesh::Submesh(vector<Vertex*> vertices, vector<Surface*> surfaces) : Submesh(vertices)
{
	for (Surface* surface : surfaces)
		this->surfaces.push_back(new Surface(surface));
}

void Submesh::addSurface(Surface* s)
{
	surfaces.push_back(new Surface(s));
}