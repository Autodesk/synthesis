#pragma once

#include <map>
#include <vector>
#include <Fusion/Components/Joint.h>
#include "CustomJSONObject.h"
#include "Driver.h"

using namespace adsk;

namespace BXDJ
{
	class ConfigData : public CustomJSONObject
	{
	public:
		std::string robotName;

		ConfigData();
		ConfigData(const ConfigData & other);

		std::unique_ptr<Driver> getDriver(core::Ptr<fusion::Joint>) const;
		void setDriver(core::Ptr<fusion::Joint>, const Driver &);
		void setNoDriver(core::Ptr<fusion::Joint>);

		// Removes joint configurations that are not in a vector of joints, and adds empty configurations for those not present.
		void filterJoints(std::vector<core::Ptr<fusion::Joint>>);

		rapidjson::Value getJSONObject(rapidjson::MemoryPoolAllocator<>&) const;
		void loadJSONObject(const rapidjson::Value&);

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
				driver = (other.driver == nullptr) ? nullptr : std::make_unique<Driver>(*other.driver);
			}

			JointConfig& JointConfig::operator=(const JointConfig &other)
			{
				name = other.name;
				motion = other.motion;
				driver = (other.driver == nullptr) ? nullptr : std::make_unique<Driver>(*other.driver);
				return *this;
			}
		};

		std::map<std::string, JointConfig> joints;

		JointMotionType internalJointMotion(fusion::JointTypes) const;

	};
}
