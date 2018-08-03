#include "ConfigData.h"
#include <rapidjson/document.h>
#include <rapidjson/writer.h>
#include <rapidjson/stringbuffer.h>
#include <Fusion/FusionTypeDefs.h>
#include <Fusion/Components/JointMotion.h>

using namespace adsk;
using namespace BXDJ;

ConfigData::ConfigData()
{
	robotName = "unnamed";
}

ConfigData::ConfigData(const std::vector<core::Ptr<fusion::Joint>> & joints) : ConfigData()
{
	setNoDrivers(joints);
}

ConfigData::ConfigData(std::string configStr, const std::vector<core::Ptr<fusion::Joint>> & joints)
{
	rapidjson::Document configJSON;
	configJSON.Parse(configStr.c_str());

	// Get robot name
	robotName = configJSON["name"].GetString();

	const rapidjson::Value& jointConfig = configJSON["joints"].GetArray();

	// Read each joint configuration
	for (rapidjson::SizeType i = 0; i < jointConfig.Size(); i++)
	{
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

			setDriver(joints[i], driver);
		}
		else
		{
			setNoDriver(joints[i]);
		}
	}
}

ConfigData::ConfigData(const ConfigData & other)
{
	robotName = other.robotName;

	for (auto i = other.joints.begin(); i != other.joints.end(); i++)
		if (i->second != nullptr)
			joints[i->first] = std::make_unique<Driver>(*i->second);
		else
			joints[i->first] = nullptr;
}

std::unique_ptr<Driver> ConfigData::getDriver(core::Ptr<fusion::Joint> joint) const
{
	if (joints.find(joint) == joints.end() || joints.at(joint) == nullptr)
		return nullptr;

	return std::make_unique<Driver>(*joints.at(joint));
}

void ConfigData::setDriver(core::Ptr<fusion::Joint> joint, Driver driver)
{
	joints[joint] = std::make_unique<Driver>(driver);
}

void BXDJ::ConfigData::setNoDriver(core::Ptr<fusion::Joint> joint)
{
	joints[joint] = nullptr;
}

void BXDJ::ConfigData::setNoDrivers(const std::vector<core::Ptr<fusion::Joint>>& newJoints)
{
	for (core::Ptr<fusion::Joint> joint : newJoints)
		setNoDriver(joint);
}

std::string ConfigData::toString()
{
	// Create JSON object containing all joint info
	rapidjson::Document configJSON;
	configJSON.SetObject();

	// Robot Name
	rapidjson::Value name;
	name.SetString(robotName.c_str(), robotName.length(), configJSON.GetAllocator());
	configJSON.AddMember("name", name, configJSON.GetAllocator());

	// Joints
	rapidjson::Value jointsJSON;
	jointsJSON.SetArray();

	for (auto i = joints.begin(); i != joints.end(); i++)
	{
		core::Ptr<fusion::Joint> joint = i->first;

		rapidjson::Value jointJSON;
		jointJSON.SetObject();

		// Joint Name
		jointJSON.AddMember("name", rapidjson::Value(joint->name().c_str(), joint->name().length()), configJSON.GetAllocator());

		// Joint Motion (linear and/or angular)
		fusion::JointTypes type = joint->jointMotion()->jointType();
		rapidjson::Value motionType;

		if (type == fusion::JointTypes::RevoluteJointType)
			motionType.SetUint(ANGULAR);

		else if (type == fusion::JointTypes::SliderJointType ||
			type == fusion::JointTypes::CylindricalJointType)
			motionType.SetUint(LINEAR);

		else if (false)
			motionType.SetUint(BOTH);

		else
			motionType.SetUint(NEITHER);

		jointJSON.AddMember("type", motionType, configJSON.GetAllocator());

		// Driver Information
		rapidjson::Value driverJSON;

		if (i->second == nullptr)
			driverJSON.SetNull();
		else
		{
			driverJSON.SetObject();
			Driver driver(*i->second);

			driverJSON.AddMember("type", rapidjson::Value((int)driver.type), configJSON.GetAllocator());
			driverJSON.AddMember("signal", rapidjson::Value((int)driver.portSignal), configJSON.GetAllocator());
			driverJSON.AddMember("portA", rapidjson::Value((int)driver.portA), configJSON.GetAllocator());
			driverJSON.AddMember("portB", rapidjson::Value((int)driver.portB), configJSON.GetAllocator());

			// Wheel Information
			driverJSON.AddMember("wheel", rapidjson::Value(), configJSON.GetAllocator());
			
			// Pneumatic Information
			driverJSON.AddMember("pneumatic", rapidjson::Value(), configJSON.GetAllocator());
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
