#ifndef _SEND_DATA_HPP_
#define _SEND_DATA_HPP_

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

struct SendData {
   private:
	/**
	 * \brief Whether SendData has been updated since last serialization
	 */

	bool new_data;

	/**
	 * \brief Whether the robot is enabled and SendData should send outputs
	 */

	bool enabled;

	/**
	 * \brief The interpreted states of all the PWM header outputs
	 */
public:
	BoundsCheckedArray<double, PWMSystem::NUM_HDRS> pwm_hdrs;
private:
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

	BoundsCheckedArray<bool, DigitalSystem::NUM_DIGITAL_HEADERS> digital_hdrs;

	/**
	 * \brief All the CAN motor controller outputs
	 */

	std::map<uint32_t, CANMotorController> can_motor_controllers;

	EmulationService::RobotOutputs output;

	/**
	 * \brief Update the serialized_data with the PWM headers
	 */

	void serializePWMHdrs();

	/**
	 * \brief Update the serialized_data with the relays
	 */

	void serializeRelays();

	/**
	 * \brief Update the serialized_data with the analog outputs
	 */

	void serializeAnalogOutputs();

	/**
	 * \brief Update the serialized_data with the digital MXP
	 */

	void serializeDigitalMXP();

	/**
	 * \brief Update the serialized_data with the digital headers
	 */

	void serializeDigitalHdrs();

	/**
	 * \brief Update the serialized_data with the CAN motor controllers
	 */

	void serializeCANMotorControllers();

   public:
	/**
	 * \brief Constructor for SendData
	 */

	SendData();

	/**
	 * \brief Update the data held by SendData from a copy of the RoboRIO
	 * instance This only updates the data supported by Synthesis's engine
	 */

	void updateShallow();

	/**
	 * \brief Update the data held by SendData from a copy of the RoboRIO
	 * instance This updates all the data supported by HEL
	 */

	void updateDeep();

	/**
	 * \brief Set output enable
	 * If SendData is disabled, it outputs zeroed outputs until it is re-enabled
	 * \param e The new value to use for enabled
	 */

	void enable(bool);

	/**
	 * \brief Format SendData as a string
	 * \return A string containing all the values held by SendData
	 */

	std::string toString() const;

	EmulationService::RobotOutputs syncShallow();
	EmulationService::RobotOutputs syncDeep();

	/**
	 * \brief Update and return the JSON serialized outputs
	 * Only updates the cached serialized string if there is new data and it is
	 * enabled. If it is disabled, it returns zeroed outputs. This also only
	 * generates a string using the data supported by Synthesis's engine.
	 */

	std::string serializeShallow();

	/**
	 * \brief Update and return the JSON serialized outputs
	 * Only updates the cached serialized string if there is new data and it is
	 * enabled. If it is disabled, it returns zeroed outputs. This also
	 * generates a string using all the data supported by HEL.
	 */

	std::string serializeDeep();

	/**
	 * \brief Get if SendData has new data
	 * \return True if SendData has been updated since last serialization
	 */

	bool hasNewData() const;
};

class SendDataManager {  // TODO move to separate file
   public:
	static std::pair<std::shared_ptr<SendData>,
					 std::unique_lock<std::recursive_mutex>>
	getInstance() {
		std::unique_lock<std::recursive_mutex> lock(send_data_mutex);
		if (instance == nullptr) {
			instance = std::make_shared<SendData>();
		}
		return std::make_pair(instance, std::move(lock));
	}

   private:
	static std::shared_ptr<SendData> instance;
	static std::recursive_mutex send_data_mutex;
};
}  // namespace hel

#endif
