#pragma once

#include <map>
#include <Fusion/Components/Joint.h>
#include "Driver.h"

namespace BXDJ
{
	class ConfigData
	{
	public:
		std::string robotName;

		ConfigData();

		ConfigData(const ConfigData & other);

		std::unique_ptr<Driver> getDriver(adsk::core::Ptr<adsk::fusion::Joint>) const;
		void setDriver(adsk::core::Ptr<adsk::fusion::Joint>, Driver);

	private:
		std::map<adsk::core::Ptr<adsk::fusion::Joint>, std::unique_ptr<Driver>> joints;

	};
}
