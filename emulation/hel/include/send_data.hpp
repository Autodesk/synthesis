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

namespace hel{
    struct SendData{
    private:
        std::string serialized_data;
        bool new_data;
        bool enabled;
        BoundsCheckedArray<double, PWMSystem::NUM_HDRS> pwm_hdrs;

        BoundsCheckedArray<RelaySystem::State, RelaySystem::NUM_RELAY_HEADERS> relays;

        BoundsCheckedArray<double, AnalogOutputs::NUM_ANALOG_OUTPUTS> analog_outputs;

        BoundsCheckedArray<hel::MXPData, DigitalSystem::NUM_DIGITAL_MXP_CHANNELS> digital_mxp;

        BoundsCheckedArray<bool, DigitalSystem::NUM_DIGITAL_HEADERS> digital_hdrs;

        std::map<uint32_t, CANMotorController> can_motor_controllers;

        void serializePWMHdrs();
        void serializeRelays();
        void serializeAnalogOutputs();
        void serializeDigitalMXP();
        void serializeDigitalHdrs();
        void serializeCANMotorControllers();

    public:
        SendData();

        void updateShallow();
        void updateDeep();
        void enable(bool);

        std::string toString()const;

        std::string serializeShallow();
        std::string serializeDeep();

        bool hasNewData()const;
    };

    class SendDataManager {
    public:
        static std::pair<std::shared_ptr<SendData>, std::unique_lock<std::recursive_mutex>> getInstance() {
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
}

#endif
