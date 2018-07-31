#include "BallJoint.h"
#include <Core/Geometry/Vector3D.h>

using namespace BXDJ;

BallJoint::BallJoint(const BallJoint & jointToCopy) : Joint(jointToCopy)
{
	fusionJointMotion = jointToCopy.fusionJointMotion;
}

BallJoint::BallJoint(RigidNode * parent, core::Ptr<fusion::Joint> joint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, joint, parentOccurrence)
{
	this->fusionJointMotion = this->getFusionJoint()->jointMotion();
}

void BallJoint::applyConfig(const ConfigData & config)
{
	// Update wheels with actual mesh information
	std::unique_ptr<Driver> driver = config.getDriver(getFusionJoint());
	if (driver != nullptr)
	{
		setDriver(*driver);
	}
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
