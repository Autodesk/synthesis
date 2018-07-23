#include "receive_data.hpp"

#include "hal/HAL.h"
#include "roborio.hpp"
#include "util.hpp"
#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

void hel::ReceiveData::update()const{
    if(!hel::hal_is_initialized){
        return;
    }
    auto instance = hel::RoboRIOManager::getInstance();

    /*
    for(unsigned i = 0; i < analog_inputs.size(); i++){
        instance.first->analog_inputs.setValues(i, analog_inputs[i]);
    }
    */
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
    instance.first->joysticks = joysticks.toArray();
}

std::string hel::ReceiveData::toString()const{
    std::string s = "(";
    s += "digital_hdrs:" + hel::to_string(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
    s += "joysticks:" + hel::to_string(joysticks, std::function<std::string(hel::Joystick)>([&](hel::Joystick joy){ return joy.toString(); })) + ", ";
    s += "digital_mxp:" + hel::to_string(digital_mxp, std::function<std::string(hel::MXPData)>([&](hel::MXPData mxp){ return mxp.serialize();}));
    s += ")";
    return s;
}

void hel::ReceiveData::deserialize(std::string input){
    try{
    digital_hdrs = hel::deserializeList(
        hel::pullValue("\"digital_hdrs\"", input),
        std::function<bool(std::string)>(hel::stob),
        true);
    } catch(const std::exception& ex){
        //TODO error handling
    }
    try{
    joysticks = hel::deserializeList(
        hel::pullValue("\"joysticks\"", input),
        std::function<Joystick(std::string)>(Joystick::deserialize),
        true);
    } catch(const std::exception& ex){
        //TODO error handling
    }
    try{
        digital_mxp = hel::deserializeList(
        hel::pullValue("\"digital_mxp\"", input),
        std::function<MXPData(std::string)>(MXPData::deserialize),
        true);
    } catch(const std::exception& ex){
        //TODO error handling
    }
    //TODO finish
}
