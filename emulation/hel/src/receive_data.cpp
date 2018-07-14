#include "receive_data.h"

#include "HAL/HAL.h"
#include "util.h"
#include "json_util.h"

void hel::ReceiveData::update()const{
    auto instance = hel::RoboRIOManager::getInstance();

    for(unsigned i = 0; i < analog_inputs.size(); i++){
        instance.first->analog_inputs.setValues(i, analog_inputs[i]);
    }
    {
        tDIO::tDI di = instance.first->digital_system.getInputs();
        tDIO::tOutputEnable output_mode = instance.first->digital_system.getEnabledOutputs();
        for(unsigned i = 0; i < digital_hdrs.size(); i++){
            if(checkBitHigh(output_mode.MXP,i)){
                di.Headers = setBit(di.Headers, digital_hdrs[i], i);
            }
        }
        instance.first->digital_system.setInputs(di);
    }
    for(unsigned i = 0;i < digital_mxp.size(); i++){
        switch(digital_mxp[i].config){
        case MXPData::Config::DI:
        {
            tDIO::tDI di = instance.first->digital_system.getInputs();
            di.MXP = setBit(di.MXP, digital_mxp[i].value, i);
            instance.first->digital_system.setInputs(di);
            break;
        }
        case MXPData::Config::I2C:
            //TODO
            break;
        case MXPData::Config::SPI:
            //TODO
            break;
        default:
            ; //Do nothing
        }
    }
    instance.first->joysticks = joysticks;
    //TODO
}

std::string hel::ReceiveData::toString()const{
    return ""; //TODO implement function in readable print-out
}

void hel::ReceiveData::deserialize(std::string input){
    {
        unsigned i = 0;
        for(bool a: hel::deserializeList(
                hel::pullValue("\"dio\"", input),
                std::function<bool(std::string)>([&](std::string dio_str){
                    return (bool)std::stoi(dio_str);
                }),
                true
            )
        ){
            digital_hdrs[i] = a;
            i++;
            //TODO bounds check i
        }
    }
    //TODO finish
}
