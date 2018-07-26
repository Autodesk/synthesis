#pragma once

#include "../../Vector3.h"
#include "../Joint.h"

namespace BXDJ
{
	class AngularJoint : public Joint
	{
	public:
		AngularJoint(const AngularJoint & j) : Joint(j) {}
		AngularJoint(RigidNode * p, core::Ptr<fusion::Joint> j, core::Ptr<fusion::Occurrence> o) : Joint(p, j, o) {}

		virtual Vector3<> getAxisOfRotation() const = 0;
		virtual float getCurrentAngle() const = 0;
		virtual bool hasLimits() const = 0;
		virtual float getMinAngle() const = 0;
		virtual float getMaxAngle() const = 0;
	};
}
