#include "mxp_data.h"

std::string hel::to_string(hel::MXPData::Config config){
    switch(config){
    case hel::MXPData::Config::DIO:
        return "DIO";
    case hel::MXPData::Config::PWM:
        return "PWM";
    case hel::MXPData::Config::SPI:
        return "SPI";
    case hel::MXPData::Config::I2C:
        return "I2C";
    default:
        return ""; //TODO error handling
    }
}

