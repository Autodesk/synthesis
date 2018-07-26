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
		RotationalJoint(const RotationalJoint &);
		RotationalJoint(RigidNode *, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		Vector3<> getAxisOfRotation() const;
		float getCurrentAngle() const;
		bool hasLimits() const;
		float getMinAngle() const;
		float getMaxAngle() const;
		void applyConfig(const ConfigData &);

		void write(XmlWriter &) const;

	private:
		core::Ptr<fusion::RevoluteJointMotion> fusionJointMotion;

	};
}
