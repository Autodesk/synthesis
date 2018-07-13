#ifndef _SEND_DATA_H_
#define _SEND_DATA_H_

#include "roborio.h"
#include "mxp_data.h"

namespace hel{
    struct SendData{
        enum class RelayState{OFF, REVERSE, FORWARD, ERROR};

    private:
        std::array<double, tPWM::kNumHdrRegisters> pwm_hdrs;

        std::array<RelayState,hal::kNumRelayHeaders> relays;

        std::array<double, hal::kNumAnalogOutputs> analog_outputs;

        std::array<hel::MXPData, hal::kNumDigitalMXPChannels> digital_mxp;

        std::array<bool, hal::kNumDigitalHeaders> digital_hdrs;
    public:
        void update();

        std::string toString()const;

        std::string serialize()const;
    };

    class SendDataManager {

    public:
        static std::shared_ptr<SendData> getInstance() {
            if (instance == nullptr) {
                instance = std::make_shared<SendData>();
            }
            return instance;
        }

    private:
        static std::shared_ptr<SendData> instance;

    };

    std::string to_string(SendData::RelayState);
}

#endif
