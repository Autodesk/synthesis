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

SubMesh::SubMesh(const SubMesh & s) : vertices(s.vertices.size()), surfaces(s.surfaces.size())
{
	for (Vertex* vertex : s.vertices)
		vertices.push_back(new Vertex(*vertex));
	for (Surface* surface : s.surfaces)
		surfaces.push_back(new Surface(*surface));
}

SubMesh::SubMesh(const std::vector<Vertex> & vertices) : vertices(vertices.size())
{
	for (Vertex vertex : vertices)
		this->vertices.push_back(new Vertex(vertex));
}

SubMesh::SubMesh(const std::vector<Vertex> & vertices, const std::vector<Surface> & surfaces) : SubMesh(vertices)
{
	for (Surface surface : surfaces)
		this->surfaces.push_back(new Surface(surface));
}

std::ostream& BXDA::operator<<(std::ostream& output, const SubMesh& s)
{
	// Output vertices' locations
	output << (int)s.vertices.size() * 3;
	for (Vertex * vertex : s.vertices)
		output << vertex->location;

	// Output vertices' normals
	output << (int)s.vertices.size() * 3;
	for (Vertex * vertex : s.vertices)
		output << vertex->normal;

	// Output surfaces
	output << (int)s.surfaces.size();
	for (Surface * surface : s.surfaces)
		output << *surface;

	return output;
}

void SubMesh::addVertices(std::vector<Vertex> vertices)
{
	for (Vertex vertex : vertices)
		this->vertices.push_back(new Vertex(vertex));
}

void SubMesh::addSurface(const Surface & s)
{
	surfaces.push_back(new Surface(s));
}

void SubMesh::mergeMesh(const SubMesh & other)
{
	// Store offset that will be used to increment indices from other mesh
	int offset = (int)vertices.size();

	// Add other mesh's vertices to this mesh
	for (Vertex * vertex : other.vertices)
		this->vertices.push_back(new Vertex(*vertex));

	// Add other mesh's triangles to this mesh
	for (Surface * surface : other.surfaces)
	{
		Surface * newSurface = new Surface(*surface);
		newSurface->offsetIndices(offset);
		addSurface(*newSurface);
	}
}

void SubMesh::getConvexCollider(SubMesh & outputMesh) const
{
	outputMesh = SubMesh();

	// TODO: Use convex hull to actually get a convex collider

	for (Vertex * vertex : vertices)
		outputMesh.vertices.push_back(new Vertex(*vertex));

	// Create one surface that contains all triangles
	Surface * newSurface = new Surface();
	for (Surface * surface : surfaces)
		newSurface->addTriangles(*surface);

	outputMesh.addSurface(*newSurface); // Actual convex hull should have only one surface

	delete newSurface;
}
