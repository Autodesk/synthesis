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
		ConfigData(const std::vector<core::Ptr<fusion::Joint>> & joints);
		ConfigData(std::string jsonConfig, const std::vector<core::Ptr<fusion::Joint>> & joints);
		ConfigData(const ConfigData & other);

		std::unique_ptr<Driver> getDriver(core::Ptr<fusion::Joint>) const;
		void setDriver(core::Ptr<fusion::Joint>, Driver);
		void setNoDriver(core::Ptr<fusion::Joint>);
		void setNoDrivers(const std::vector<core::Ptr<fusion::Joint>> &);

		std::string toString();

	private:
		// Constants used for communicating joint motion type
		enum JointMotionType : int
		{
			ANGULAR = 1, LINEAR = 2, BOTH = 3, NEITHER = 0
		};

		std::map<adsk::core::Ptr<adsk::fusion::Joint>, std::unique_ptr<Driver>> joints;

	};
}
