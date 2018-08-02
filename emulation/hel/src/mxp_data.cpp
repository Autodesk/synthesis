#include "mxp_data.hpp"
#include "util.hpp"
#include "json_util.hpp"
#include "error.hpp"

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
        throw UnhandledEnumConstantException("hel::MXPData::Config");
    }
}

hel::MXPData::Config hel::s_to_mxp_config(std::string s){
    switch(hasher(s.c_str())){
    case hasher("DI"):
        return hel::MXPData::Config::DI;
    case hasher("DO"):
        return hel::MXPData::Config::DO;
    case hasher("PWM"):
        return hel::MXPData::Config::PWM;
    case hasher("SPI"):
        return hel::MXPData::Config::SPI;
    case hasher("I2C"):
        return hel::MXPData::Config::I2C;
    default:
        throw UnhandledCase();
    }
}

hel::MXPData::MXPData()noexcept:config(hel::MXPData::Config::DI),value(0.0){}

hel::MXPData::MXPData(const MXPData& source)noexcept{
#define COPY(NAME) NAME = source.NAME
    COPY(config);
    COPY(value);
#undef COPY
}

std::string hel::MXPData::toString()const{
    std::string s = "(";
    s += "config:" + hel::to_string(config) + ", ";
    s += "value:" + std::to_string(value) + ")";
    return s;
}

std::string hel::MXPData::serialize()const{
    std::string s = "{";
    s += "\"config\":" + hel::quote(hel::to_string(config)) + ", ";
    s += "\"value\":" + std::to_string(value);
    s += "}";
    return s;
}

hel::MXPData hel::MXPData::deserialize(std::string s){
    MXPData m;
    m.config = s_to_mxp_config(hel::unquote(hel::pullValue("\"config\"",s)));
    m.value = std::stod(hel::pullValue("\"value\"",s));
    return m;
}
