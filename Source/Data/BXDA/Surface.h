#pragma once

#include <vector>
#include "BinaryWriter.h"
#include "Triangle.h"

namespace BXDA
{
	class Surface : public BinaryWritable
	{
	public:
		Surface();
		~Surface();

		Surface(const Surface & s);
		Surface(const std::vector<int> & indices);
		Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular);
		Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular, const std::vector<int> & indices );

		void addTriangles(const std::vector<Triangle> &);
		void addTriangles(const Surface &); // Adds the triangles from another surface
		void offsetIndices(int offset); // Adds an offset to all triangles' vertex indices

	private:
		bool hasColor;

		// HEX color
		unsigned int color;

		// Shading of surface
		float transparency;
		float translucency;
		float specular;

		// Stores the indices used for each triangle
		std::vector<Triangle*> triangles;

		void write(BinaryWriter &) const;

	};
}