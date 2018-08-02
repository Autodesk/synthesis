#include "roborio_manager.hpp"
#include "util.hpp"
#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

hel::SendData::SendData():serialized_data(""),new_data(true){
    for(auto& a: pwm_hdrs){
        a = 0.0;
    }
    for(auto& a: relays){
        a = RelayState::OFF;
    }
    for(auto& a: analog_outputs){
        a = 0.0;
    }
    for(auto& a: digital_mxp){
        a = {};
    }
    for(auto& a: digital_hdrs){
        a = {};
    }

    can_motor_controllers = {};
}

bool hel::SendData::hasNewData()const{
    return new_data;
}

void hel::SendData::update(){
    if(!hel::hal_is_initialized){
        return;
    }

    RoboRIO roborio = RoboRIOManager::getCopy();

    for(unsigned i = 0; i < pwm_hdrs.size(); i++){
        pwm_hdrs[i] = PWMSystem::getSpeed(roborio.pwm_system.getHdrPulseWidth(i));
    }

    for(unsigned i = 0; i < relays.size(); i++){
        relays[i] = [&](){
            bool forward = checkBitHigh(roborio.relay_system.getValue().Forward, i);
            bool reverse  = checkBitHigh(roborio.relay_system.getValue().Reverse, i);
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
        analog_outputs[i] = (roborio.analog_outputs.getMXPOutput(i)) * 5. / 0x1000;
    }

    for(unsigned i = 0; i < digital_mxp.size(); i++){
        digital_mxp[i].config = [&](){
            if(checkBitHigh(roborio.digital_system.getMXPSpecialFunctionsEnabled(), i)){
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
            tDIO::tOutputEnable output_mode = roborio.digital_system.getEnabledOutputs();
            if(checkBitHigh(output_mode.MXP,i)){
                return hel::MXPData::Config::DO;
            }
            return hel::MXPData::Config::DI;
        }();

        switch(digital_mxp[i].config){
        case hel::MXPData::Config::DO:
            digital_mxp[i].value = (checkBitHigh(roborio.digital_system.getOutputs().MXP, i) | checkBitHigh(roborio.digital_system.getPulses().MXP, i));
            break;
        case hel::MXPData::Config::PWM:
            {
                int remapped_i = i;
                if(remapped_i >= 4){ //digital ports 0-3 line up with mxp pwm ports 0-3, the rest are offset by 4
                    remapped_i -= 4;
                }
                digital_mxp[i].value = PWMSystem::getSpeed(roborio.pwm_system.getMXPPulseWidth(remapped_i));
            }
            break;
        case hel::MXPData::Config::SPI:
            std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Digital MXP input "<<i<<" configured for SPI during data send phase to engine\n";
            break;
        case hel::MXPData::Config::I2C:
            std::cerr<<"Synthesis warning: Feature unsupported by Synthesis: Digital MXP input "<<i<<" configured for I2C during data send phase to engine\n";
            break;
        default:
            break; //do nothing
        }
    }
    {
        tDIO::tOutputEnable output_mode = roborio.digital_system.getEnabledOutputs();
        auto values = roborio.digital_system.getOutputs().Headers;
        auto pulses = roborio.digital_system.getPulses().Headers;
        for(unsigned i = 0; i < digital_hdrs.size(); i++){
            if(checkBitHigh(output_mode.Headers, i)){
                digital_hdrs[i] = (checkBitHigh(values, i) | checkBitHigh(pulses, i));
            }
        }
    }
    can_motor_controllers = roborio.can_motor_controllers;
    new_data = true;
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
    s += "digital_mxp:" + hel::to_string(digital_mxp, std::function<std::string(MXPData)>([&](MXPData a){ return a.toString();})) + ", ";
    s += "digital_hdrs:" + hel::to_string(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
    s += "can_motor_controllers:" + hel::to_string(can_motor_controllers, std::function<std::string(std::pair<uint32_t,CANMotorController>)>([&](std::pair<uint32_t, CANMotorController> a){ return "[" + std::to_string(a.first) + ", " + a.second.toString() + "]";}));
    s += ")";
    return s;
}

std::string hel::SendData::serialize(){
    if(!new_data){
        return serialized_data;
    }

    serialized_data = "{\"roborio\":{";

    serialized_data += serializeList("\"pwm_hdrs\"", pwm_hdrs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
    serialized_data += ",";
    serialized_data += serializeList(
        "\"relays\"",
        relays,
        std::function<std::string(RelayState)>([&](RelayState r){
            return hel::quote(hel::to_string(r));
        })
    );
    serialized_data += ",";
    serialized_data += serializeList("\"analog_outputs\"", analog_outputs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
    serialized_data += ",";
    serialized_data += serializeList(
        "\"digital_mxp\"",
        digital_mxp,
        std::function<std::string(hel::MXPData)>([&](MXPData data){
            return "{\"config\":" + hel::quote(hel::to_string(data.config)) + ",\"value\":" + std::to_string(data.value) + "}";
        })
    );
    serialized_data += ",";
    serialized_data += serializeList(
        "\"digital_hdrs\"",
        digital_hdrs,
        std::function<std::string(bool)>([&](bool b){
            return b ? "1" : "0";
        })
    );

    serialized_data += ",";
    serialized_data += serializeList(
        "\"can_motor_controllers\"",
        can_motor_controllers,
        std::function<std::string(std::pair<uint32_t,CANMotorController>)>([&](std::pair<uint32_t, CANMotorController> a){
            return a.second.serialize();
        })
    );

    serialized_data += "}}";
    serialized_data += JSON_PACKET_SUFFIX;
    new_data = false;
    return serialized_data;
}
