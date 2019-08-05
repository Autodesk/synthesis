#include "SliderJoint.h"
#include <Fusion/Components/JointLimits.h>
#include <Fusion/Components/AsBuiltJoint.h>
#include <Core/Geometry/Vector3D.h>
#include "../ConfigData.h"
#include "../Driver.h"
#include "../JointSensor.h"

using namespace BXDJ;

SliderJoint::SliderJoint(const SliderJoint & jointToCopy) : Joint(jointToCopy)
{
	fusionJointMotion = jointToCopy.fusionJointMotion;
}

SliderJoint::SliderJoint(RigidNode * parent, core::Ptr<fusion::Joint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, fusionJoint, parentOccurrence)
{
	this->fusionJointMotion = fusionJoint->jointMotion();
}

SliderJoint::SliderJoint(RigidNode * parent, core::Ptr<fusion::AsBuiltJoint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, fusionJoint, parentOccurrence)
{
	this->fusionJointMotion = fusionJoint->jointMotion();
}

Vector3<> SliderJoint::getAxisOfTranslation() const
{
	core::Ptr<core::Vector3D> axis = fusionJointMotion->slideDirectionVector();
	return Vector3<>(axis->x(), axis->y(), axis->z());
}

float SliderJoint::getCurrentTranslation() const
{
	return (float)fusionJointMotion->slideValue();
}

float SliderJoint::getMinTranslation() const
{
	if (fusionJointMotion->slideLimits()->isMinimumValueEnabled())
		return (float)fusionJointMotion->slideLimits()->minimumValue();
	else
		return -999;
}

float SliderJoint::getMaxTranslation() const
{
	if (fusionJointMotion->slideLimits()->isMaximumValueEnabled())
		return (float)fusionJointMotion->slideLimits()->maximumValue();
	else
		return 999;
}

void SliderJoint::applyConfig(const ConfigData & config)
{
	// Update wheels with actual mesh information
	std::unique_ptr<Driver> driver = searchDriver(config);
	if (driver != nullptr)
	{
		setDriver(*driver);
	}

	// Add sensors
	std::vector<std::shared_ptr<JointSensor>> sensors = searchSensors(config);
	for (std::shared_ptr<JointSensor> sensor : sensors)
		if (false) // Filter out unsupported sensors (no limit sensors are implemented)
			addSensor(*sensor);
}

void SliderJoint::write(XmlWriter & output) const
{
	// Write joint information
	output.startElement("LinearJoint");

	// Base point
	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "BasePoint");
	output.write(getChildBasePoint());
	output.endElement();

	// Axis
	output.startElement("BXDVector3");
	output.writeAttribute("VectorID", "Axis");
	output.write(getAxisOfTranslation());
	output.endElement();

	// Limits
	output.writeElement("LinearLowLimit", std::to_string(getMinTranslation()));
	output.writeElement("LinearUpperLimit", std::to_string(getMaxTranslation()));

	// Current angle
	output.writeElement("CurrentLinearPosition", std::to_string(getCurrentTranslation()));

	output.endElement();

	// Write driver information
	Joint::write(output);
}
