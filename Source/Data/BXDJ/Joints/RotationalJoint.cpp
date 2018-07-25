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

Vector3<float> RotationalJoint::getAxisOfRotation() const
{
	return Vector3<float>(0, 0, 0);
}

float RotationalJoint::getCurrentAngle() const
{
	return 0.0f;
}

float RotationalJoint::getUpperLimit() const
{
	return 0.0f;
}

float RotationalJoint::getLowerLimit() const
{
	return 0.0f;
}

void RotationalJoint::write(XmlWriter & output) const
{
	// Write joint information
	output.startElement("RotationalJoint");

	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "Axis");
	output.write(getAxisOfRotation());
	output.endElement();

	output.writeElement("CurrentAngularPosition", std::to_string(getCurrentAngle()));

	output.endElement();

	// Write driver information
	AngularJoint::Joint::write(output);
}
