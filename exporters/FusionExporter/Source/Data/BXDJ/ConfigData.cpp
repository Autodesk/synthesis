#include "ConfigData.h"
#include <Fusion/FusionTypeDefs.h>
#include <Fusion/Components/JointMotion.h>
#include <Fusion/Components/Occurrence.h>
#include <rapidjson/document.h>
#include "Utility.h"

using namespace adsk;
using namespace BXDJ;

ConfigData::ConfigData()
{
	robotName = "unnamed";
}

ConfigData::ConfigData(const ConfigData & other)
{
	robotName = other.robotName;

	for (auto i = other.joints.begin(); i != other.joints.end(); i++)
		joints[i->first] = i->second;
}

std::unique_ptr<Driver> ConfigData::getDriver(core::Ptr<fusion::Joint> joint) const
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
	joints[id].motion = internalJointMotion(joint->jointMotion()->jointType());
	joints[id].driver = std::make_unique<Driver>(driver);
}

void ConfigData::setNoDriver(core::Ptr<fusion::Joint> joint)
{
	std::string id = BXDJ::Utility::getUniqueJointID(joint);
	joints[id].name = joint->name();
	joints[id].motion = internalJointMotion(joint->jointMotion()->jointType());
	joints[id].driver = nullptr;
}

void ConfigData::filterJoints(std::vector<core::Ptr<fusion::Joint>> filterJoints)
{
	// Make list of IDs in filter while adding joints not yet present
	std::vector<std::string> filterJointIDs;
	for (core::Ptr<fusion::Joint> joint : filterJoints)
	{
		std::string id = Utility::getUniqueJointID(joint);
		filterJointIDs.push_back(id);

		if (joints.find(id) == joints.end())
			setNoDriver(joint);
	}

	// Find all joints not present in filter
	std::vector<std::string> jointsToErase;
	for (auto i = joints.begin(); i != joints.end(); i++)
	{
		int j = 0;
		while (j < filterJointIDs.size() && filterJointIDs[j] != i->first) { j++; }

		if (j >= filterJointIDs.size())
			jointsToErase.push_back(i->first);
	}

	// Erase joints
	for (std::string jointToErase : jointsToErase)
		joints.erase(jointToErase);
}

rapidjson::Value ConfigData::getJSONObject(rapidjson::MemoryPoolAllocator<>& allocator) const
{
	rapidjson::Value configJSON;
	configJSON.SetObject();

	// Robot Name
	configJSON.AddMember("name", rapidjson::Value(robotName.c_str(), robotName.length(), allocator), allocator);

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
		jointJSON.AddMember("type", rapidjson::Value((int)jointConfig.motion), allocator);

		// Driver Information
		rapidjson::Value driverJSON;
		if (jointConfig.driver != nullptr)
			driverJSON = jointConfig.driver->getJSONObject(allocator);

		jointJSON.AddMember("driver", driverJSON, allocator);

		// Add joint to JSON array
		jointsJSON.PushBack(jointJSON, allocator);
	}

	configJSON.AddMember("joints", jointsJSON, allocator);
	return configJSON;
}

void ConfigData::loadJSONObject(const rapidjson::Value& configJSON)
{
	// Get robot name
	robotName = configJSON["name"].GetString();

	// Read each joint configuration
	auto jointsJSON = configJSON["joints"].GetArray();
	for (rapidjson::SizeType i = 0; i < jointsJSON.Size(); i++)
	{
		std::string jointID = jointsJSON[i]["id"].GetString();

		joints[jointID].name = jointsJSON[i]["name"].GetString();
		joints[jointID].motion = (JointMotionType)jointsJSON[i]["type"].GetInt();

		const rapidjson::Value& driverJSON = jointsJSON[i]["driver"];
		if (driverJSON.IsObject())
		{
			Driver driver;
			driver.loadJSONObject(driverJSON);
			joints[jointID].driver = std::make_unique<Driver>(driver);
		}
		else
			joints[jointID].driver = nullptr;
	}
}

ConfigData::JointMotionType ConfigData::internalJointMotion(fusion::JointTypes type) const
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
