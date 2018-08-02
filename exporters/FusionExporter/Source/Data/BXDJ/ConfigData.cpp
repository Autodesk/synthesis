#include "ConfigData.h"
#include <rapidjson/document.h>
#include <rapidjson/writer.h>
#include <rapidjson/stringbuffer.h>

using namespace adsk;
using namespace BXDJ;

ConfigData::ConfigData()
{
	robotName = "unnamed";
}

ConfigData::ConfigData(std::string configStr, std::vector<core::Ptr<fusion::Joint>>& joints)
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
	}
}

ConfigData::ConfigData(const ConfigData & other)
{
	robotName = other.robotName;

	for (auto i = other.joints.begin(); i != other.joints.end(); i++)
		joints[i->first] = std::make_unique<Driver>(*i->second);
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
