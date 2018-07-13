#include "receive_data.h"

#include "HAL/HAL.h"
#include "util.h"

void hel::ReceiveData::update()const{
    auto instance = hel::RoboRIOManager::getInstance();

    for(unsigned i = 0; i < analog_inputs.size(); i++){
        instance.first->analog_inputs.setValues(i, analog_inputs[i]);
    }

    for(unsigned i = 0; i < digital_hdrs.size(); i++){
        //TODO
    }
    for(unsigned i = 0;i < digital_mxp.size(); i++){
        if(digital_mxp[i].config == MXPData::Config::DIO){
            //TODO
        }
    }
    //TODO
}

std::string hel::ReceiveData::toString()const{
    return ""; //TODO implement function in readable print-out
}

void hel::ReceiveData::deserialize(std::string)const{
    //TODO finish
}
