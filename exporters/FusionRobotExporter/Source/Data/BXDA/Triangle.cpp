#include "Triangle.h"

using namespace BXDA;

Triangle::Triangle() : Triangle(0, 0, 0)
{}

Triangle::Triangle(const Triangle & triangle) : Triangle(triangle.vertexIndices[0], triangle.vertexIndices[1], triangle.vertexIndices[2])
{}

Triangle::Triangle(const unsigned int vertexIndices[]) : Triangle(vertexIndices[0], vertexIndices[1], vertexIndices[2])
{}

Triangle::Triangle(unsigned int vertexIndex0, unsigned int vertexIndex1, unsigned int vertexIndex2)
{
	vertexIndices[0] = vertexIndex0;
	vertexIndices[1] = vertexIndex1;
	vertexIndices[2] = vertexIndex2;
}

void BXDA::Triangle::write(BinaryWriter & output) const
{
	output.write((int)vertexIndices[0]);
	output.write((int)vertexIndices[1]);
	output.write((int)vertexIndices[2]);
}
