#ifndef _MXP_DATA_H_
#define _MXP_DATA_H_

#include <string>

namespace hel{
    struct MXPData{
        enum class Config{
            DI, DO, PWM, SPI, I2C
        };

        Config config;

        double value;
    };

    std::string to_string(MXPData::Config);
}

#endif
