#pragma once

#include <Fusion/Components/CylindricalJointMotion.h>
#include "../Joint.h"

using namespace adsk;

namespace BXDJ
{
	class ConfigData;

	class CylindricalJoint : public Joint
	{
	public:
		CylindricalJoint(const CylindricalJoint &);
		CylindricalJoint(RigidNode *, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		Vector3<> getAxis() const;

		float getCurrentAngle() const;
		bool hasLimits() const;
		float getMinAngle() const;
		float getMaxAngle() const;

		float getCurrentTranslation() const;
		float getMinTranslation() const;
		float getMaxTranslation() const;

		void applyConfig(const ConfigData &);

	private:
		core::Ptr<fusion::CylindricalJointMotion> fusionJointMotion;

		void write(XmlWriter &) const;

	};
}
