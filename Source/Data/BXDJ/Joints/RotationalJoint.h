#pragma once

#include <Fusion/Components/RevoluteJointMotion.h>
#include "AngularJoint.h"

namespace BXDJ
{
	class RotationalJoint : public AngularJoint
	{
	public:
		RotationalJoint(const RigidNode &, RigidNode *, core::Ptr<fusion::RevoluteJointMotion>);
		RotationalJoint(const RotationalJoint &);

		Vector3<float> getAxisOfRotation() const;
		float getCurrentAngle() const;
		float getUpperLimit() const;
		float getLowerLimit() const;
		
		void write(XmlWriter &) const;

	private:
		core::Ptr<fusion::RevoluteJointMotion> fusionJoint;

	};
}
