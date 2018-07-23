#pragma once

#include <Fusion/Components/RevoluteJointMotion.h>
#include "AngularJoint.h"

namespace BXDJ
{
	class RotationalJoint : public AngularJoint
	{
	public:
		//RotationalJoint(const RigidNode & child, const adsk::fusion::RevoluteJointMotion &);

	private:
		adsk::fusion::RevoluteJointMotion * fusionJoint;

	};
}
