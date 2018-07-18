#ifndef _MXP_DATA_HPP_
#define _MXP_DATA_HPP_

#include <string>

namespace hel{
    struct MXPData{
        enum class Config{
            DI, DO, PWM, SPI, I2C
        };

        Config config;

        double value;

        std::string serialize()const;
        static MXPData deserialize(std::string);

        MXPData();
    };

    MXPData::Config s_to_mxp_config(std::string);

    std::string to_string(MXPData::Config);
}

#endif
