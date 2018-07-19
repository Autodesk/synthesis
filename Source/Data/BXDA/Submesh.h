#pragma once

#include <vector>
#include "Vertex.h"
#include "Surface.h"

namespace BXDA
{
	class SubMesh
	{
	public:
		SubMesh();
		~SubMesh();

		SubMesh(SubMesh*);
		SubMesh(std::vector<Vertex*> vertices);
		SubMesh(std::vector<Vertex*> vertices, std::vector<Surface*> surfaces);

		friend std::ostream& operator<<(std::ostream&, const SubMesh&);

		void addVertices(std::vector<Vertex*>);
		void addSurface(Surface*);
		void mergeMesh(const SubMesh*); // Merge another submesh's vertices and surfaces with this submesh
		void getConvexCollider(SubMesh*) const; // Fills a submesh with a convex collider of this mesh. NOT FULLY IMPLEMENTED

	private:
		std::vector<Vertex*> vertices;
		std::vector<Surface*> surfaces;
	};
}