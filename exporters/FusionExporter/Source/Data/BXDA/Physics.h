#pragma once

#include <string>
#include "BinaryWriter.h"
#include "../Vector3.h"

namespace BXDA
{
	/// Stores the physics properties of a mesh, such as mass and center of mass.
	class Physics : public BinaryWritable
	{
	public:
		/// Constructs with zero mass and a COM at the origin.
		Physics();
		/// Copy constructor.
		Physics(const Physics &);

		///
		/// Constructs with the given COM and mass.
		/// \param centerOfMass The center of mass of the Physics properties' mesh.
		/// \param mass The mass of the Physics properties' mesh.
		///
		Physics(Vector3<float> centerOfMass, float mass);

		Physics operator+=(const Physics &); ///< Averages the center of mass of this instance with another's

		std::string toString(); ///< return A debug string for viewing physics properties.

	private:
		Vector3<float> centerOfMass; ///< The center of mass of the Mesh.
		float mass; ///< The total mass of the Mesh.

		void write(BinaryWriter &) const;

	};
}
