#include "CylindricalJoint.h"
#include <Fusion/Components/JointLimits.h>
#include <Fusion/Components/AsBuiltJoint.h>
#include <Core/Geometry/Vector3D.h>
#include "../Driver.h"
#include "../ConfigData.h"

using namespace BXDJ;

CylindricalJoint::CylindricalJoint(const CylindricalJoint & jointToCopy) : Joint(jointToCopy)
{
	fusionJointMotion = jointToCopy.fusionJointMotion;
}

CylindricalJoint::CylindricalJoint(RigidNode * parent, core::Ptr<fusion::Joint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, fusionJoint, parentOccurrence)
{
	this->fusionJointMotion = fusionJoint->jointMotion();
}

CylindricalJoint::CylindricalJoint(RigidNode * parent, core::Ptr<fusion::AsBuiltJoint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, fusionJoint, parentOccurrence)
{
	this->fusionJointMotion = fusionJoint->jointMotion();
}

Vector3<> CylindricalJoint::getAxis() const
{
	core::Ptr<core::Vector3D> axis = fusionJointMotion->rotationAxisVector();
	return Vector3<>(axis->x(), axis->y(), axis->z());
}

float CylindricalJoint::getCurrentAngle() const
{
	return (float)fusionJointMotion->rotationValue();
}

bool CylindricalJoint::hasLimits() const
{
	return fusionJointMotion->rotationLimits()->isMinimumValueEnabled() || fusionJointMotion->rotationLimits()->isMaximumValueEnabled();
}

float CylindricalJoint::getMinAngle() const
{
	if (fusionJointMotion->rotationLimits()->isMinimumValueEnabled())
		return (float)fusionJointMotion->rotationLimits()->minimumValue();
	else
		return std::numeric_limits<float>::min();
}

float CylindricalJoint::getMaxAngle() const
{
	if (fusionJointMotion->rotationLimits()->isMaximumValueEnabled())
		return (float)fusionJointMotion->rotationLimits()->maximumValue();
	else
		return std::numeric_limits<float>::max();
}

float CylindricalJoint::getCurrentTranslation() const
{
	return (float)fusionJointMotion->slideValue();
}

float CylindricalJoint::getMinTranslation() const
{
	if (fusionJointMotion->slideLimits()->isMinimumValueEnabled())
		return (float)fusionJointMotion->slideLimits()->minimumValue();
	else
		return std::numeric_limits<float>::min();
}

float CylindricalJoint::getMaxTranslation() const
{
	if (fusionJointMotion->slideLimits()->isMaximumValueEnabled())
		return (float)fusionJointMotion->slideLimits()->maximumValue();
	else
		return std::numeric_limits<float>::max();
}

void CylindricalJoint::applyConfig(const ConfigData & config)
{
	// Update wheels with actual mesh information
	std::unique_ptr<Driver> driver = searchDriver(config);
	if (driver != nullptr)
	{
		setDriver(*driver);
	}

	// This joint does not currently support sensors
}

void CylindricalJoint::write(XmlWriter & output) const
{
	// Write joint information
	output.startElement("CylindricalJoint");

	// Base point
	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "BasePoint");
	output.write(getChildBasePoint());
	output.endElement();

	// Axis
	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "Axis");
	output.write(getAxis());
	output.endElement();

	// Limits
	output.writeElement("AngularLowLimit", std::to_string(getMinAngle()));
	output.writeElement("AngularHighLimit", std::to_string(getMaxAngle()));
	output.writeElement("LinearStartLimit", std::to_string(getMinTranslation()));
	output.writeElement("LinearEndLimit", std::to_string(getMaxTranslation()));

	// Current position and angle
	output.writeElement("CurrentLinearPosition", std::to_string(getCurrentTranslation()));
	output.writeElement("CurrentAngularPosition", std::to_string(getCurrentAngle()));

	output.endElement();

	// Write driver information
	Joint::write(output);
}
