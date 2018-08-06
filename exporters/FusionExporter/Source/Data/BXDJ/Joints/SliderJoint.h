#pragma once

#include <Fusion/Components/SliderJointMotion.h>
#include "../Joint.h"

using namespace adsk;

namespace BXDJ
{
	class SliderJoint : public Joint
	{
	public:
		SliderJoint(const SliderJoint &);
		SliderJoint(RigidNode *, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		Vector3<> getAxisOfTranslation() const;
		float getCurrentTranslation() const;
		float getMinTranslation() const;
		float getMaxTranslation() const;

		void applyConfig(const ConfigData &);

	private:
		core::Ptr<fusion::SliderJointMotion> fusionJointMotion;

		void write(XmlWriter &) const;

	};
}
