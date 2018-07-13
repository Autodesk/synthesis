#ifndef _ROBORIO_INTERFACE_H_
#define _ROBORIO_INTERFACE_H_

#include "roborio.h"

namespace hel{
    struct RoboRIOInterface{
        struct MXPData{
            enum class Config{
                DIO, PWM, SPI, I2C
            };

            Config config;

            double value;
        };

        enum class RelayState{OFF, REVERSE, FORWARD, ERROR};

    private:
        std::array<double, tPWM::kNumHdrRegisters> pwm_hdrs;
        
        std::array<RelayState,hal::kNumRelayHeaders> relays;

        std::array<double, hal::kNumAnalogOutputs> analog_outputs;

        std::array<MXPData, hal::kNumDigitalMXPChannels> digital_mxp;

    public:
        void update();

        std::string toString()const;

        std::string serialize()const;
    };
}

#endif
