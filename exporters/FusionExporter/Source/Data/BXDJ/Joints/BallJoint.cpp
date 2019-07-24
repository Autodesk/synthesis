#include "BallJoint.h"
#include <Fusion/Components/AsBuiltJoint.h>
#include <Core/Geometry/Vector3D.h>
#include "../ConfigData.h"
#include "../Driver.h"

using namespace BXDJ;

BallJoint::BallJoint(const BallJoint & jointToCopy) : Joint(jointToCopy)
{
	fusionJointMotion = jointToCopy.fusionJointMotion;
}

BallJoint::BallJoint(RigidNode * parent, core::Ptr<fusion::Joint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, fusionJoint, parentOccurrence)
{
	this->fusionJointMotion = fusionJoint->jointMotion();
}

BallJoint::BallJoint(RigidNode * parent, core::Ptr<fusion::AsBuiltJoint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, fusionJoint, parentOccurrence)
{
	this->fusionJointMotion = fusionJoint->jointMotion();
}

void BallJoint::applyConfig(const ConfigData & config)
{
	// Update wheels with actual mesh information
	std::unique_ptr<Driver> driver = searchDriver(config);
	if (driver != nullptr)
	{
		setDriver(*driver);
	}

	// Ball joints do not support sensors
}

void BallJoint::write(XmlWriter & output) const
{
	// Write joint information
	output.startElement("BallJoint");

	// Base point
	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "BasePoint");
	output.write(getChildBasePoint());
	output.endElement();

	output.endElement();

	// Write driver information
	// Joint::write(output); // No drivers are compatible with ball joint
}


nlohmann::json BallJoint::GetJson() {
	nlohmann::json jointJson;

	return jointJson;
}