#pragma once

#include <Fusion/Components/JointLimits.h>
#include <vector>
#include <limits>
#include "RigidNode.h"

namespace BXDJ
{	
	// Links RigidNodes together
	class Joint : public XmlWritable
	{
	public:
		Joint(const Joint &);
		Joint(RigidNode * parent, core::Ptr<fusion::Joint>, core::Ptr<fusion::Occurrence>);

		RigidNode * getParent();
		std::shared_ptr<RigidNode> getChild();
		Vector3<float> getParentBasePoint() const;
		Vector3<float> getChildBasePoint() const;

		virtual void write(XmlWriter &) const;

	protected:
		core::Ptr<fusion::Joint> getFusionJoint() { return fusionJoint; }
		bool isParentOccOne() { return parentIsOccOne; }

	private:
		core::Ptr<fusion::Joint> fusionJoint;
		bool parentIsOccOne; // True if fusionJoint->occurrenceOne() is the parent
		RigidNode * parent;
		std::shared_ptr<RigidNode> child;

	};
};
