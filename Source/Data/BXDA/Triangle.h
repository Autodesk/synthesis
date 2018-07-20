#pragma once

#include "BinaryWriter.h"

namespace BXDA
{
	class Triangle : public BinaryWritable
	{
	public:
		unsigned int vertexIndices[3];

		Triangle();
		Triangle(const unsigned int vertexIndices[]);
		Triangle(const Triangle & triangle);
		Triangle(unsigned int vertexIndex0, unsigned int vertexIndex1, unsigned int vertexIndex2);

	private:
		void write(BinaryWriter &) const;

	};
}