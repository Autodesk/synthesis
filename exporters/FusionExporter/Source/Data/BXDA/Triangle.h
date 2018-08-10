#pragma once

#include "BinaryWriter.h"

namespace BXDA
{
	/// A triangle face on a Surface of a SubMesh.
	class Triangle : public BinaryWritable
	{
	public:
		unsigned int vertexIndices[3]; ///< The indices of the triangle's vertices within its containing submesh.

		/// Constructs a triangle with all vertex indices equal to 0.
		Triangle();
		/// Copy constructor.
		Triangle(const Triangle & triangle);

		///
		/// Constructs a triangle with the indices contained in an array of three unsigned ints.
		/// \param vertexIndices Indices of the triangle's vertices.
		///
		Triangle(const unsigned int vertexIndices[]);

		///
		/// Constructs a triangle with the given vertex indices.
		/// \param vertexIndex0 The index of the triangle's first vertex.
		/// \param vertexIndex1 The index of the triangle's second vertex.
		/// \param vertexIndex2 The index of the triangle's third vertex.
		///
		Triangle(unsigned int vertexIndex0, unsigned int vertexIndex1, unsigned int vertexIndex2);

	private:
		void write(BinaryWriter &) const;

	};
}
