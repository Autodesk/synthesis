#pragma once

#include <map>
#include <vector>
#include <Fusion/Components/Joint.h>
#include "CustomJSONObject.h"
#include "Driver.h"
#include "JointSensor.h"

using namespace adsk;

namespace BXDJ
{
	/// Stores the user's configuration for drivers, wheels, pneumatics, etc.
	/// All data is relative to a Joint.
	class ConfigData : public CustomJSONObject
	{
	public:
		/// Type of drive train.
		enum DrivetrainType : int
		{
			TANK = 1, ///< Tank
			H_DRIVE = 2, ///< H-Drive
			CUSTOM = 3 ///< Custom Drivetrain
		};

		enum ConvexType : int
		{
			BOX = 0,
			VHACD_LOW = 1,
			VHACD_MID = 2,
			VHACD_HIGH = 3
		};

		ConvexType convexType;
		std::string robotName; ///< Name of the robot. Used for writing to the robot directory.
		DrivetrainType drivetrainType; ///< The type of the robot's drivetrain.

		double weight; /// Weight of the subsystem

		std::string tempIconDir;

		///
		/// Creates a set of config data with no drivers, the name "unnamed," and tank drive.
		///
		ConfigData();

		/// Copy constructor.
		ConfigData(const ConfigData & other);

		// Drive Stuff
		void ConfigData::setDriveType(const std::string type);

		std::unique_ptr<Driver> getDriver(core::Ptr<fusion::Joint>) const; ///< \return The driver assigned to a Fusion joint.
		std::unique_ptr<Driver> getDriver(core::Ptr<fusion::AsBuiltJoint>) const; ///< \return The driver assigned to a Fusion as-built joint.

		///
		/// Assigns a driver to a joint in Fusion.
		/// \param joint Joint to apply driver to.
		/// \param driver Driver to apply to joint.
		///
		void setDriver(core::Ptr<fusion::Joint>, const Driver &);
		///
		/// Assigns a driver to an as-built joint in Fusion.
		/// \param joint Joint to apply driver to.
		/// \param driver Driver to apply to joint.
		///
		void setDriver(core::Ptr<fusion::AsBuiltJoint>, const Driver &);
		
		///
		/// Unassigns any driver from a joint in Fusion.
		/// \param joint Joint to remove driver from.
		///
		void setNoDriver(core::Ptr<fusion::Joint>);
		///
		/// Unassigns any driver from an as-built joint in Fusion.
		/// \param joint Joint to remove driver from.
		///
		void setNoDriver(core::Ptr<fusion::AsBuiltJoint>);

		std::vector<std::shared_ptr<JointSensor>> getSensors(core::Ptr<fusion::Joint>) const; ///< \return A vector containing the sensors attached to a Fusion joint.
		std::vector<std::shared_ptr<JointSensor>> getSensors(core::Ptr<fusion::AsBuiltJoint>) const; ///< \return A vector containing the sensors attached to a Fusion as-built joint.

		///
		/// Ensures that the only documented joints are those listed in the given vector, and that all joints in that vector are documented.
		/// \param filterJoints Joints to keep in the ConfigData.
		///
		void filterJoints(std::vector<core::Ptr<fusion::Joint>> filterJoints);
		///
		/// Ensures that the only documented as-built joints are those listed in the given vector, and that all joints in that vector are documented.
		/// \param filterJoints As-built joints to keep in the ConfigData.
		///
		void filterJoints(std::vector<core::Ptr<fusion::AsBuiltJoint>> filterJoints);

		nlohmann::json getJSONObject() const;
		void loadJSONObject(const nlohmann::json);

		static std::string toString(DrivetrainType); ///< \return The name of the drivetrain.

		/// Constants used for communicating joint motion type
		enum JointMotionType : int
		{
			ANGULAR = 1, ///< Joint is allowed to have a wheel.
			LINEAR = 2, ///< Joint is allowed to have pneumatics.
			BOTH = 3, ///< Joint can have a wheel or pneumatics.
			NEITHER = 0 ///< Joint cannot have wheel or pneumatics.
		};

		/// Contains the information needed to identify a Fusion joint, as well as any driver or sensors on the associated Joint.
		struct JointConfig
		{
			std::string name; ///< The name of the Fusion joint.
			bool asBuilt; ///< Is an as built joint.
			JointMotionType motion; ///< The limitation of the joint's motion.
			std::unique_ptr<Driver> driver; ///< The driver assigned to the joint.
			std::vector<std::shared_ptr<JointSensor>> sensors; /// The sensors assigned to the joint.

			///
			/// Creates a nameless configuration without a driver.
			///
			JointConfig() { name = ""; asBuilt = false; motion = NEITHER; driver = nullptr; }

			/// Copy constructor.
			JointConfig(const JointConfig & other)
			{
				name = other.name;
				asBuilt = other.asBuilt;
				motion = other.motion;
				driver = (other.driver == nullptr) ? nullptr : std::make_unique<Driver>(*other.driver);
				for (std::shared_ptr<JointSensor> sensor : other.sensors)
					sensors.push_back(std::make_shared<JointSensor>(*sensor));
			}

			JointConfig& JointConfig::operator=(const JointConfig &other)
			{
				name = other.name;
				asBuilt = other.asBuilt;
				motion = other.motion;
				driver = (other.driver == nullptr) ? nullptr : std::make_unique<Driver>(*other.driver);
				sensors.clear();
				for (std::shared_ptr<JointSensor> sensor : other.sensors)
					sensors.push_back(std::make_shared<JointSensor>(*sensor));
				return *this;
			}
		};

		std::map<std::string, JointConfig> getJoints();

	private:

		std::map<std::string, JointConfig> joints; ///< Map of all documented Fusion joints, accessed by their ID generated from Utility::getUniqueJointID.

		static JointMotionType internalJointMotion(fusion::JointTypes); ///< \return The JointMotionType equivalent of a Fusion joint type.

	};
}
