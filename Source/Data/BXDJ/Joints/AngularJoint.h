#pragma once

#include "../../Vector3.h"

namespace BXDJ
{
	class AngularJoint
	{
	public:
		virtual Vector3<> getAxisOfRotation() const = 0;
		virtual float getCurrentAngle() const = 0;
		virtual bool hasLimits() const = 0;
		virtual float getMinAngle() const = 0;
		virtual float getMaxAngle() const = 0;
	};
}
