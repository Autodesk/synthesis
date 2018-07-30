#pragma once

#include "../../Vector3.h"
#include "../Joint.h"

namespace BXDJ
{
	class LinearJoint : public Joint
	{
	public:
		LinearJoint(const LinearJoint & j) : Joint(j) {}
		LinearJoint(RigidNode * p, core::Ptr<fusion::Joint> j, core::Ptr<fusion::Occurrence> o) : Joint(p, j, o) {}

		virtual Vector3<> getAxisOfTranslation() const = 0;
		virtual float getCurrentTranslation() const = 0;
		virtual float getMinTranslation() const = 0;
		virtual float getMaxTranslation() const = 0;
	};
}
