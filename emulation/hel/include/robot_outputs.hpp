#ifndef _ROBOT_OUTPUTS_HPP_
#define _ROBOT_OUTPUTS_HPP_

#include <map>
#include <memory>
#include <mutex>
#include <string>

#include "analog_outputs.hpp"
#include "can_motor_controller.hpp"
#include "digital_system.hpp"
#include "mxp_data.hpp"
#include "pwm_system.hpp"
#include "relay_system.hpp"
#include "robot_output_service.hpp"

namespace hel {

/**
 * \brief Container for all the data to send to the Synthesis engine
 * Contains functions to interpret RoboRIO data and prepare it for transmission
 */

struct RobotOutputs {
   private:
	/**
	 * \brief Whether the robot is enabled and RobotOutputs should send outputs
	 */

	bool enabled;

	/**
	 * \brief Generate zeroed-output packet when disabled
	 */

	const inline EmulationService::RobotOutputs generateZeroedOutput()const;

	/**
	 * \brief The interpreted states of all the PWM header outputs
	 */

    BoundsCheckedArray<double, PWMSystem::NUM_HDRS> pwm_hdrs;

    /**
	 * \brief The interpreted states of all the relay outputs
	 */

	BoundsCheckedArray<RelaySystem::State, RelaySystem::NUM_RELAY_HEADERS>
		relays;

	/**
	 * \brief The interpreted states of all the analog outputs
	 */

	BoundsCheckedArray<double, AnalogOutputs::NUM_ANALOG_OUTPUTS>
		analog_outputs;

	/**
	 * \brief The interpreted states of all the digital MXP outputs
	 */

	BoundsCheckedArray<MXPData, DigitalSystem::NUM_DIGITAL_MXP_CHANNELS>
		digital_mxp;

	/**
	 * \brief The interpreted states of all the digital header outputs
	 */

	BoundsCheckedArray<std::pair<DigitalSystem::HeaderConfig, bool>, DigitalSystem::NUM_DIGITAL_HEADERS> digital_hdrs;

	/**
	 * \brief All the CAN motor controller outputs
	 */

    std::map<uint32_t, std::shared_ptr<CANMotorControllerBase>> can_motor_controllers;

	/**
	 * \brief The cached robot outputs state for transmission
	 */

	EmulationService::RobotOutputs output;

   public:
	/**
	 * \brief Constructor for RobotOutputs
	 */

	RobotOutputs();

	/**
	 * \brief Update the data held by RobotOutputs from a copy of the RoboRIO
	 * instance This only updates the data supported by Synthesis's engine
	 */

	void updateShallow();

	/**
	 * \brief Update the data held by RobotOutputs from a copy of the RoboRIO
	 * instance This updates all the data supported by HEL
	 */

	void updateDeep();

	/**
	 * \brief Set output enable
	 * If RobotOutputs is disabled, it outputs zeroed outputs until it is re-enabled
	 * \param e The new value to use for enabled
	 */

	void setEnable(bool);

	/**
	 * \brief Format RobotOutputs as a string
	 * \return A string containing all the values held by RobotOutputs
	 */

	std::string toString() const;

	/**
	 * \brief Update the gRPC robot outputs state for transmission
	 *
	 * Only update core outputs
	 */
	
	EmulationService::RobotOutputs syncShallow();
	
	/**
	 * \brief Update the gRPC robot outputs state for transmission
	 *
	 * Update all outputs
	 */
	
	EmulationService::RobotOutputs syncDeep();

	/**
	 * \brief Get if RobotOutputs has new data
	 * \return True if RobotOutputs has been updated since last serialization
	 */

	bool hasNewData() const;
};

class RobotOutputsManager {  // TODO move to separate file
   public:
	static std::pair<std::shared_ptr<RobotOutputs>,
					 std::unique_lock<std::recursive_mutex>>
	getInstance() {
		std::unique_lock<std::recursive_mutex> lock(send_data_mutex);
		if (instance == nullptr) {
			instance = std::make_shared<RobotOutputs>();
		}
		return std::make_pair(instance, std::move(lock));
	}

   private:
	static std::shared_ptr<RobotOutputs> instance;
	static std::recursive_mutex send_data_mutex;
};
}  // namespace hel

#endif
