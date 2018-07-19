#pragma once

#include <vector>
#include "Triangle.h"

namespace BXDA
{
	class Surface
	{
	public:
		Surface();
		~Surface();

		Surface(const Surface * s);
		Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular, const vector<int> & indices );
		
		friend std::ostream& operator<<(std::ostream&, const Surface&);

		void addTriangles(const vector<Triangle> &);
		void addTriangles(const Surface *); // Adds the triangles from another surface
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
		vector<Triangle*> triangles;
	};
}