#pragma once

#include <map>
#include <vector>
#include <Fusion/Components/Joint.h>
#include "Driver.h"

using namespace adsk;

namespace BXDJ
{
	class ConfigData
	{
	public:
		std::string robotName;

		ConfigData();
		ConfigData(std::string jsonConfig, std::vector<core::Ptr<fusion::Joint>>& joints);
		ConfigData(const ConfigData & other);

		std::unique_ptr<Driver> getDriver(adsk::core::Ptr<adsk::fusion::Joint>) const;
		void setDriver(adsk::core::Ptr<adsk::fusion::Joint>, Driver);

	private:
		std::map<adsk::core::Ptr<adsk::fusion::Joint>, std::unique_ptr<Driver>> joints;

	};
}
