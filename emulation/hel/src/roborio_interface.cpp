#include "roborio_interface.h"

#include "HAL/HAL.h"
#include "util.h"

void hel::RoboRIOInterface::update(){
    int32_t status = 0;

    for(unsigned i = 0; i < pwm_hdrs.size(); i++){
        pwm_hdrs[i] = HAL_GetPWMSpeed(i, &status);
    }

    for(unsigned i = 0; i < relays.size(); i++){
        relays[i] = [&](){
            int forward_handle = i;
            int reverse_handle = i + hal::kNumRelayHeaders;
            if(HAL_GetRelay(forward_handle, &status)){
                if(HAL_GetRelay(reverse_handle, &status)){
                    return RelayState::ERROR;
                } else{
                    return RelayState::FORWARD;
                }
            } else if(HAL_GetRelay(reverse_handle, &status)){
                return RelayState::REVERSE;
            }
            return RelayState::OFF;
        }();
    }

    for(unsigned i = 0; i < analog_outputs.size(); i++){
        analog_outputs[i] = HAL_GetAnalogOutput(i, &status);
    }

    for(unsigned i = 0; i < digital_mxp.size(); i++){
        digital_mxp[i].config = [&](){
            if(checkBitHigh(RoboRIOManager::getInstance()->digital_system.getMXPSpecialFunctionsEnabled(), i)){
                unsigned mxp_remapped_i = i + hal::kNumDigitalHeaders;
                if(
                    mxp_remapped_i == 0 || 
                    mxp_remapped_i == 1 || 
                    mxp_remapped_i == 2 || 
                    mxp_remapped_i == 3 || 
                    mxp_remapped_i == 8 || 
                    mxp_remapped_i == 9 || 
                    mxp_remapped_i == 10 || 
                    mxp_remapped_i == 11 || 
                    mxp_remapped_i == 12 || 
                    mxp_remapped_i == 13
                ){
                    return MXPData::Config::PWM;
                }
                if(
                    mxp_remapped_i == 5 || 
                    mxp_remapped_i == 6 || 
                    mxp_remapped_i == 7
                ){
                    return MXPData::Config::SPI;
                }
                if(
                    mxp_remapped_i == 14 || 
                    mxp_remapped_i == 15
                ){
                    return MXPData::Config::I2C;
                }
            }
            return MXPData::Config::DIO;
        }();
        switch(digital_mxp[i].config){
        case MXPData::Config::DIO:
            digital_mxp[i].value = HAL_GetDIO(i + hal::kNumDigitalHeaders, &status);
            break;
        case MXPData::Config::PWM:
            digital_mxp[i].value = HAL_GetPWMSpeed(i + tPWM::kNumHdrRegisters, &status);
            break;
        case MXPData::Config::SPI:
            digital_mxp[i].value = 0; //TODO
            break;
        case MXPData::Config::I2C:
            digital_mxp[i].value = 0; //TODO
            break;
        default:
            ; //TODO error handling
        }
    }

    for(unsigned i = 0; i < digital_hdrs.size(); i++){
        digital_hdrs[i] = HAL_GetDIO(i, &status);
    }
}

std::string hel::to_string(hel::RoboRIOInterface::RelayState r){
    switch(r){
    case hel::RoboRIOInterface::RelayState::OFF:
        return "OFF";
    case hel::RoboRIOInterface::RelayState::REVERSE:
        return "REVERSE";
    case hel::RoboRIOInterface::RelayState::FORWARD:
        return "FORWARD";
    case hel::RoboRIOInterface::RelayState::ERROR:
        return "ERROR";
    default:
        return ""; //TODO error handling
    }
}

std::string hel::RoboRIOInterface::toString()const{
    return ""; //TODO implement function in readable print-out
}

std::string hel::to_string(hel::RoboRIOInterface::MXPData::Config config){
    switch(config){
    case hel::RoboRIOInterface::MXPData::Config::DIO:
        return "DIO";
    case hel::RoboRIOInterface::MXPData::Config::PWM:
        return "PWM";
    case hel::RoboRIOInterface::MXPData::Config::SPI:
        return "SPI";
    case hel::RoboRIOInterface::MXPData::Config::I2C:
        return "I2C";
    default:
        return ""; //TODO error handling
    }
}

std::string hel::RoboRIOInterface::serialize()const{
    std::string s = "\"roborio\":{";

    s += serializeArray("\"pwm_hdrs\"", pwm_hdrs, static_cast<std::string(*)(double)>(std::to_string));
    s += ",";
    s += serializeArray("\"relays\"", relays, static_cast<std::string(*)(RelayState)>(hel::to_string));
    s += ",";
    s += serializeArray("\"analog_outputs\"", analog_outputs, static_cast<std::string(*)(double)>(std::to_string));
    s += ",";
    s += serializeArray(
        "\"digital_mxp\"",
        digital_mxp,
        std::function<std::string(MXPData)>([&](MXPData data){
            return "\"config:\"" + hel::to_string(data.config) + ",\"value\":" + std::to_string(data.value);
        })
    );
    s += ",";
    s += serializeArray(
        "\"digital_hdrs\"", 
        digital_hdrs,
        std::function<std::string(bool)>([&](bool b){
            return b ? "1" : "0";
        })
    );
    //TODO finish

    s += "}";
    return s;
}
