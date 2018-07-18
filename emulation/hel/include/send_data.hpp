#ifndef _SEND_DATA_HPP_
#define _SEND_DATA_HPP_


#include "HAL/HAL.h"
#include "mxp_data.hpp"
#include "athena/PortsInternal.h"
#include "HAL/ChipObject.h"
#include <array>
#include <string>
#include <memory>
#include <mutex>
#include <condition_variable>

namespace hel{
    struct SendData{
        enum class RelayState{OFF, REVERSE, FORWARD, ERROR};

    private:
        std::array<double, nFPGA::nRoboRIO_FPGANamespace::tPWM::kNumHdrRegisters> pwm_hdrs;

        std::array<RelayState, hal::kNumRelayHeaders> relays;

        std::array<double, hal::kNumAnalogOutputs> analog_outputs;

        std::array<hel::MXPData, hal::kNumDigitalMXPChannels> digital_mxp;

        std::array<bool, hal::kNumDigitalHeaders> digital_hdrs;
    public:
        void update();

        std::string toString()const;

        std::string serialize()const;
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
        static std::condition_variable cv;

    private:
        static std::shared_ptr<SendData> instance;
        static std::recursive_mutex m;

    };


}

#endif
