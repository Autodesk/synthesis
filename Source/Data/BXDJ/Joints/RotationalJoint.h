#pragma once

#include <Fusion/Components/RevoluteJointMotion.h>
#include "AngularJoint.h"

namespace BXDJ
{
	class RotationalJoint : public AngularJoint
	{
	public:
		RotationalJoint(const RigidNode & child, core::Ptr<fusion::RevoluteJointMotion>);
		RotationalJoint(const RotationalJoint &);

		Vector3<float> getAxisOfRotation();
		float getCurrentAngle();
		float getUpperLimit();
		float getLowerLimit();

	private:
		core::Ptr<fusion::RevoluteJointMotion> fusionJoint;

	};
}
