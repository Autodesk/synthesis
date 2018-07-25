#pragma once

#include "../../Vector3.h"
#include "../Joint.h"

namespace BXDJ
{
	class AngularJoint : public Joint
	{
	public:
		AngularJoint(const RigidNode & child, RigidNode * parent) : Joint(child, parent) {}

		virtual Vector3<float> getAxisOfRotation() const = 0;
		virtual float getCurrentAngle() const = 0;
		virtual float getUpperLimit() const = 0;
		virtual float getLowerLimit() const = 0;
	};
}
