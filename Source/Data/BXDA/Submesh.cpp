#include "Submesh.h"

using namespace BXDA;

SubMesh::SubMesh()
{}

SubMesh::SubMesh(const SubMesh & s) : vertices(s.vertices.size()), surfaces(s.surfaces.size())
{
	for (int v = 0; v < s.vertices.size(); v++)
		vertices[v] = s.vertices[v];

	for (int i = 0; i < s.surfaces.size(); i++)
		surfaces[i] = s.surfaces[i];
}

void SubMesh::addVertices(std::vector<Vertex> vertices)
{
	for (Vertex vertex : vertices)
		this->vertices.push_back(vertex);
}

void SubMesh::addSurface(const Surface & s)
{
	surfaces.push_back(std::make_shared<Surface>(s));
}

void SubMesh::mergeMesh(const SubMesh & other)
{
	// Store offset that will be used to increment indices from other mesh
	int offset = (int)vertices.size();

	// Add other mesh's vertices to this mesh
	for (Vertex vertex : other.vertices)
		this->vertices.push_back(vertex);

	// Add other mesh's triangles to this mesh
	for (std::shared_ptr<Surface> surface : other.surfaces)
	{
		std::shared_ptr<Surface> newSurface = std::make_shared<Surface>(*surface);
		newSurface->offsetIndices(offset);
		surfaces.push_back(newSurface);
	}
}

void SubMesh::getConvexCollider(SubMesh & outputMesh) const
{
	outputMesh = SubMesh();

	// TODO: Use convex hull to actually get a convex collider

	for (Vertex vertex : vertices)
		outputMesh.vertices.push_back(vertex);

	// Create one surface that contains all triangles
	Surface newSurface;
	for (std::shared_ptr<Surface> surface : surfaces)
		newSurface.addTriangles(*surface);

	outputMesh.addSurface(newSurface); // Actual convex hull should have only one surface
}

void SubMesh::write(BinaryWriter & output) const
{
	output.write((char)1); // Mesh flag for normals

	// Output vertices' locations
	output.write((int)vertices.size() * 3);
	for (Vertex vertex : vertices)
		output.write(vertex.location);

	// Output vertices' normals
	output.write((int)vertices.size() * 3);
	for (Vertex vertex : vertices)
		output.write(vertex.normal);

	// Output surfaces
	output.write((int)surfaces.size());
	for (std::shared_ptr<Surface> surface : surfaces)
		output.write(*surface);
}
