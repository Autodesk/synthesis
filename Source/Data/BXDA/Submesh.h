#pragma once

#include <vector>
#include "Vertex.h"
#include "Surface.h"

using namespace std;

namespace BXDA
{
	class SubMesh
	{
	public:
		SubMesh();
		~SubMesh();

		SubMesh(SubMesh*);
		SubMesh(vector<Vertex*> vertices);
		SubMesh(vector<Vertex*> vertices, vector<Surface*> surfaces);

		friend std::ostream& operator<<(std::ostream&, const SubMesh&);

		void addVertices(vector<Vertex*>);
		void addSurface(Surface*);
		void mergeMesh(const SubMesh*); // Merge another submesh's vertices and surfaces with this submesh
		void getConvexCollider(SubMesh*) const; // Fills a submesh with a convex collider of this mesh. NOT FULLY IMPLEMENTED

	private:
		vector<Vertex*> vertices;
		vector<Surface*> surfaces;
	};
}