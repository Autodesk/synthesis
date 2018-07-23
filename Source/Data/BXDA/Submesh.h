#pragma once

#include <vector>
#include "BinaryWriter.h"
#include "Vertex.h"
#include "Surface.h"

namespace BXDA
{
	class SubMesh : public BinaryWritable
	{
	public:
		SubMesh();
		~SubMesh();

		SubMesh(const SubMesh &);
		SubMesh(const std::vector<Vertex> &);
		SubMesh(const std::vector<Vertex> &, const std::vector<Surface> &);

		void addVertices(std::vector<Vertex>);
		void addSurface(const Surface &);
		void mergeMesh(const SubMesh &); // Merge another submesh's vertices and surfaces with this submesh
		void getConvexCollider(SubMesh &) const; // Fills a submesh with a convex collider of this mesh. NOT FULLY IMPLEMENTED

	private:
		std::vector<Vertex*> vertices;
		std::vector<Surface*> surfaces;

		void write(BinaryWriter &) const;

	};
}
