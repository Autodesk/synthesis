#include "RotationalJoint.h"
#include <Fusion/Components/JointLimits.h>
#include <Fusion/Components/AsBuiltJoint.h>
#include <Core/Geometry/Vector3D.h>
#include "../ConfigData.h"
#include "../Driver.h"
#include "../JointSensor.h"
#include "../Components/Wheel.h"

using namespace BXDJ;

RotationalJoint::RotationalJoint(const RotationalJoint & jointToCopy) : Joint(jointToCopy)
{
	fusionJointMotion = jointToCopy.fusionJointMotion;
}

RotationalJoint::RotationalJoint(RigidNode * parent, core::Ptr<fusion::Joint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, fusionJoint, parentOccurrence)
{
	this->fusionJointMotion = fusionJoint->jointMotion();
}

RotationalJoint::RotationalJoint(RigidNode * parent, core::Ptr<fusion::AsBuiltJoint> fusionJoint, core::Ptr<fusion::Occurrence> parentOccurrence) : Joint(parent, fusionJoint, parentOccurrence)
{
	this->fusionJointMotion = fusionJoint->jointMotion();
}

Vector3<> RotationalJoint::getAxisOfRotation() const
{
	core::Ptr<core::Vector3D> axis = fusionJointMotion->rotationAxisVector();
	return Vector3<>(axis->x(), axis->y(), axis->z());
}

float RotationalJoint::getCurrentAngle() const
{
	return (float)fusionJointMotion->rotationValue();
}

bool RotationalJoint::hasLimits() const
{
	return fusionJointMotion->rotationLimits()->isMinimumValueEnabled() && fusionJointMotion->rotationLimits()->isMaximumValueEnabled();
}

float RotationalJoint::getMinAngle() const
{
	if (fusionJointMotion->rotationLimits()->isMinimumValueEnabled())
		return (float)fusionJointMotion->rotationLimits()->minimumValue();
	else
		return 0;
}

float RotationalJoint::getMaxAngle() const
{
	if (fusionJointMotion->rotationLimits()->isMaximumValueEnabled())
		return (float)fusionJointMotion->rotationLimits()->maximumValue();
	else
		return 0;
}

void RotationalJoint::applyConfig(const ConfigData & config)
{
	// Update wheels with actual mesh information
	std::unique_ptr<Driver> driver = searchDriver(config);
	if (driver != nullptr)
	{
		if (driver->getWheel() != nullptr)
			driver->setComponent(Wheel(*driver->getWheel(), *this));

		setDriver(*driver);
	}

	// Add sensors
	std::vector<std::shared_ptr<JointSensor>> sensors = searchSensors(config);
	for (std::shared_ptr<JointSensor> sensor : sensors)
		if (sensor->type == JointSensor::ENCODER) // Filter out unsupported sensors
			addSensor(*sensor);
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
	Joint::write(output);
}


nlohmann::json RotationalJoint::GetJson() {
	nlohmann::json jointJson;
	jointJson["$type"] = "RotationalJoint, RobotExportAPI";
	jointJson["axis"] = getAxisOfRotation().GetJson();
	jointJson["basePoint"] = getChildBasePoint().GetJson();
	jointJson["currentAngularPosition"] = getCurrentAngle();
	jointJson["hasAngularLimit"] = hasLimits();

	// Limits
	if (hasLimits())
	{
		jointJson["angularLimitLow"] = getMinAngle();
		jointJson["angularLimitHigh"] = getMaxAngle();
	}
		
	jointJson["typeSave"] = "ROTATIONAL";
	jointJson["weight"] = getWeightData();


	nlohmann::json sensorJson = nlohmann::json::array();

	for (int i = 0; i < sensors.size(); i++) {
		sensorJson.push_back(sensors[i]->GetExportJSON());
	}

	jointJson["attachedSensors"] = sensorJson;
	jointJson["cDriver"] = getDriver()->GetExportJson();
	
	return jointJson;
}