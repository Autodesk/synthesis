#include "Submesh.h"
#include "Triangle.h"
#include "Surface.h"
#include <VHACD.h>
#include "../BXDJ/ConfigData.h"

using namespace BXDA;

VHACD::IVHACD* SubMesh::decomper = nullptr;

SubMesh::SubMesh()
{

	if (decomper == nullptr) {
		decomper = VHACD::CreateVHACD();
		
	}
}

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

void SubMesh::addVertices(std::vector<double> coords, std::vector<double> norms)
{
	for (int v = 0; v < coords.size(); v += 3)
		this->vertices.push_back(Vertex(Vector3<>(coords[v], coords[v+1], coords[v+2]),
										Vector3<>(norms[v], norms[v+1], norms[v+2])));
}

void SubMesh::addSurface(const Surface & s)
{
	surfaces.push_back(std::make_shared<Surface>(s));
}

void SubMesh::addSurface(std::shared_ptr<Surface> s)
{
	surfaces.push_back(s);
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

int SubMesh::getVertCount()
{
	return (int)vertices.size();
}

void SubMesh::getConvexCollider(SubMesh & outputMesh, int level) const
{
	
	outputMesh = SubMesh();
	std::vector<Triangle> triangles;

	if (level == BXDJ::ConfigData::ConvexType::BOX || level == 0) {
		Vector3<> min, max; bool first = true;

		for (Vertex vertex : vertices)
		{
			if (vertex.location.x < min.x || first) min.x = vertex.location.x;
			if (vertex.location.x > max.x || first) max.x = vertex.location.x;
			if (vertex.location.y < min.y || first) min.y = vertex.location.y;
			if (vertex.location.y > max.y || first) max.y = vertex.location.y;
			if (vertex.location.z < min.z || first) min.z = vertex.location.z;
			if (vertex.location.z > max.z || first) max.z = vertex.location.z;
			first = false;
		}

		// Create vertices
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, min.y, min.z), Vector3<>(-1, 0, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, min.y, max.z), Vector3<>(-1, 0, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, max.y, max.z), Vector3<>(-1, 0, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, max.y, min.z), Vector3<>(-1, 0, 0)));

		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, min.y, min.z), Vector3<>(0, -1, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, min.y, min.z), Vector3<>(0, -1, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, min.y, max.z), Vector3<>(0, -1, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, min.y, max.z), Vector3<>(0, -1, 0)));

		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, min.y, min.z), Vector3<>(0, 0, -1)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, max.y, min.z), Vector3<>(0, 0, -1)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, max.y, min.z), Vector3<>(0, 0, -1)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, min.y, min.z), Vector3<>(0, 0, -1)));

		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, max.y, max.z), Vector3<>(1, 0, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, min.y, max.z), Vector3<>(1, 0, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, min.y, min.z), Vector3<>(1, 0, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, max.y, min.z), Vector3<>(1, 0, 0)));

		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, max.y, max.z), Vector3<>(0, 1, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, max.y, min.z), Vector3<>(0, 1, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, max.y, min.z), Vector3<>(0, 1, 0)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, max.y, max.z), Vector3<>(0, 1, 0)));

		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, max.y, max.z), Vector3<>(0, 0, 1)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, max.y, max.z), Vector3<>(0, 0, 1)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(min.x, min.y, max.z), Vector3<>(0, 0, 1)));
		outputMesh.vertices.push_back(Vertex(Vector3<>(max.x, min.y, max.z), Vector3<>(0, 0, 1)));

	
		triangles.push_back(Triangle(0, 1, 2));
		triangles.push_back(Triangle(2, 3, 0));

		triangles.push_back(Triangle(4, 5, 6));
		triangles.push_back(Triangle(6, 7, 4));

		triangles.push_back(Triangle(8, 9, 10));
		triangles.push_back(Triangle(10, 11, 8));

		triangles.push_back(Triangle(12, 13, 14));
		triangles.push_back(Triangle(14, 15, 12));

		triangles.push_back(Triangle(16, 17, 18));
		triangles.push_back(Triangle(18, 19, 16));

		triangles.push_back(Triangle(20, 21, 22));
		triangles.push_back(Triangle(22, 23, 20));

	}

	if (level == BXDJ::ConfigData::ConvexType::VHACD_LOW || level == BXDJ::ConfigData::ConvexType::VHACD_MID|| level == BXDJ::ConfigData::ConvexType::VHACD_HIGH) {
		VHACD::IVHACD::Parameters parms;

		parms.Init();
		if (level == BXDJ::ConfigData::ConvexType::VHACD_LOW) {
			parms.m_resolution = 1000;
			parms.m_convexhullDownsampling = 5;
			parms.m_concavity = 1.0;
		}
		
		if (level == BXDJ::ConfigData::ConvexType::VHACD_MID){
			parms.m_resolution = 10000;
			parms.m_convexhullDownsampling = 5;
			parms.m_concavity = 0.25;
		}
		if (level == BXDJ::ConfigData::ConvexType::VHACD_HIGH) {
			parms.m_resolution = 150000;
			parms.m_convexhullDownsampling = 4;
			parms.m_concavity = 0.0075;
		}

		// Find bounding box
		Vector3<> min, max; bool first = true;
		std::vector<double> mPoints;
		std::vector<int> mTriangles;

		for (int i = 0; i < vertices.size(); i++) {
			mPoints.push_back(vertices[i].location.x);
			mPoints.push_back(vertices[i].location.y);
			mPoints.push_back(vertices[i].location.z);
		}

		for (int i = 0; i < surfaces.size(); i++) {
			for (int j = 0; j < surfaces[i]->triangles.size(); j++) {
				mTriangles.push_back(surfaces[i]->triangles[j].vertexIndices[0]);
				mTriangles.push_back(surfaces[i]->triangles[j].vertexIndices[1]);
				mTriangles.push_back(surfaces[i]->triangles[j].vertexIndices[2]);

			}
		}

		bool result = decomper->Compute(&mPoints[0], (unsigned int)mPoints.size() / 3,
			(const uint32_t*)& mTriangles[0], (unsigned int)mTriangles.size() / 3, parms);

		unsigned int nConvexHulls = decomper->GetNConvexHulls();
		VHACD::IVHACD::ConvexHull ch;

		int vOffset = 1;
		for (int x = 0; x < nConvexHulls; x++) {
			decomper->GetConvexHull(x, ch);


			for (int i = 0; i < ch.m_nPoints * 3; i += 3) {
				outputMesh.vertices.push_back(Vertex(Vector3<>(ch.m_points[i], ch.m_points[i + 1], ch.m_points[i + 2]), Vector3<>(0, 0, 0)));
			}

			for (int i = 0; i < ch.m_nTriangles * 3; i += 3)
			{
				triangles.push_back(Triangle(ch.m_triangles[i] + vOffset - 1, ch.m_triangles[i + 1] + vOffset - 1, ch.m_triangles[i + 2] + vOffset - 1));

			}
			vOffset += ch.m_nPoints;

		}

		decomper->Clean();
	}
	// Create surface
	Surface newSurface;
	newSurface.addTriangles(triangles);
	newSurface.setColor(0, 255, 0, 64); // Used when rendering colliders
	outputMesh.addSurface(newSurface);
}

void SubMesh::calculateWheelShape(Vector3<> axis, Vector3<> origin, double & minWidth, double & maxWidth, double & maxRadius) const
{
	minWidth = 0.0;
	maxWidth = 0.0;
	maxRadius = 0.0;

	bool first = true;

	for (Vertex v : vertices)
	{
		double width;
		double radius;

		v.location.getRadialCoordinates(axis, origin, width, radius);

		if (first || width < minWidth)
			minWidth = width;
		if (first || width > maxWidth)
			maxWidth = width;
		if (first || radius > maxRadius)
			maxRadius = radius;
		
		first = false;
	}
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
