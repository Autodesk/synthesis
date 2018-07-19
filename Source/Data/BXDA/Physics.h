#pragma once

#include "Vector3.h"

namespace BXDA
{
	class Physics
	{
	public:
		Physics();

		Physics(Physics*);
		Physics(Vector3 centerOfMass, double mass);

		Physics operator+=(const Physics &); // Averages the center of mass of a physics class with another
		friend std::ostream& operator<<(std::ostream&, const Physics&);

	private:
		Vector3 centerOfMass;
		double mass;
	};
}