#include "ConfigData.h"
#include <rapidjson/document.h>
#include <rapidjson/writer.h>
#include <rapidjson/stringbuffer.h>
#include <Fusion/FusionTypeDefs.h>
#include <Fusion/Components/JointMotion.h>
#include <Fusion/Components/Occurrence.h>
#include "Utility.h"

using namespace adsk;
using namespace BXDJ;

ConfigData::ConfigData()
{
	robotName = "unnamed";
}

ConfigData::ConfigData(std::string configStr)
{
	loadFromJSON(configStr);
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

void ConfigData::loadFromJSON(std::string configStr)
{
	rapidjson::Document configJSON;
	configJSON.Parse(configStr.c_str());

	// Get robot name
	robotName = configJSON["name"].GetString();

	const rapidjson::Value& jointConfig = configJSON["joints"].GetArray();

	// Read each joint configuration
	for (rapidjson::SizeType i = 0; i < jointConfig.Size(); i++)
	{
		std::string jointID = jointConfig[i]["id"].GetString();

		joints[jointID].name = jointConfig[i]["name"].GetString();
		joints[jointID].motion = (JointMotionType)jointConfig[i]["type"].GetInt();

		const rapidjson::Value& driverJSON = jointConfig[i]["driver"];

		if (driverJSON.IsObject())
		{
			Driver driver;

			// Driver Properties
			driver.type = (Driver::Type)driverJSON["type"].GetInt();
			driver.portSignal = (Driver::Signal)driverJSON["signal"].GetInt();
			driver.portA = driverJSON["portA"].GetInt();
			driver.portB = driverJSON["portB"].GetInt();

			// Wheel Properties
			const rapidjson::Value& wheelJSON = driverJSON["wheel"];
			if (wheelJSON.IsObject())
			{
				Wheel wheel;

				wheel.type = (Wheel::Type)wheelJSON["type"].GetInt();
				wheel.frictionLevel = (Wheel::FrictionLevel)wheelJSON["frictionLevel"].GetInt();
				wheel.isDriveWheel = wheelJSON["isDriveWheel"].GetBool();

				driver.setComponent(wheel);
			}

			// Pneumatic Properties
			const rapidjson::Value& pneumaticJSON = driverJSON["pneumatic"];
			if (pneumaticJSON.IsObject())
			{
				Pneumatic pneumatic;

				pneumatic.widthMillimeter = Pneumatic::COMMON_WIDTHS[wheelJSON["width"].GetInt()];
				pneumatic.pressurePSI = Pneumatic::COMMON_PRESSURES[wheelJSON["pressure"].GetInt()];

				driver.setComponent(pneumatic);
			}

			joints[jointID].driver = std::make_unique<Driver>(driver);
		}
		else
		{
			joints[jointID].driver = nullptr;
		}
	}
}

std::string ConfigData::toString() const
{
	// Create JSON object containing all joint info
	rapidjson::Document configJSON;
	configJSON.SetObject();

	// Robot Name
	configJSON.AddMember("name", rapidjson::Value(robotName.c_str(), robotName.length(), configJSON.GetAllocator()), configJSON.GetAllocator());

	// Joints
	rapidjson::Value jointsJSON;
	jointsJSON.SetArray();

	for (auto i = joints.begin(); i != joints.end(); i++)
	{
		std::string jointID = i->first;
		JointConfig jointConfig = i->second;

		rapidjson::Value jointJSON;
		jointJSON.SetObject();

		// Joint ID
		std::string jointName = jointID;
		jointJSON.AddMember("id", rapidjson::Value(jointName.c_str(), jointName.length(), configJSON.GetAllocator()), configJSON.GetAllocator());

		// Joint Name
		jointJSON.AddMember("name", rapidjson::Value(jointConfig.name.c_str(), jointConfig.name.length(), configJSON.GetAllocator()), configJSON.GetAllocator());

		// Joint Motion (linear and/or angular)
		jointJSON.AddMember("type", rapidjson::Value((int)jointConfig.motion), configJSON.GetAllocator());

		// Driver Information
		rapidjson::Value driverJSON;

		if (jointConfig.driver != nullptr)
		{
			driverJSON.SetObject();
			Driver driver(*jointConfig.driver);

			driverJSON.AddMember("type", rapidjson::Value((int)driver.type), configJSON.GetAllocator());
			driverJSON.AddMember("signal", rapidjson::Value((int)driver.portSignal), configJSON.GetAllocator());
			driverJSON.AddMember("portA", rapidjson::Value((int)driver.portA), configJSON.GetAllocator());
			driverJSON.AddMember("portB", rapidjson::Value((int)driver.portB), configJSON.GetAllocator());

			// Wheel Information
			rapidjson::Value wheelJSON;
			std::unique_ptr<Wheel> wheel = driver.getWheel();

			if (wheel != nullptr)
			{
				wheelJSON.SetObject();

				driverJSON.AddMember("type", rapidjson::Value((int)wheel->type), configJSON.GetAllocator());
				driverJSON.AddMember("frictionLevel", rapidjson::Value((int)wheel->frictionLevel), configJSON.GetAllocator());
				driverJSON.AddMember("isDriveWheel", rapidjson::Value(wheel->isDriveWheel), configJSON.GetAllocator());
			}

			driverJSON.AddMember("wheel", wheelJSON, configJSON.GetAllocator());
			
			// Pneumatic Information
			rapidjson::Value pneumaticJSON;
			std::unique_ptr<Pneumatic> pneumatic = driver.getPneumatic();

			if (pneumatic != nullptr)
			{
				pneumaticJSON.SetObject();

				driverJSON.AddMember("width", rapidjson::Value(pneumatic->getCommonWidth()), configJSON.GetAllocator());
				driverJSON.AddMember("pressure", rapidjson::Value(pneumatic->getCommonPressure()), configJSON.GetAllocator());
			}

			driverJSON.AddMember("pneumatic", pneumaticJSON, configJSON.GetAllocator());
		}

		jointJSON.AddMember("driver", driverJSON, configJSON.GetAllocator());

		// Add joint to JSON array
		jointsJSON.PushBack(jointJSON, configJSON.GetAllocator());
	}

	configJSON.AddMember("joints", jointsJSON, configJSON.GetAllocator());

	// Copy JSON to string
	rapidjson::StringBuffer buffer;
	rapidjson::Writer<rapidjson::StringBuffer> writer(buffer);
	configJSON.Accept(writer);

	std::string jsonString(buffer.GetString());
	return jsonString;
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
