#pragma once

#include <Fusion/Components/RevoluteJointMotion.h>
#include <Core/Geometry/Vector3D.h>
#include "AngularJoint.h"

using namespace adsk;

namespace BXDJ
{
	class RotationalJoint : public AngularJoint
	{
	public:
		RotationalJoint(const RigidNode &, RigidNode *, core::Ptr<fusion::RevoluteJointMotion>);
		RotationalJoint(const RotationalJoint &);

		Vector3<float> getBasePoint() const;
		Vector3<float> getAxisOfRotation() const;
		float getCurrentAngle() const;
		bool hasLimits() const;
		float getMinAngle() const;
		float getMaxAngle() const;
		
		void write(XmlWriter &) const;

	private:
		core::Ptr<fusion::RevoluteJointMotion> fusionJoint;

	};
}
