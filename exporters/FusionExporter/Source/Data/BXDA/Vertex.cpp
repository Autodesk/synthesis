#include "Vertex.h"

using namespace BXDA;

Vertex::Vertex() : location(), normal()
{}

Vertex::Vertex(const Vertex & vertex) : location(vertex.location), normal(vertex.normal)
{}

Vertex::Vertex(Vector3<> location, Vector3<> normal)
{
	this->location = location;
	this->normal = normal;
}
