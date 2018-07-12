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
                                return RelayState::_11; //TODO error handling
                            } else{
                                return RelayState::_10;
                            }
                        } else if(HAL_GetRelay(reverse_handle, &status)){
                            return RelayState::_01;
                        }
                        return RelayState::_00;
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
            digital_mxp[i].value = 0; //TODO
        case MXPData::Config::PWM:
            digital_mxp[i].value = 0; //TODO
        case MXPData::Config::SPI:
            digital_mxp[i].value = 0; //TODO
        case MXPData::Config::I2C:
            digital_mxp[i].value = 0; //TODO
        default:
            ; //TODO error handling
        }
    }
}

std::string hel::RoboRIOInterface::toString()const{
    return ""; //TODO
}
