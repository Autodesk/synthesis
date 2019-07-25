#include "Physics.h"

using namespace BXDA;

Physics::Physics() : centerOfMass(), mass(0)
{}

Physics::Physics(const Physics & physicsToCopy)
{
	centerOfMass = physicsToCopy.centerOfMass;
	mass = physicsToCopy.mass;
}

Physics::Physics(Vector3<float> centerOfMass, float mass)
{
	this->centerOfMass = centerOfMass;
	this->mass = mass;
}

Physics Physics::operator+=(const Physics & physicsToAverage)
{
	centerOfMass = (centerOfMass * mass + physicsToAverage.centerOfMass * physicsToAverage.mass) / (mass + physicsToAverage.mass);
	mass += physicsToAverage.mass;
	return *this;
}

std::string Physics::toString()
{
	return "Center of Mass: " + centerOfMass.toString() + ", Mass: " + std::to_string(mass);
}

void Physics::write(BinaryWriter & output) const
{
	output.write(centerOfMass);
	output.write(mass);
}
