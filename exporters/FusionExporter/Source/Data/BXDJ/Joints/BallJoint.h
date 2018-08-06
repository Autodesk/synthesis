#pragma once

#include <Fusion/Components/BallJointMotion.h>
#include "../Joint.h"

using namespace adsk;

namespace BXDJ
{
	class ConfigData;

	class BallJoint : public Joint
	{
	public:
		BallJoint(const BallJoint &);
		BallJoint(RigidNode *, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		void applyConfig(const ConfigData &);

	private:
		core::Ptr<fusion::BallJointMotion> fusionJointMotion;

		void write(XmlWriter &) const;

	};
}
