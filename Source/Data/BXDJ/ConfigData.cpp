#include "ConfigData.h"

using namespace BXDJ;

BXDJ::ConfigData::ConfigData()
{}

ConfigData::ConfigData(const ConfigData & other)
{
	for (auto i = other.joints.begin(); i != other.joints.end(); i++)
		joints[i->first] = std::make_unique<Driver>(*i->second);
}

std::unique_ptr<Driver> ConfigData::getDriver(adsk::core::Ptr<adsk::fusion::Joint> joint) const
{
	if (joints.find(joint) == joints.end() || joints.at(joint) == nullptr)
		return nullptr;

	return std::make_unique<Driver>(*joints.at(joint));
}

void ConfigData::setDriver(adsk::core::Ptr<adsk::fusion::Joint> joint, Driver driver)
{
	joints[joint] = std::make_unique<Driver>(driver);
}
