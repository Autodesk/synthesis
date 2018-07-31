#ifndef _SEND_DATA_HPP_
#define _SEND_DATA_HPP_

#include <array>
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

namespace hel{
    struct SendData{
        enum class RelayState{OFF, REVERSE, FORWARD, ERROR};

    private:
        std::string serialized_data;
        bool gen_serialization;

        std::array<double, PWMSystem::NUM_HDRS> pwm_hdrs;

        std::array<RelayState, RelaySystem::NUM_RELAY_HEADERS> relays;

        std::array<double, AnalogOutputs::NUM_ANALOG_OUTPUTS> analog_outputs;

        std::array<hel::MXPData, DigitalSystem::NUM_DIGITAL_MXP_CHANNELS> digital_mxp;

        std::array<bool, DigitalSystem::NUM_DIGITAL_HEADERS> digital_hdrs;

        std::map<uint32_t, CANMotorController> can_motor_controllers;
    public:
        SendData();

        void update();

        std::string toString()const;

        std::string serialize();
    };
    std::string to_string(SendData::RelayState);

    class SendDataManager {
    public:
        static std::pair<std::shared_ptr<SendData>, std::unique_lock<std::recursive_mutex>> getInstance() {
            std::unique_lock<std::recursive_mutex> lock(m);
            if (instance == nullptr) {
                instance = std::make_shared<SendData>();
            }
            return std::make_pair(instance, std::move(lock));
        }

    private:
        static std::shared_ptr<SendData> instance;
        static std::recursive_mutex m;

    };
}

#endif
