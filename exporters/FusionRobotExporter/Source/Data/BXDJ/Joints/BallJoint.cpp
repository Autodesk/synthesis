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
	/*
	nlohmann::json jointJson;
	jointJson["$type"] = "RotationalJoint, RobotExportAPI";
	jointJson["axis"] = getAxisOfRotation().GetJson();
	jointJson["basePoint"] = getChildBasePoint().GetJson();
	jointJson["currentAngularPosition"] = getCurrentAngle();
	jointJson["hasAngularLimit"] = hasLimits();

	double min = roundf(getMinAngle() * 100) / 100;
	if(!min){
		min = 0;
 	}

	double max = roundf(getMaxAngle() * 100) / 100;
	if (max == NULL) {
		max = 0;
	}

	jointJson["angularLimitLow"] =  min;
	jointJson["angularLimitHigh"] = max;
	jointJson["typeSave"] = "ROTATIONAL";
	jointJson["weight"] = getWeightData();


	jointJson["attachedSensors"] = nlohmann::json::array();
	jointJson["cDriver"] = getDriver()->GetExportJson();*/
	nlohmann::json jointJson;
	jointJson["$type"] = "BallJoint, RobotExportAPI";
	jointJson["basePoint"] = getParentBasePoint().GetJson();
	jointJson["attachedSensors"] = nlohmann::json::array();
	jointJson["cDrive"] = getDriver()->GetExportJson();
	jointJson["weight"] = getWeightData();
	jointJson["typeSave"] = "BALL";
	nlohmann::json sensorJson = nlohmann::json::array();

	for (int i = 0; i < sensors.size(); i++) {
		sensorJson.push_back(sensors[i]->GetExportJSON());
	}

	jointJson["attachedSensors"] = sensorJson;
	return jointJson;
}