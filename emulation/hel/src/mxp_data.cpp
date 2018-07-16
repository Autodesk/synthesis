#include "mxp_data.h"
#include "json_util.h"

std::string hel::to_string(hel::MXPData::Config config){
    switch(config){
    case hel::MXPData::Config::DI:
        return "DI";
    case hel::MXPData::Config::DO:
        return "DO";
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

std::string hel::MXPData::serialize()const{
    std::string s = "MXPData: {";
    s += "\"config\":" + hel::to_string(config) + ", ";
    s += "\"value\":" + std::to_string(value);
    s += "}";
    return s;
}

hel::MXPData hel::MXPData::deserialize(std::string s){
    MXPData m;
    m.config = static_cast<hel::MXPData::Config>(std::stoi(hel::pullValue("\"config\"",s)));
    m.value = std::stod(hel::pullValue("\"value\"",s));
    return m;
}
