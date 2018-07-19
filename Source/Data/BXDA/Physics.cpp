#include "Physics.h"

using namespace BXDA;

Physics::Physics() : centerOfMass(), mass(0)
{}

Physics::Physics(Physics * physicsToCopy)
{
	centerOfMass = physicsToCopy->centerOfMass;
	mass = physicsToCopy->mass;
}

Physics::Physics(Vector3 centerOfMass, double mass)
{
	this->centerOfMass = centerOfMass;
	this->mass = mass;
}

Physics Physics::operator+=(const Physics & physicsToAverage)
{
	centerOfMass = (centerOfMass * mass + physicsToAverage.centerOfMass * physicsToAverage.mass) / (mass + physicsToAverage.mass);
	return this;
}