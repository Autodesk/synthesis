#include "roborio_interface.h"

#include "HAL/HAL.h"

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
            return MXPData::Config::DIO; //TODO
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
}

std::string hel::RoboRIOInterface::toString()const{
    return ""; //TODO implement function in readable print-out
}

std::string hel::RoboRIOInterface::serialize()const{
    std::string s = "roborio: {";
    
    s += "pwm_hdrs: [";
    for(unsigned i = 0; i < pwm_hdrs.size(); i++){
        s += std::to_string(pwm_hdrs[i]);
        if((i + 1) < pwm_hdrs.size()){
            s += ",";
        }
    }
    s += "],";
    
    //TODO finish

    s += "]";
    return s;
}
