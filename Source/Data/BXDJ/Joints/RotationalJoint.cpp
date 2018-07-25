#include "RotationalJoint.h"

using namespace BXDJ;

RotationalJoint::RotationalJoint(const RigidNode & child, RigidNode * parent, core::Ptr<fusion::RevoluteJointMotion> fusionJoint) : AngularJoint(child, parent)
{
	this->fusionJoint = fusionJoint;
}

RotationalJoint::RotationalJoint(const RotationalJoint & jointToCopy) : AngularJoint(jointToCopy)
{
	fusionJoint = jointToCopy.fusionJoint;
}

Vector3<float> BXDJ::RotationalJoint::getBasePoint() const
{
	return Vector3<float>(0, 0, 0);
}

Vector3<float> RotationalJoint::getAxisOfRotation() const
{
	return Vector3<float>(0, 0, 0);
}

float RotationalJoint::getCurrentAngle() const
{
	return 0.0f;
}

bool BXDJ::RotationalJoint::hasLimits() const
{
	return false;
}

float RotationalJoint::getMaxAngle() const
{
	return 0.0f;
}

float RotationalJoint::getMinAngle() const
{
	return 0.0f;
}

void RotationalJoint::write(XmlWriter & output) const
{
	// Write joint information
	output.startElement("RotationalJoint");

	// Base point
	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "BasePoint");
	output.write(getBasePoint());
	output.endElement();

	// Axis
	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "Axis");
	output.write(getAxisOfRotation());
	output.endElement();

	// Limits
	if (hasLimits())
	{
		output.writeElement("AngularLowLimit", std::to_string(getMinAngle()));
		output.writeElement("AngularHighLimit", std::to_string(getMaxAngle()));
	}

	// Current angle
	output.writeElement("CurrentAngularPosition", std::to_string(getCurrentAngle()));

	output.endElement();

	// Write driver information
	AngularJoint::Joint::write(output);
}
