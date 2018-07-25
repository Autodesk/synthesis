#include "RotationalJoint.h"

using namespace BXDJ;

RotationalJoint::RotationalJoint(const RotationalJoint & jointToCopy) : AngularJoint(jointToCopy)
{
	fusionJointMotion = jointToCopy.fusionJointMotion;
}

RotationalJoint::RotationalJoint(RigidNode * parent, core::Ptr<fusion::Joint> joint, core::Ptr<fusion::Occurrence> parentOccurrence) : AngularJoint(parent, joint, parentOccurrence)
{
	this->fusionJointMotion = this->getFusionJoint()->jointMotion();

	driver = std::make_unique<Driver>(this, Driver::MOTOR);
	driver->portA = 0;
}

Vector3<float> RotationalJoint::getAxisOfRotation() const
{
	core::Ptr<core::Vector3D> axis = fusionJointMotion->rotationAxisVector();
	return Vector3<float>((float)axis->x(), (float)axis->y(), (float)axis->z());
}

float RotationalJoint::getCurrentAngle() const
{
	return (float)fusionJointMotion->rotationValue();
}

bool BXDJ::RotationalJoint::hasLimits() const
{
	return fusionJointMotion->rotationLimits()->isMinimumValueEnabled() || fusionJointMotion->rotationLimits()->isMaximumValueEnabled();
}

float RotationalJoint::getMinAngle() const
{
	if (fusionJointMotion->rotationLimits()->isMinimumValueEnabled())
		return (float)fusionJointMotion->rotationLimits()->minimumValue();
	else
		return std::numeric_limits<float>::min();
}

float RotationalJoint::getMaxAngle() const
{
	if (fusionJointMotion->rotationLimits()->isMaximumValueEnabled())
		return (float)fusionJointMotion->rotationLimits()->maximumValue();
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
	output.write(getChildBasePoint());
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
