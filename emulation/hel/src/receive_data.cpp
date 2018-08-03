#include "receive_data.hpp"

#include "roborio_manager.hpp"
#include "util.hpp"
#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace {
    const std::function<hel::Maybe<hel::EncoderManager>(hel::Maybe<std::string>)> liftedDeserialize = hel::Maybe<std::string>::lift<hel::EncoderManager>(hel::EncoderManager::deserialize);
}

hel::ReceiveData::ReceiveData():last_received_data(""),digital_hdrs(false), digital_mxp({}), joysticks({}), match_info({}), robot_mode({}), encoder_managers({}){}

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
        case MXPData::Config::SPI:
        default:
            ; //Do nothing
        }
    }
    instance.first->joysticks = joysticks;
    instance.first->match_info = match_info;
    instance.first->robot_mode = robot_mode;
    instance.first->encoder_managers = encoder_managers;
    for(Maybe<EncoderManager>& a: instance.first->encoder_managers){
        if(a){
            a.get().update();
        }
    }
    instance.first->engine_initialized = true;
    instance.second.unlock();
}

std::string hel::ReceiveData::toString()const{
    std::string s = "(";
    s += "digital_hdrs:" + hel::to_string(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
    s += "joysticks:" + hel::to_string(joysticks, std::function<std::string(hel::Joystick)>([&](hel::Joystick joy){ return joy.toString(); })) + ", ";
    s += "digital_mxp:" + hel::to_string(digital_mxp, std::function<std::string(hel::MXPData)>([&](hel::MXPData mxp){ return mxp.serialize();})) + ", ";
    s += "match_info:" + match_info.toString() + ", ";
    s += "robot_mode:" + robot_mode.toString();
    s += "encoder_managers:" + hel::to_string(encoder_managers, std::function<std::string(Maybe<EncoderManager>)>([&](Maybe<EncoderManager> a){
                                                                                                                    if(a){
                                                                                                                        return a.get().serialize();
                                                                                                                    }
                                                                                                                    return std::string("null");
                                                                                                                }));
    s += ")";
    return s;
}

void hel::ReceiveData::deserializeDigitalHdrs(std::string& input){
    if(input.find(quote("digital_hdrs")) != std::string::npos){
        try{
            digital_hdrs = hel::deserializeList(
                hel::pullValue("\"digital_hdrs\"", input),
                std::function<bool(std::string)>(hel::stob),
                true);
        } catch(const std::exception& ex){
            throw JSONParsingException("digital_hdrs");
        }
    }
}

void hel::ReceiveData::deserializeJoysticks(std::string& input){
    if(input.find(quote("joysticks")) != std::string::npos){
        try{
            joysticks = hel::deserializeList(
                hel::pullValue("\"joysticks\"", input),
                std::function<Joystick(std::string)>(Joystick::deserialize),
                true);
        } catch(const std::exception& ex){
            throw JSONParsingException("joysticks");
        }
    }
}

void hel::ReceiveData::deserializeDigitalMXP(std::string& input){
    if(input.find(quote("digital_mxp")) != std::string::npos){
        try{
            digital_mxp = hel::deserializeList(
                hel::pullValue("\"digital_mxp\"", input),
                std::function<MXPData(std::string)>(MXPData::deserialize),
                true);
        } catch(const std::exception& ex){
            throw JSONParsingException("digital_mxp");
        }
    }
}

void hel::ReceiveData::deserializeMatchInfo(std::string& input){
    if(input.find(quote("match_info")) != std::string::npos){
        try{
            match_info = MatchInfo::deserialize(hel::pullValue("\"match_info\"", input));
        } catch(const std::exception& ex){
            throw JSONParsingException("match_info");
        }
    }
}

void hel::ReceiveData::deserializeRobotMode(std::string& input){
    if(input.find(quote("robot_mode")) != std::string::npos){
        try{
            robot_mode = RobotMode::deserialize(hel::pullValue("\"robot_mode\"", input));
        } catch(const std::exception& ex){
            throw JSONParsingException("robot_mode");
        }
    }
}

void hel::ReceiveData::deserializeEncoders(std::string& input){
    if(input.find(quote("encoders")) != std::string::npos){
        try{
            encoder_managers = hel::deserializeList(
                hel::pullValue("\"encoders\"", input),
                std::function<Maybe<EncoderManager>(std::string)>([&](std::string str){
																	  if(str == "null"){
																			  return Maybe<EncoderManager>();
																		  }
                                    Maybe<std::string> a = Maybe<std::string>(str);
                                    return a.fmap(liftedDeserialize);
																  }),
                true);
        } catch(const std::exception& ex){
            throw JSONParsingException("encoders");
        }
    }
}

void hel::ReceiveData::deserializeShallow(std::string input){
    if(input == last_received_data){
        return;
    }
    last_received_data = input;

    deserializeJoysticks(input);
    deserializeMatchInfo(input);
    deserializeRobotMode(input);
    deserializeEncoders(input);
}

void hel::ReceiveData::deserializeDeep(std::string input){
    if(input == last_received_data){
        return;
    }
    last_received_data = input;

    deserializeDigitalHdrs(input);
    deserializeJoysticks(input);
    deserializeDigitalMXP(input);
    deserializeMatchInfo(input);
    deserializeRobotMode(input);
    deserializeEncoders(input);
}

void hel::ReceiveData::deserializeAndUpdate(std::string input){
    deserializeDeep(input);

    update();
}
