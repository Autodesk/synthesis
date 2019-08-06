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

rapidjson::Value ConfigData::getJSONObject(rapidjson::MemoryPoolAllocator<>& allocator) const
{
	rapidjson::Value configJSON;
	configJSON.SetObject();

	// General Information
	configJSON.AddMember("name", rapidjson::Value(robotName.c_str(), robotName.length(), allocator), allocator);
	configJSON.AddMember("drivetrainType", rapidjson::Value((int)drivetrainType), allocator);
	std::string convex = "BOX";
	switch (convexType) {
	case BOX:
		convex = "BOX";
		configJSON.AddMember("convex", rapidjson::Value(convex.c_str(), convex.length(), allocator), allocator);
		break;
	case VHACD_LOW:
		convex = "VHACD_LOW";
		configJSON.AddMember("convex", rapidjson::Value(convex.c_str(), convex.length(), allocator), allocator);
		break;

	case VHACD_MID:
		convex = "VHACD_MID";
		configJSON.AddMember("convex", rapidjson::Value(convex.c_str(), convex.length(), allocator), allocator);
		break;

	case VHACD_HIGH:
		convex = "VHACD_HIGH";
		configJSON.AddMember("convex", rapidjson::Value(convex.c_str(), convex.length(), allocator), allocator);
		break;

	}


	// Weight
	configJSON.AddMember("weight", rapidjson::Value(weight), allocator);

	configJSON.AddMember("tempIconDir", rapidjson::Value(tempIconDir.c_str(), tempIconDir.length(), allocator), allocator);

	// Joints
	rapidjson::Value jointsJSON;
	jointsJSON.SetArray();

	for (auto i = joints.begin(); i != joints.end(); i++)
	{
		std::string jointID = i->first;
		JointConfig jointConfig = i->second;

		rapidjson::Value jointJSON;
		jointJSON.SetObject();

		// Joint Info
		jointJSON.AddMember("id", rapidjson::Value(jointID.c_str(), jointID.length(), allocator), allocator);
		jointJSON.AddMember("name", rapidjson::Value(jointConfig.name.c_str(), jointConfig.name.length(), allocator), allocator);
		jointJSON.AddMember("asBuilt", jointConfig.asBuilt, allocator);
		jointJSON.AddMember("type", rapidjson::Value((int)jointConfig.motion), allocator);



		// Driver Information
		rapidjson::Value driverJSON;
		if (jointConfig.driver != nullptr)
			driverJSON = jointConfig.driver->getJSONObject(allocator);

		jointJSON.AddMember("driver", driverJSON, allocator);

		// Sensor Information
		rapidjson::Value sensorsJSON;
		sensorsJSON.SetArray();

		for (std::shared_ptr<JointSensor> sensor : jointConfig.sensors)
			sensorsJSON.PushBack(sensor->getJSONObject(allocator), allocator);

		jointJSON.AddMember("sensors", sensorsJSON, allocator);

		// Add joint to JSON array
		jointsJSON.PushBack(jointJSON, allocator);
	}

	configJSON.AddMember("joints", jointsJSON, allocator);
	return configJSON;
}

void ConfigData::loadJSONObject(const rapidjson::Value& configJSON)
{
	// Get general information 
	if (configJSON.HasMember("name") && configJSON["name"].IsString())
		robotName = configJSON["name"].GetString();

	if (configJSON.HasMember("drivetrainType") && configJSON["drivetrainType"].IsNumber())
		drivetrainType = (DrivetrainType)configJSON["drivetrainType"].GetInt();

	if (configJSON.HasMember("convex") && configJSON["convex"].IsString()) {

		
		
		std::string con = configJSON["convex"].GetString();
		if (con == "BOX") {
			convexType = BOX;
		}

		if (con == "VHACD_LOW") {
			convexType = VHACD_LOW;
		}

		if (con == "VHACD_MID") {
			convexType = VHACD_MID;
		}

		if (con == "VHACD_HIGH") {
			convexType = VHACD_HIGH;
		}
	}

	if (configJSON.HasMember("weight")) {
		weight = configJSON["weight"].GetFloat();
	}

	if (configJSON.HasMember("tempIconDir") && configJSON["tempIconDir"].IsString())
		tempIconDir = configJSON["tempIconDir"].GetString();

	// Read each joint configuration
	if (configJSON.HasMember("joints") && configJSON["joints"].IsArray()) {
		auto jointsJSON = configJSON["joints"].GetArray();
		for (rapidjson::SizeType i = 0; i < jointsJSON.Size(); i++)
		{
			// Joint Info
			std::string jointID = jointsJSON[i]["id"].GetString();

			if (jointsJSON[i].HasMember("name") && jointsJSON[i]["name"].IsString())		joints[jointID].name = jointsJSON[i]["name"].GetString();
			if (jointsJSON[i].HasMember("asBuilt") && jointsJSON[i]["asBuilt"].IsBool())	joints[jointID].asBuilt = jointsJSON[i]["asBuilt"].GetBool();
			if (jointsJSON[i].HasMember("type") && jointsJSON[i]["type"].IsNumber())		joints[jointID].motion = (JointMotionType)jointsJSON[i]["type"].GetInt();

			// Driver Information
			if (jointsJSON[i].HasMember("driver") && jointsJSON[i]["driver"].IsObject())
			{
				Driver driver;
				driver.loadJSONObject(jointsJSON[i]["driver"]);
				joints[jointID].driver = std::make_unique<Driver>(driver);
			}
			else
				joints[jointID].driver = nullptr;

			// Sensor Information
			if (jointsJSON[i].HasMember("sensors") && jointsJSON[i]["sensors"].IsArray())
			{
				auto sensorsJSONArray = jointsJSON[i]["sensors"].GetArray();
				for (rapidjson::SizeType j = 0; j < sensorsJSONArray.Size(); j++)
				{
					if (sensorsJSONArray[j].IsObject())
					{
						std::shared_ptr<JointSensor> sensor = std::make_shared<JointSensor>();
						sensor->loadJSONObject(sensorsJSONArray[j]);
						joints[jointID].sensors.push_back(sensor);
					}
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
