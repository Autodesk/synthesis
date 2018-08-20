#pragma once

#include <vector>
#include "BinaryWriter.h"
#include "Vertex.h"
#include "../Vector3.h"

namespace BXDA
{
	class Vertex;
	class Surface;

	/// A container of a vertices and surfaces that exists as a distinct body.
	class SubMesh : public BinaryWritable
	{
	public:
		/// Constructs an empty submesh.
		SubMesh();
		/// Copy constructor.
		SubMesh(const SubMesh &);

		/// Adds vertices to the SubMesh from a Vertex vector.
		void addVertices(std::vector<Vertex>);

		///
		/// Adds vertices to the SubMesh from two vectors of doubles.
		/// \param coords The x, y, and z values of the vertex coordinates. Length should be a multiple of 3.
		/// \param norms The x, y, and z values of the vertex normals. Length should be equal to coords.
		///
		void addVertices(std::vector<double> coords, std::vector<double> norms);

		void addSurface(const Surface &); ///< Copies a Surface to the SubMesh.
		void addSurface(std::shared_ptr<Surface>); ///< Adds a Surface to the SubMesh.
		void mergeMesh(const SubMesh &); ///< Merge another SubMesh's Vertices and Surfaces with this SubMesh

		int getVertCount(); ///< \return The number of vertices currently stored by the submesh
		void getConvexCollider(SubMesh &) const; ///< Fills a submesh with a convex collider of this mesh. Currently, this is simply a bounding box of the SubMesh.
		
		///
		/// Calculates the radial limits of the SubMesh.
		/// \param axis The axis along which the wheel rotates.
		/// \param origin The base point of the wheel's rotation.
		/// \param[out] minWidth The smallest distance along axis at which a vertex resides.
		/// \param[out] maxWidth The largest distance along axis at which a vertex resides.
		/// \param[out] maxRadius The furthest radial distance a vertex is from the axis.
		///
		void calculateWheelShape(Vector3<>, Vector3<>, double & minWidth, double & maxWidth, double & maxRadius) const;

	private:
		std::vector<Vertex> vertices; ///< The vertices in the SubMesh.
		std::vector<std::shared_ptr<Surface>> surfaces; ///< The surfaces of the SubMesh.

		void write(BinaryWriter &) const;

	};
}
