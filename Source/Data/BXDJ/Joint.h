#pragma once

#include <Fusion/Components/JointLimits.h>
#include <vector>
#include <limits>
#include "RigidNode.h"
#include "Driver.h"

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
		enum OneTwo : bool { ONE = true, TWO = false };

		core::Ptr<fusion::Joint> getFusionJoint() { return fusionJoint; }
		OneTwo getParentOccNum() { return parentOcc; }

	private:
		core::Ptr<fusion::Joint> fusionJoint;
		OneTwo parentOcc;
		RigidNode * parent;
		std::shared_ptr<RigidNode> child;

		std::unique_ptr<Driver> driver;

	};
};
