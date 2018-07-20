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
		Physics(Vector3<float> centerOfMass, float mass);

		Physics operator+=(const Physics &); // Averages the center of mass of a physics class with another

		std::string toString();

	private:
		Vector3<float> centerOfMass;
		float mass;

		void write(BinaryWriter &) const;

	};
}