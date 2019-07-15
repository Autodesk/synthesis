#ifndef _ROBORIO_HPP_
#define _ROBORIO_HPP_

#define ASIO_STANDALONE
#define ASIO_HAS_STD_ADDRESSOF
#define ASIO_HAS_STD_ARRAY
#define ASIO_HAS_CSTDINT
#define ASIO_HAS_STD_SHARED_PTR
#define ASIO_HAS_STD_TYPE_TRAITS

#include <array>
#include <map>
#include <memory>
#include <mutex>
#include <vector>
#include <thread>
#include <atomic>

#include <asio.hpp>

#include "accelerometer.hpp"
#include "accumulator.hpp"
#include "alarm.hpp"
#include "analog_inputs.hpp"
#include "analog_outputs.hpp"
#include "can_motor_controller.hpp"
#include "counter.hpp"
#include "digital_system.hpp"
#include "encoder_manager.hpp"
#include "error.hpp"
#include "fpga_encoder.hpp"
#include "global.hpp"
#include "joystick.hpp"
#include "match_info.hpp"
#include "net_comm.hpp"
#include "power.hpp"
#include "pwm_system.hpp"
#include "relay_system.hpp"
#include "receive_data.hpp"
#include "robot_mode.hpp"
#include "send_data.hpp"
#include "pcm.hpp"
#include "pdp.hpp"
#include "spi_system.hpp"
#include "sys_watchdog.hpp"
#include "system_interface.hpp"

/**
 * \mainpage Hardware Emulation Layer
 * HEL is a re-implementation of the Ni FPGA which would normally run on the RoboRIO. FRC user code interfaces with WPILib, which is a high level library built on HAL (the RoboRIO's hardware abstration layer). In turn, HAL is built on the Ni FPGA, which interfaces with hardware. Ni FPGA code is available as a set of header files, which can be found in allwpilib under ni-libraries. These headers contain pure abstract classes which HEL implements using derived classes. So where HAL calls Ni FPGA functions which normally communicate with hardware, those calls instead use HEL's implementation which communicate with its core, a [RoboRIO Singleton instance](@ref hel::RoboRIOManager) which handles the data.
 *
 * Reading into that RoboRIO instance, background threads serialize and deserialize data as JSON to communicate with Synthesis's engine over TCP. They update RoboRIO with received data such as joystick and encoder inputs while transmitting outputs such as PWM signals to the simulated robot.
 */

/**
 * \namespace nFPGA
 * \brief Namespace for all of Ni FPGA code
 */

namespace nFPGA{

    /**
     * \namespace nRoboRIO_FPGANamespace
     * \brief Namespace for all Ni FPGA RoboRIO chip object code
     */

    namespace nRoboRIO_FPGANamespace{}
}

/**
 * \namespace hel
 * \brief Namespace for all Synthesis emulation code
 */

namespace hel{
    /**
     * \brief Boolean representation of whether HAL has been initialized
     * This is used to prevent communication with Synthesis's engine before data is initialized
     */

    extern std::atomic<bool> hal_is_initialized;

    /**
     * \brief Mock RoboRIO implementation
     *
     * This class represents the internals of the RoboRIO hardware, broken up into several sub-systems.
     */
    struct RoboRIO{
        /**
         * \brief Represents the state of the user button on the roborio
         */

        bool user_button;

        /**
         * \brief Represents the state of the oboard accelerometer
         */

        Accelerometer accelerometer;

        /**
         * \brief Model for all the analog accumulators
         */

        BoundsCheckedArray<Accumulator, AnalogInputs::NUM_ANALOG_INPUTS> accumulators;

        /**
         * \brief Model for an alarm
         */

        Alarm alarm;

        /**
         * \brief Represents the states of all the analog inputs
         */

        AnalogInputs analog_inputs;

        /**
         * \brief Represents the states of all the analog outputs
         */

        AnalogOutputs analog_outputs;

        /**
         * \brief Represents the states of all the CAN motor controllers
         */

        std::map<uint32_t,std::shared_ptr<CANMotorControllerBase>> can_motor_controllers;

        /**
         * \brief Represents the states of all the counters
         */

        BoundsCheckedArray<Counter, Counter::MAX_COUNTER_COUNT> counters;

        /**
         * \brief Represents the states of all the digital pins
         */

        DigitalSystem digital_system;

        /**
         * \brief A vector of all the Driver Station errors that have been logged
         */

        std::vector<DSError> ds_errors;

        /**
         * \brief Container of all the FRC match information for the emulation running environment
         */

        MatchInfo match_info;

        /**
         * \brief Managers for all the encoder data
         * Maps encoder data either to counters or FPGA encoders as HAL expects it
         */

        BoundsCheckedArray<Maybe<EncoderManager>, FPGAEncoder::NUM_ENCODERS> encoder_managers; //TODO should be total number of FPGAEncoders and Counters

        /**
         * \brief Container for all the encoder data that HAL refers to as FPGA encoders
         */

        BoundsCheckedArray<FPGAEncoder, FPGAEncoder::NUM_ENCODERS> fpga_encoders;

        /**
         * \brief Data manager associated with Ni FPGA's tGlobal class
         */

        Global global;

        /**
         * \brief Represents the states of all the received joystick data
         */

        BoundsCheckedArray<Joystick, Joystick::MAX_JOYSTICK_COUNT> joysticks;

        /**
         * \brief Container for Driver Station networking data
         */

        NetComm net_comm;

        /**
         * \brief Represents the states of all the power rails
         */

        Power power;

        /**
         * \brief Represents the states of all the PWM outputs
         */

        PWMSystem pwm_system;

        /**
         * \brief Represents the states of all the relay outputs
         */

        RelaySystem relay_system;

        /**
         * \brief The robot mode as set by the simulated Driver Station
         */

        RobotMode robot_mode;

        /**
         * \brief Represents the state of an attached PCM
         */

        PCM pcm;

        /**
         * \brief Represents the state of an attached PDP
         */

        PDP pdp;

        /**
         * \brief Represnts the states of Ni FPGA's SPI system
         */

        SPISystem spi_system;

        /**
         * \brief Data manager associated with Ni FPGA's tSysWatchdog class
         */

        SysWatchdog watchdog;

        /**
         * Constructor for RoboRIO
         *
         * RoboRIOManager handles the RoboRIO instance used by all of HEL, so this should not be used by anything else
         */

        explicit RoboRIO()noexcept;

        /**
         * Deconstructor for RoboRIO
         */

        ~RoboRIO(){}

        friend class RoboRIOManager;

        /**
         * Constructor for RoboRIO
         * \param source A RoboRIO object to copy
         */

        RoboRIO(RoboRIO const&)noexcept;
    private:

        /**
         * \brief Assignment operator for RoboRIO
         * \param source A RoboRIO object to copy
         * \return The updated RoboRIO object
         */

        RoboRIO& operator=(const RoboRIO&);
    };
}
#endif
