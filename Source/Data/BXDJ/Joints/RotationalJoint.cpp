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
	core::Ptr<core::Vector3D> axis = fusionJoint->rotationAxisVector();
	return Vector3<float>((float)axis->x(), (float)axis->y(), (float)axis->z());
}

float RotationalJoint::getCurrentAngle() const
{
	return (float)fusionJoint->rotationValue();
}

bool BXDJ::RotationalJoint::hasLimits() const
{
	return fusionJoint->rotationLimits()->isMinimumValueEnabled() || fusionJoint->rotationLimits()->isMaximumValueEnabled();
}

float RotationalJoint::getMinAngle() const
{
	if (fusionJoint->rotationLimits()->isMinimumValueEnabled())
		return (float)fusionJoint->rotationLimits()->minimumValue();
	else
		return std::numeric_limits<float>::min();
}

float RotationalJoint::getMaxAngle() const
{
	if (fusionJoint->rotationLimits()->isMaximumValueEnabled())
		return (float)fusionJoint->rotationLimits()->maximumValue();
	else
		return std::numeric_limits<float>::max();
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
