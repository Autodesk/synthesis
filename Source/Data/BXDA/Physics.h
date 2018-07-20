#pragma once

#include <string>
#include "BinaryWriter.h"
#include "Vector3.h"

namespace BXDA
{
	class Physics : public BinaryWritable
	{
	public:
		Physics();

		Physics(const Physics &);
		Physics(Vector3 centerOfMass, double mass);

		Physics operator+=(const Physics &); // Averages the center of mass of a physics class with another

		std::string toString();

	private:
		Vector3 centerOfMass;
		double mass;

		void write(BinaryWriter &) const;

	};
}