#pragma once

#include <Fusion/Components/RevoluteJointMotion.h>
#include "../Joint.h"

using namespace adsk;

namespace BXDJ
{
	class RotationalJoint : public Joint
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

	private:
		core::Ptr<fusion::RevoluteJointMotion> fusionJointMotion;

		void write(XmlWriter &) const;

	};
}
