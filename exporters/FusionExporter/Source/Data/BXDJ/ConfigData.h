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
		ConfigData(std::string jsonConfig);
		ConfigData(const ConfigData & other);

		std::unique_ptr<Driver> getDriver(core::Ptr<fusion::Joint>) const;
		void setDriver(core::Ptr<fusion::Joint>, const Driver &);
		void setNoDriver(core::Ptr<fusion::Joint>);

		void loadFromJSON(std::string);
		std::string toString() const;

	private:
		// Constants used for communicating joint motion type
		enum JointMotionType : int
		{
			ANGULAR = 1, LINEAR = 2, BOTH = 3, NEITHER = 0
		};

		struct JointConfig
		{
			std::string name;
			JointMotionType motion;
			std::unique_ptr<Driver> driver;

			JointConfig() { name = ""; motion = NEITHER; driver = nullptr; }

			JointConfig(const JointConfig & other)
			{
				name = other.name;
				motion = other.motion;
				driver = std::make_unique<Driver>(*other.driver);
			}

			JointConfig& JointConfig::operator=(const JointConfig &other)
			{
				name = other.name;
				motion = other.motion;
				driver = std::make_unique<Driver>(*other.driver);
				return *this;
			}
		};

		std::map<std::string, JointConfig> joints;

		JointMotionType internalJointMotion(fusion::JointTypes) const;

	};
}
