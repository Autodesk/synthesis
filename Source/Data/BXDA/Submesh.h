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

		SubMesh(const SubMesh &);

		void addVertices(std::vector<Vertex>);
		void addVertices(std::vector<double> coords, std::vector<double> norms);
		void addSurface(const Surface &);
		void addSurface(std::shared_ptr<Surface>);
		void mergeMesh(const SubMesh &); // Merge another submesh's vertices and surfaces with this submesh

		int getVertCount(); // Gets the number of vertices currently stored by the submesh
		void getConvexCollider(SubMesh &) const; // Fills a submesh with a convex collider of this mesh. NOT FULLY IMPLEMENTED

	private:
		std::vector<Vertex> vertices;
		std::vector<std::shared_ptr<Surface>> surfaces;

		void write(BinaryWriter &) const;

	};
}
