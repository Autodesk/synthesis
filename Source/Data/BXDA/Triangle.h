#pragma once

#include <iostream>

namespace BXDA
{
	class Triangle
	{
	public:
		unsigned int vertexIndices[3];

		Triangle();
		Triangle(const unsigned int vertexIndices[]);
		Triangle(const Triangle * triangle);
		Triangle(unsigned int vertexIndex0, unsigned int vertexIndex1, unsigned int vertexIndex2);
		
		friend std::ostream& operator<<(std::ostream&, const Triangle&);
	};
}