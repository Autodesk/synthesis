#ifndef _ROBORIO_H_
#define _ROBORIO_H_

#define ASIO_STANDALONE
#define ASIO_HAS_STD_ADDRESSOF
#define ASIO_HAS_STD_ARRAY
#define ASIO_HAS_CSTDINT
#define ASIO_HAS_STD_SHARED_PTR
#define ASIO_HAS_STD_TYPE_TRAITS


/**
 * \file roborio.h
 * \brief Defines internal structure of mock RoboRIO
 * This file defines the RoboRIOs structure
 */

#include <array>
#include <memory>
#include <mutex>
#include <vector>
#include <thread>
#include <atomic>

#include <asio.hpp>

#include "accelerometer.hpp"
#include "accumulator.hpp"
#include "analog_inputs.hpp"
#include "analog_outputs.hpp"
#include "can_bus.hpp"
#include "counter.hpp"
#include "digital_system.hpp"
#include "encoder.hpp"
#include "error.h"
#include "global.hpp"
#include "joystick.hpp"
#include "match_info.hpp"
#include "net_comm.hpp"
#include "power.hpp"
#include "pwm_system.hpp"
#include "relay_system.hpp"
#include "robot_mode.hpp"
#include "send_data.h"
#include "spi_system.hpp"
#include "sync_server.h"
#include "sys_watchdog.hpp"

namespace hel{
    /**
     * \struct RoboRIO roborio.h
     * \brief Mock RoboRIO implementation
     *
     * This class represents the internals of the RoboRIO hardware, broken up into several sub-systems:
     * Analog Input, Analog Output, PWM, DIO, SPI, MXP, RS232, and I2C.
     */
    struct RoboRIO{
        /**
         * \var bool user_button
         * \represents the state of the user button on the roborio
         */

        bool user_button;

        Accelerometer accelerometer;
        std::array<Accumulator, AnalogInputs::NUM_ANALOG_INPUTS> accumulators;
        AnalogInputs analog_inputs;
        AnalogOutputs analog_outputs;
        CANBus can_bus;
        std::array<Counter, Counter::MAX_COUNTER_COUNT> counters;
        DigitalSystem digital_system;
        std::vector<DSError> ds_errors;
        MatchInfo match_info;
        std::array<Encoder, Encoder::NUM_ENCODERS> encoders;
        Global global;
        std::array<Joystick, Joystick::MAX_JOYSTICK_COUNT> joysticks;
        NetComm net_comm;
        Power power;
        PWMSystem pwm_system;
        RelaySystem relay_system;
        RobotMode robot_mode;
        SPISystem spi_system;
        SysWatchdog watchdog;

        explicit RoboRIO() = default;

        friend class RoboRIOManager;
    private:
        RoboRIO(RoboRIO const&) = default;
        RoboRIO& operator=(const RoboRIO& r) = default;
    };
    /**
     *
     */

    class RoboRIOManager {

    public:

        // This is the only method exposed to the outside.
        // All other instance getters should be private, accessible through friend classes

        static std::pair<std::shared_ptr<RoboRIO>, std::unique_lock<std::recursive_mutex>> getInstance() {
            static int counter = 0;
            std::unique_lock<std::recursive_mutex> lock(m);
            if (instance == nullptr) {
                instance = std::make_shared<RoboRIO>();
            }
            if (counter > 2000) {
                auto send_instance = SendDataManager::getInstance();
                //send_instance.first->update();
                send_instance.second.unlock();
            }
            counter++;
            return std::make_pair(instance, std::move(lock));
        }

        static std::pair<std::shared_ptr<RoboRIO>, std::unique_lock<std::recursive_mutex>> getInstance(void*) {
            std::unique_lock<std::recursive_mutex> lock(m);
            if (instance == nullptr) {
                instance = std::make_shared<RoboRIO>();
            }
            return std::make_pair(instance, std::move(lock));
        }

        static RoboRIO getCopy() {
            return RoboRIO(*(RoboRIOManager::getInstance().first));
        }

        enum class Buffer {
            Recieve,
            Execute,
            Send,
        };


    private:
        RoboRIOManager() {}
        static std::shared_ptr<RoboRIO> instance;

        static std::recursive_mutex m;
    public:

        RoboRIOManager(RoboRIOManager const&) = delete;
        void operator=(RoboRIOManager const&) = delete;

        friend class SyncServer;
    };

}
#endif
