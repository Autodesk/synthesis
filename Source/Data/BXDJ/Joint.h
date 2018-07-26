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

		RigidNode * getParent() const;
		std::shared_ptr<RigidNode> getChild() const;
		Vector3<> getParentBasePoint() const;
		Vector3<> getChildBasePoint() const;

		bool applyConfig(ConfigData);
		void setDriver(Driver);
		void removeDriver();
		std::unique_ptr<Driver> getDriver() const;

		virtual void write(XmlWriter &) const;

	protected:
		enum OneTwo : bool { ONE = true, TWO = false };

		core::Ptr<fusion::Joint> getFusionJoint() { return fusionJoint; }
		OneTwo getParentOccNum() { return parentOcc; }

	private:
		OneTwo parentOcc;
		core::Ptr<fusion::Joint> fusionJoint;
		RigidNode * parent;
		std::shared_ptr<RigidNode> child;
		std::unique_ptr<Driver> driver;

	};
};
