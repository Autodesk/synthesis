#pragma once

#include "../Vector3.h"

namespace BXDA
{
	/// A point with a normal vector located in Fusion space.
	class Vertex
	{
	public:
		Vector3<> location; ///< Location of the vertex in Fusion space.

		Vector3<> normal; ///< The outward direction of a point on a surface at which this vertex is located.

		/// Constructs a vertex at (0, 0, 0) with a normal of (0, 0, 0).
		Vertex();
		/// Copy constructor.
		Vertex(const Vertex &);
		
		///
		/// Construct a vertex at a specific point with a specific normal direction.
		/// \param location Location of the vertex.
		/// \param normal Normal direction of the vertex.
		///
		Vertex(Vector3<> location, Vector3<> normal);
	};
}
