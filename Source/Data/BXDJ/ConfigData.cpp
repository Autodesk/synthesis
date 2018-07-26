#include "ConfigData.h"
#include "Driver.h"

using namespace BXDJ;

BXDJ::ConfigData::ConfigData()
{}

ConfigData::ConfigData(const ConfigData & other)
{
	for (auto i = other.joints.begin(); i != other.joints.end(); i++)
		joints[i->first] = std::make_unique<Driver>(*i->second);
}

void ConfigData::setDriver(adsk::core::Ptr<adsk::fusion::Joint> joint, Driver driver)
{
	joints[joint] = std::make_unique<Driver>(driver);
}

std::unique_ptr<Driver> ConfigData::getDriver(adsk::core::Ptr<adsk::fusion::Joint> joint)
{
	if (joints.find(joint) == joints.end() || joints[joint] == nullptr)
		return nullptr;

	return std::make_unique<Driver>(*joints[joint]);
}
