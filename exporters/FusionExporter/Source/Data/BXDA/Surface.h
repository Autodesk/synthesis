#pragma once

#include <vector>
#include <Core/Materials/Material.h>
#include <Core/Materials/Appearance.h>
#include "BinaryWriter.h"
#include "Triangle.h"

using namespace adsk;

namespace BXDA
{
	/// A collection of triangles with a common appearance.
	class Surface : public BinaryWritable
	{
	public:
		/// Constructs an empty, white surface.
		Surface();
		/// Copy constructor.
		Surface(const Surface & s);

		///
		/// Constructs a white surface with the given vertices.
		/// \param indices The indices of the surface's vertices.
		/// \param offset The amount to offset the values stored in indices.
		///
		Surface(const std::vector<int> & indices, int offset = 0);

		///
		/// Constructs an empty, colored surface.
		/// \param hasColor True if the surface has color.
		/// \param color The hexadecimal representation of the RGBA color.
		/// \param transparency Between 0 and 1.
		/// \param translucency Between 0 and 1.
		/// \param specular Between 0 and 1.
		///
		Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular);

		///
		/// Constructs a colored surface with the given vertices.
		/// \param hasColor True if the surface has color.
		/// \param color The hexadecimal representation of the RGBA color.
		/// \param transparency Between 0 and 1.
		/// \param translucency Between 0 and 1.
		/// \param specular Between 0 and 1.
		/// \param indices The indices of the surface's vertices.
		///
		Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular, const std::vector<int> & indices );

		void addTriangles(const std::vector<Triangle> &); ///< Adds triangles to the surface.
		void addTriangles(const Surface &); ///< Adds triangles from another surface
		void offsetIndices(int offset); ///< Adds an offset to all triangles' vertex indices

		///
		/// Sets the color of the surface.
		/// \param r Red.
		/// \param g Green.
		/// \param b Blue.
		/// \param a Alpha.
		///
		void setColor(unsigned char r, unsigned char g, unsigned char b, unsigned char a = 255);

		///
		/// Sets the color of the surface based on a material and appearance from Fusion.
		/// \param material Fusion material.
		/// \param appearance Fusion appearance.
		///
		void setColor(core::Ptr<core::Material>, core::Ptr<core::Appearance>);

		void removeColor(); ///< Sets hasColor to true, removing color from the surface.

	private:
		bool hasColor; ///< True if the value of color should be written to the BXDA file.

		unsigned int color; ///< The hexadecimal representation of the surface's color.

		float transparency; ///< Transparency of the surface. While this is currently written to the BXDA file, it seems to be unused, so it is not implemented in the exporter either.
		float translucency; ///< Translucency of the surface. While this is currently written to the BXDA file, it seems to be unused, so it is not implemented in the exporter either.
		float specular; ///< Specular value of the surface. Reading this value from Fusion materials is not yet supported.

		std::vector<Triangle> triangles; ///< The Triangles that make up the surface.

		void write(BinaryWriter &) const;

	};
}
