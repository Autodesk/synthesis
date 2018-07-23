#pragma once

#include "../Vector3.h"

namespace BXDA
{
	class Vertex
	{
	public:
		Vector3<> location;

		Vector3<> normal;

		Vertex();
		Vertex(const Vertex &);
		Vertex(Vector3<> location, Vector3<> normal);
	};
}
