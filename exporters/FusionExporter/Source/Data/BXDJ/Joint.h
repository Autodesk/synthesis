#pragma once

#include <Fusion/Components/Joint.h>
#include <Fusion/Components/Occurrence.h>
#include "XmlWriter.h"
#include "Driver.h"
#include "../Vector3.h"

using namespace adsk;

namespace BXDJ
{	
	class RigidNode;
	class ConfigData;

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

		virtual void applyConfig(const ConfigData &) = 0;
		void setDriver(Driver);
		void setNoDriver();
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
