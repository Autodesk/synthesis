#include "ConfigData.h"
#include <Fusion/FusionTypeDefs.h>
#include <Fusion/Components/JointMotion.h>
#include <Fusion/Components/Occurrence.h>
#include <Fusion/Components/AsBuiltJoint.h>
#include <rapidjson/document.h>
#include "Utility.h"
#include <string>

using namespace adsk;
using namespace BXDJ;

ConfigData::ConfigData()
{
	robotName = "unnamed";
	drivetrainType = TANK;
	weight = 10;
	tempIconDir = "";
	convexType = BOX;
}

ConfigData::ConfigData(const ConfigData & other)
{
	robotName = other.robotName;
	drivetrainType = other.drivetrainType;
	tempIconDir = other.tempIconDir;
	weight = other.weight;
	convexType = other.convexType;

	for (auto i = other.joints.begin(); i != other.joints.end(); i++)
		joints[i->first] = i->second;
}

void ConfigData::setDriveType(const std::string type) {
	if (type == "tank") {
		drivetrainType = TANK;
	} else if (type == "h-drive") {
		drivetrainType = H_DRIVE;
	} else if (type == "other") {
		drivetrainType = CUSTOM;
	} else {
		drivetrainType = TANK; // default
	}
}

std::unique_ptr<Driver> ConfigData::getDriver(core::Ptr<fusion::Joint> joint) const
{
	std::string id = BXDJ::Utility::getUniqueJointID(joint);

	if (joints.find(id) == joints.end() || joints.at(id).driver == nullptr)
		return nullptr;

	return std::make_unique<Driver>(*joints.at(id).driver);
}
std::unique_ptr<Driver> ConfigData::getDriver(core::Ptr<fusion::AsBuiltJoint> joint) const
{
	std::string id = BXDJ::Utility::getUniqueJointID(joint);

	if (joints.find(id) == joints.end() || joints.at(id).driver == nullptr)
		return nullptr;

	return std::make_unique<Driver>(*joints.at(id).driver);
}

void ConfigData::setDriver(core::Ptr<fusion::Joint> joint, const Driver & driver)
{
	std::string id = BXDJ::Utility::getUniqueJointID(joint);
	joints[id].name = joint->name();
	joints[id].asBuilt = false;
	joints[id].motion = internalJointMotion(joint->jointMotion()->jointType());
	joints[id].driver = std::make_unique<Driver>(driver);
}
void ConfigData::setDriver(core::Ptr<fusion::AsBuiltJoint> joint, const Driver & driver)
{
	std::string id = BXDJ::Utility::getUniqueJointID(joint);
	joints[id].name = joint->name();
	joints[id].asBuilt = true;
	joints[id].motion = internalJointMotion(joint->jointMotion()->jointType());
	joints[id].driver = std::make_unique<Driver>(driver);
}

void ConfigData::setNoDriver(core::Ptr<fusion::Joint> joint)
{
	std::string id = BXDJ::Utility::getUniqueJointID(joint);
	joints[id].name = joint->name();
	joints[id].asBuilt = false;
	joints[id].motion = internalJointMotion(joint->jointMotion()->jointType());
	joints[id].driver = nullptr;
}
void ConfigData::setNoDriver(core::Ptr<fusion::AsBuiltJoint> joint)
{
	std::string id = BXDJ::Utility::getUniqueJointID(joint);
	joints[id].name = joint->name();
	joints[id].asBuilt = true;
	joints[id].motion = internalJointMotion(joint->jointMotion()->jointType());
	joints[id].driver = nullptr;
}

std::vector<std::shared_ptr<JointSensor>> ConfigData::getSensors(core::Ptr<fusion::Joint> joint) const
{
	std::string id = Utility::getUniqueJointID(joint);

	if (joints.find(id) == joints.end() || joints.at(id).driver == nullptr)
		return std::vector<std::shared_ptr<JointSensor>>();

	return joints.at(id).sensors;
}
std::vector<std::shared_ptr<JointSensor>> ConfigData::getSensors(core::Ptr<fusion::AsBuiltJoint> joint) const
{
	std::string id = Utility::getUniqueJointID(joint);

	if (joints.find(id) == joints.end() || joints.at(id).driver == nullptr)
		return std::vector<std::shared_ptr<JointSensor>>();

	return joints.at(id).sensors;
}

void ConfigData::filterJoints(std::vector<core::Ptr<fusion::Joint>> filterJoints)
{
	// Make list of IDs in filter while adding joints not yet present
	std::vector<std::string> filterJointIDs;
	for (core::Ptr<fusion::Joint> joint : filterJoints)
	{
		std::string id = Utility::getUniqueJointID(joint);
		filterJointIDs.push_back(id);

		if (joints.find(id) == joints.end() || joints[id].motion != internalJointMotion(joint->jointMotion()->jointType()))
			setNoDriver(joint);
	}

	// Find all joints not present in filter
	std::vector<std::string> jointsToErase;
	for (auto i = joints.begin(); i != joints.end(); i++)
	{
		if (!i->second.asBuilt)
		{
			int j = 0;
			while (j < filterJointIDs.size() && filterJointIDs[j] != i->first) { j++; }

			if (j >= filterJointIDs.size())
				jointsToErase.push_back(i->first);
		}
	}

	// Erase joints
	for (std::string jointToErase : jointsToErase)
		joints.erase(jointToErase);
}
void ConfigData::filterJoints(std::vector<core::Ptr<fusion::AsBuiltJoint>> filterJoints)
{
	// Make list of IDs in filter while adding joints not yet present
	std::vector<std::string> filterJointIDs;
	for (core::Ptr<fusion::AsBuiltJoint> joint : filterJoints)
	{
		std::string id = Utility::getUniqueJointID(joint);
		filterJointIDs.push_back(id);

		if (joints.find(id) == joints.end() || joints[id].motion != internalJointMotion(joint->jointMotion()->jointType()))
			setNoDriver(joint);
	}

	// Find all joints not present in filter
	std::vector<std::string> jointsToErase;
	for (auto i = joints.begin(); i != joints.end(); i++)
	{
		if (i->second.asBuilt)
		{
			int j = 0;
			while (j < filterJointIDs.size() && filterJointIDs[j] != i->first) { j++; }

			if (j >= filterJointIDs.size())
				jointsToErase.push_back(i->first);
		}
	}

	// Erase joints
	for (std::string jointToErase : jointsToErase)
		joints.erase(jointToErase);
}

nlohmann::json ConfigData::getJSONObject() const
{
	nlohmann::json configJSON;
	configJSON["name"] = robotName;
	configJSON["driveTrainType"] = (int)drivetrainType;
	configJSON["convex"] = (int)convexType;


	// Weight
	configJSON.AddMember("weight", rapidjson::Value(weight), allocator);

	nlohmann::json weightJSON;	
	weightJSON["value"] = weight.value;
	weightJSON["unit"] = weight.type;

	configJSON["weight"] = weightJSON;


	configJSON["tempIconDir"] = tempIconDir;

	// Joints
	nlohmann::json jointsJson = nlohmann::json::array();
	

	for (auto i = joints.begin(); i != joints.end(); i++)
	{
		std::string jointID = i->first;
		JointConfig jointConfig = i->second;

		nlohmann::json jointJSON;
		
		// Joint Info
		jointJSON["id"] = jointID;
		jointJSON["name"] = jointConfig.name;
		jointJSON["asBuilt"] = jointConfig.asBuilt;
		jointJSON["type"] = (int)jointConfig.motion;
		
		// Driver Information
		nlohmann::json driverJSON;
		if (jointConfig.driver != nullptr)
			driverJSON = jointConfig.driver->getJSONObject();

		jointJSON["driver"] = driverJSON;
		

		// Sensor Information
		nlohmann::json sensorsJSON = nlohmann::json::array();

		for (std::shared_ptr<JointSensor> sensor : jointConfig.sensors)
			sensorsJSON.push_back(sensor->getJSONObject());

		jointJSON["sensors"] = sensorsJSON;

		// Add joint to JSON array
		jointsJson.push_back(jointJSON);
	}

	configJSON["joints"] = jointsJson;

	std::string jsonString = configJSON.dump();
	return configJSON;
}

void ConfigData::loadJSONObject(nlohmann::json configJson)
{
	// Get general information 
	std::string jsonString = configJson.dump();
	if (configJson["name"].is_string()) {
		robotName = configJson["name"].get<std::string>();
	}

	if (configJson["driveTrainType"].is_number_integer()) {
		drivetrainType = (DrivetrainType)configJson["driveTrainType"].get<int>();
	}

	if (configJson["convex"].is_number_integer()) {
		convexType = (ConvexType)configJson["convex"];
	}

	if (configJson["weight"].is_object()) {
		nlohmann::json weightJSON =  configJson["weight"];
		
		if (weightJSON["value"].is_number()) {
			weight.value = weightJSON["value"].get<double>();
		}

		if (weightJSON["unit"].is_number()) {
			weight.type = weightJSON["unit"].get<int>();
			weight.value /= 2.20462f;
		}
	}
	
	if (configJson["tempIconDir"].is_string()) {
		tempIconDir = configJson["tempIconDir"].get<std::string>();
	}

	// Read each joint configuration
	nlohmann::json jointsJSON = configJson["joints"];
	for (auto& joint : jointsJSON) {
		std::string jointID = joint["id"].get<std::string>();
		if (joint["name"].is_string()) {
			joints[jointID].name = joint["name"].get<std::string>();
		}
		if (joint["asBuilt"].is_boolean()) {
			joints[jointID].asBuilt = joint["asBuilt"].get<bool>();
		}
		if (joint["type"].is_number()) {
			joints[jointID].motion = (ConfigData::JointMotionType)joint["type"].get<int>();
		}


		if (joint["driver"].is_object()) {
			Driver driver;
			driver.loadJSONObject(joint["driver"]);
			joints[jointID].driver = std::make_unique<Driver>(driver);
		}
		else
			joints[jointID].driver = nullptr;

		if (joint["sensors"].is_array())
		{
			nlohmann::json sensorsJSONArray = joint["sensors"];
			for (auto& sensorJson : sensorsJSONArray) {
				if (sensorJson.is_object())
				{
					std::shared_ptr<JointSensor> sensor = std::make_shared<JointSensor>();
					sensor->loadJSONObject(sensorJson);
					joints[jointID].sensors.push_back(sensor);
				}
			}
		}
	}

	
	
}

std::string BXDJ::ConfigData::toString(DrivetrainType type)
{
	switch (type)
	{
	case TANK: return "TANK";
	case H_DRIVE: return "H_DRIVE";
	case CUSTOM: return "CUSTOM";
	}

	return "NONE";
}

std::map<std::string, ConfigData::JointConfig> ConfigData::getJoints()
{
	return joints;
}

ConfigData::JointMotionType ConfigData::internalJointMotion(fusion::JointTypes type)
{
	if (type == fusion::JointTypes::RevoluteJointType)
		return ANGULAR;

	else if (type == fusion::JointTypes::SliderJointType ||
		type == fusion::JointTypes::CylindricalJointType)
		return LINEAR;

	else if (false)
		return BOTH;

	else
		return NEITHER;
}
