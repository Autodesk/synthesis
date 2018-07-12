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

        enum class RelayState{
            _00, //Corresponds to off
            _01, //Corresponds to reverse only
            _10, //Corresponds to forward only
            _11  //Corresponds to forward and reverse, an error
        };

    private:
        std::array<double, tPWM::kNumHdrRegisters> pwm_hdrs;
        
        std::array<RelayState,hal::kNumRelayHeaders> relays;

        std::array<double, hal::kNumAnalogOutputs> analog_outputs;

        std::array<MXPData, hal::kNumDigitalMXPChannels> digital_mxp;

    public:
        void update();

        std::string toString()const;
    };
}

#endif
