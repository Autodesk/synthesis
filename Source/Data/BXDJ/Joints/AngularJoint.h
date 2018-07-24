#pragma once

#include "../../Vector3.h"
#include "../Joint.h"

namespace BXDJ
{
	class AngularJoint : public Joint
	{
	public:
		AngularJoint(const RigidNode & child) : Joint(child) {}

		virtual Vector3<float> getAxisOfRotation() = 0;
		virtual float getCurrentAngle() = 0;
		virtual float getUpperLimit() = 0;
		virtual float getLowerLimit() = 0;
	};
}
