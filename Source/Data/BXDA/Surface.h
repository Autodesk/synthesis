#pragma once

#include <vector>
#include <Core/Materials/ColorProperty.h>
#include <Core/Application/Color.h>
#include "BinaryWriter.h"
#include "Triangle.h"

using namespace adsk;

namespace BXDA
{
	class Surface : public BinaryWritable
	{
	public:
		Surface();

		Surface(const Surface & s);
		Surface(const std::vector<int> & indices, int offset = 0);
		Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular);
		Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular, const std::vector<int> & indices );

		void addTriangles(const std::vector<Triangle> &);
		void addTriangles(const Surface &); // Adds the triangles from another surface
		void offsetIndices(int offset); // Adds an offset to all triangles' vertex indices

		void setColor(unsigned char r, unsigned char g, unsigned char b, unsigned char a = 255);
		void setColor(core::Ptr<core::ColorProperty>);
		void removeColor();

	private:
		bool hasColor;

		// HEX color
		unsigned int color;

		// Shading of surface
		float transparency;
		float translucency;
		float specular;

		// Stores the indices used for each triangle
		std::vector<Triangle> triangles;

		void write(BinaryWriter &) const;

	};
}
