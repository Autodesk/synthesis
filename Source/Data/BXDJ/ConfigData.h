#pragma once

#include <map>
#include <Fusion/Components/Joint.h>

namespace BXDJ
{
	class Driver;

	class ConfigData
	{
	public:
		ConfigData();

		ConfigData(const ConfigData & other);

		void setDriver(adsk::core::Ptr<adsk::fusion::Joint>, Driver);
		std::unique_ptr<Driver> getDriver(adsk::core::Ptr<adsk::fusion::Joint>);

	private:
		std::map<adsk::core::Ptr<adsk::fusion::Joint>, std::unique_ptr<Driver>> joints;

	};
}
