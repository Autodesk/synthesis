#include "receive_data.hpp"

#include "roborio_manager.hpp"
#include "util.hpp"
#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace {
    const std::function<hel::Maybe<hel::EncoderManager>(hel::Maybe<std::string>)> liftedDeserialize = hel::Maybe<std::string>::lift<hel::EncoderManager>(hel::EncoderManager::deserialize);
}

namespace hel{
    hel::ReceiveData::ReceiveData():last_received_data(""),digital_hdrs(false), digital_mxp({}), joysticks({}), match_info({}), robot_mode({}), encoder_managers({}){}

    void hel::ReceiveData::updateShallow()const{
        if(!hel::hal_is_initialized){
            return;
        }
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->joysticks = joysticks;
        instance.first->match_info = match_info;
        instance.first->robot_mode = robot_mode;
        instance.first->encoder_managers = encoder_managers;
        for(Maybe<EncoderManager>& a: instance.first->encoder_managers){
            if(a){
                a.get().update();
            }
        }
        instance.second.unlock();
    }

    void hel::ReceiveData::updateDeep()const{
        if(!hel::hal_is_initialized){
            return;
        }
        auto instance = hel::RoboRIOManager::getInstance();

        updateShallow();
        {
            tDIO::tDI di = instance.first->digital_system.getInputs();
            tDIO::tOutputEnable output_mode = instance.first->digital_system.getEnabledOutputs();
            for(unsigned i = 0; i < digital_hdrs.size(); i++){
                if(checkBitLow(output_mode.Headers,i)){ //if set for input, then read in the inputs
                    di.Headers = setBit(di.Headers, digital_hdrs[i], i);
                }
            }
            //TODO add MXP digital inputs
            instance.first->digital_system.setInputs(di);
        }
        instance.second.unlock();
    }

    std::string hel::ReceiveData::toString()const{
        std::string s = "(";
        s += "digital_hdrs:" + as_string(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(bool)>(as_string))) + ", ";
        s += "joysticks:" + as_string(joysticks, std::function<std::string(hel::Joystick)>(&Joystick::toString)) + ", ";
        s += "digital_mxp:" + as_string(digital_mxp, std::function<std::string(hel::MXPData)>(&MXPData::serialize)) + ", ";
        s += "match_info:" + match_info.toString() + ", ";
        s += "robot_mode:" + robot_mode.toString() + ", ";
        s += "encoder_managers:" + as_string(encoder_managers, std::function<std::string(Maybe<EncoderManager>)>([&](Maybe<EncoderManager> a){
                                                                                                                     if(a){
                                                                                                                         return a.get().toString();
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
                    hel::pullObject("\"digital_hdrs\"", input),
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
                    hel::pullObject("\"joysticks\"", input),
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
                    hel::pullObject("\"digital_mxp\"", input),
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
                match_info = MatchInfo::deserialize(hel::pullObject("\"match_info\"", input));
            } catch(const std::exception& ex){
                throw JSONParsingException("match_info");
            }
        }
    }

    void hel::ReceiveData::deserializeRobotMode(std::string& input){
        if(input.find(quote("robot_mode")) != std::string::npos){
            try{
                robot_mode = RobotMode::deserialize(hel::pullObject("\"robot_mode\"", input));
            } catch(const std::exception& ex){
                throw JSONParsingException("robot_mode");
            }
        }
    }

    void hel::ReceiveData::deserializeEncoders(std::string& input){
        if(input.find(quote("encoders")) != std::string::npos){
            try{
                encoder_managers = hel::deserializeList(
                    hel::pullObject("\"encoders\"", input),
                    std::function<Maybe<EncoderManager>(std::string)>([&](std::string str){
                                                                          if(trim(str) == "null"){
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
}
