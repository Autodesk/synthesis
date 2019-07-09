#ifndef _RECIEVE_DATA_HPP_
#define _RECIEVE_DATA_HPP_

#include <memory>
#include <mutex>

#include <emulator_service.grpc.pb.h>

#include "bounds_checked_array.hpp"
#include "digital_system.hpp"
#include "encoder_manager.hpp"
#include "fpga_encoder.hpp"
#include "joystick.hpp"
#include "match_info.hpp"
#include "mxp_data.hpp"
#include "robot_mode.hpp"

namespace hel {
/**
 * \brief Container for all the data received from the Synthesis engine
 * Contains functions to interpret the data and populate the RoboRIO object held
 * by the RoboRIOManager
 */

struct ReceiveData {
   private:
	/**
	 * \brief The states of all the digital headers configured in input mode
	 */

	BoundsCheckedArray<bool, DigitalSystem::NUM_DIGITAL_HEADERS>
		digital_hdrs;  // TODO capture the third state where the digital headers
					   // are configured for output

	/**
	 * \brief The states of all the digital MXP pins configured in input mode
	 */

	BoundsCheckedArray<MXPData, DigitalSystem::NUM_DIGITAL_MXP_CHANNELS>
		digital_mxp;

	/**
	 * \brief The states of all the joystick inputs set by the engine
	 */

	BoundsCheckedArray<Joystick, Joystick::MAX_JOYSTICK_COUNT> joysticks;

	/**
	 * \brief The match info as set by the engine
	 */

	MatchInfo match_info;

	/**
	 * \brief The robot mode as set by the engine
	 */

	RobotMode robot_mode;

	/**
	 * \brief The states of all the encoders
	 */

	BoundsCheckedArray<Maybe<EncoderManager>, FPGAEncoder::NUM_ENCODERS>
		encoder_managers;

   public:
	/**
	 * \brief Update the data held by the RoboRIO instance in RoboRIOManager
	 * given received data For efficiency, this only touches the inputs
	 * supported by Synthesis's engine.
	 */

	void updateShallow() const;

	/**
	 * \brief Update the data held by the RoboRIO instance in RoboRIOManager
	 * given received data This touches all RoboRIO inputs supported by HEL, not
	 * just those supported by Synthesis's engine
	 */

	void updateDeep() const;

	/**
	 * \brief Format the received data as a string
	 * \return A string containing all the received data
	 */

	std::string toString() const;

	void sync(const EmulationService::RobotInputs&);

    void syncDeep(const EmulationService::RobotInputs&);

	/**
	 * Constructor for ReceiveData
	 */

	ReceiveData();
};

class ReceiveDataManager {  // TODO move to separate file
   public:
	static std::pair<std::shared_ptr<ReceiveData>,
					 std::unique_lock<std::recursive_mutex>>
	getInstance() {
		std::unique_lock<std::recursive_mutex> lock(receive_data_mutex);
		if (instance == nullptr) {
			instance = std::make_shared<ReceiveData>();
		}
		return std::make_pair(instance, std::move(lock));
	}

   private:
	static std::shared_ptr<ReceiveData> instance;
	static std::recursive_mutex receive_data_mutex;
};
}  // namespace hel

#endif
