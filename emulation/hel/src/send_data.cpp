#include "roborio.hpp"
#include "hal/HAL.h"
#include "util.hpp"
#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

void hel::SendData::update(){
    if(!hel::hal_is_initialized){
        return;
    }

    auto instance = RoboRIOManager::getInstance(nullptr);
    int32_t status = 0;
    for(unsigned i = 0; i < pwm_hdrs.size(); i++){
        pwm_hdrs[i] = HAL_GetPWMSpeed(i, &status);
        printf("%f\n", pwm_hdrs[i]);
        status = 0; //reset status between HAL calls
    }

    for(unsigned i = 0; i < relays.size(); i++){
        relays[i] = [&](){
            bool forward = HAL_GetRelay(i, &status);
			status = 0; //reset status between HAL calls
            bool reverse = HAL_GetRelay(i + RelaySystem::NUM_RELAY_HEADERS, &status);
			status = 0; //reset status between HAL calls

            if(forward){
                if(reverse){
                    return RelayState::ERROR;
                }
                return RelayState::FORWARD;
            }
            if(reverse){
                return RelayState::REVERSE;
            }
            return RelayState::OFF;
        }();
    }

    for(unsigned i = 0; i < analog_outputs.size(); i++){
        analog_outputs[i] = HAL_GetAnalogOutput(i, &status);
        status = 0; //reset status between HAL calls
    }

    for(unsigned i = 0; i < digital_mxp.size(); i++){
        digital_mxp[i].config = [&](){
            if(checkBitHigh(instance.first->digital_system.getMXPSpecialFunctionsEnabled(), i)){
                if(
                    i == 0  || i == 1  ||
                    i == 2  || i == 3  ||
                    i == 8  || i == 9  ||
                    i == 10 || i == 11 ||
                    i == 12 || i == 13
                ){
                    return hel::MXPData::Config::PWM;
                }
                if(
                    i == 4 || i == 5 ||
                    i == 6 || i == 7
                ){
                    return hel::MXPData::Config::SPI;
                }
                if(i == 14 || i == 15){
                    return hel::MXPData::Config::I2C;
                }
            }
            tDIO::tOutputEnable output_mode = instance.first->digital_system.getEnabledOutputs();
            if(checkBitHigh(output_mode.MXP,i)){
                return hel::MXPData::Config::DO;
            }
            return hel::MXPData::Config::DI;
        }();

        switch(digital_mxp[i].config){
        case hel::MXPData::Config::DO:
            digital_mxp[i].value = HAL_GetDIO(i + DigitalSystem::NUM_DIGITAL_HEADERS, &status);
            if(!digital_mxp[i].value){
                status = 0;
                digital_mxp[i].value = HAL_IsPulsing(i + DigitalSystem::NUM_DIGITAL_HEADERS, &status);
            }
            break;
        case hel::MXPData::Config::PWM:
            digital_mxp[i].value = HAL_GetPWMSpeed(i + tPWM::kNumHdrRegisters, &status);
            break;
        case hel::MXPData::Config::SPI:
            digital_mxp[i].value = 0; //TODO
            break;
        case hel::MXPData::Config::I2C:
            digital_mxp[i].value = 0; //TODO
            break;
        default:
            throw UnhandledEnumConstantException("hel::MXPData::Config");
        }
        status = 0; //reset status between HAL calls
    }
    {
        tDIO::tOutputEnable output_mode = instance.first->digital_system.getEnabledOutputs();
        for(unsigned i = 0; i < digital_hdrs.size(); i++){
            if(checkBitHigh(output_mode.MXP,i)){
                digital_hdrs[i] = HAL_GetDIO(i, &status);
                if(!digital_hdrs[i]){
                    status = 0;
                    digital_hdrs[i] = HAL_IsPulsing(i, &status);
                }
            }
            status = 0; //reset status between HAL calls
        }
    }
    instance.second.unlock();
}

std::string hel::to_string(hel::SendData::RelayState r){
    switch(r){
    case hel::SendData::RelayState::OFF:
        return "OFF";
    case hel::SendData::RelayState::REVERSE:
        return "REVERSE";
    case hel::SendData::RelayState::FORWARD:
        return "FORWARD";
    case hel::SendData::RelayState::ERROR:
        return "ERROR";
    default:
        throw UnhandledEnumConstantException("hel::SendData::RelayState");
    }
}

std::string hel::SendData::toString()const{
    std::string s = "(";
    s += "pwm_hdrs:" + hel::to_string(pwm_hdrs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string))) + ", ";
    s += "relays:" + hel::to_string(relays, std::function<std::string(hel::SendData::RelayState)>(static_cast<std::string(*)(hel::SendData::RelayState)>(hel::to_string)));
    s += "analog_outputs:" + hel::to_string(analog_outputs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
    s += "digital_hdrs:" + hel::to_string(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
    s += "digital_hdrs:" + hel::to_string(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(int)>(std::to_string)));
    s += ")";
    return s;
}

std::string hel::SendData::serialize()const{
    std::string s = "{\"roborio\":{";

    s += serializeList("\"pwm_hdrs\"", pwm_hdrs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
    s += ",";
    s += serializeList(
        "\"relays\"",
        relays,
        std::function<std::string(RelayState)>([&](RelayState r){
            return hel::quote(hel::to_string(r));
        })
    );
    s += ",";
    s += serializeList("\"analog_outputs\"", analog_outputs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
    s += ",";
    s += serializeList(
        "\"digital_mxp\"",
        digital_mxp,
        std::function<std::string(hel::MXPData)>([&](MXPData data){
            return "{\"config\":" + hel::quote(hel::to_string(data.config)) + ",\"value\":" + std::to_string(data.value) + "}";
        })
    );
    s += ",";
    s += serializeList(
        "\"digital_hdrs\"", 
        digital_hdrs,
        std::function<std::string(bool)>([&](bool b){
            return b ? "1" : "0";
        })
    );
    //TODO finish

    s += "}}";
    return s;
}
