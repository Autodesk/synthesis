#pragma once

#include <Fusion/Components/SliderJointMotion.h>
#include "LinearJoint.h"

using namespace adsk;

namespace BXDJ
{
	class SliderJoint : public Joint, public LinearJoint
	{
	public:
		SliderJoint(const SliderJoint &);
		SliderJoint(RigidNode *, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		Vector3<> getAxisOfTranslation() const;
		float getCurrentTranslation() const;
		float getMinTranslation() const;
		float getMaxTranslation() const;

		void applyConfig(const ConfigData &);
		void write(XmlWriter &) const;

	private:
		core::Ptr<fusion::SliderJointMotion> fusionJointMotion;

	};
}
