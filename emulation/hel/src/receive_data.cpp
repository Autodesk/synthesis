#include "receive_data.hpp"

#include <algorithm>

#include "roborio_manager.hpp"
#include "util.hpp"
#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    namespace detail {
        const auto liftedDeserialize = hel::Maybe<std::string>::lift<hel::EncoderManager>(hel::EncoderManager::deserialize);
    }

    ReceiveData::ReceiveData():last_received_data(""),digital_hdrs(false), digital_mxp({}), joysticks({}), match_info({}), robot_mode({}), encoder_managers({}){}

    void ReceiveData::updateShallow()const{
        if(!hal_is_initialized){
            return;
        }
        auto instance = RoboRIOManager::getInstance();

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

    void ReceiveData::updateDeep()const{
        if(!hal_is_initialized){
            return;
        }
        auto instance = RoboRIOManager::getInstance();

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

    std::string ReceiveData::toString()const{
        std::string s = "(";
        s += "digital_hdrs:" + asString(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(bool)>(asString))) + ", ";
        s += "joysticks:" + asString(joysticks, std::function<std::string(Joystick)>(&Joystick::toString)) + ", ";
        s += "digital_mxp:" + asString(digital_mxp, std::function<std::string(MXPData)>(&MXPData::serialize)) + ", ";
        s += "match_info:" + match_info.toString() + ", ";
        s += "robot_mode:" + robot_mode.toString() + ", ";
        s += "encoder_managers:" + asString(encoder_managers, std::function<std::string(Maybe<EncoderManager>)>([&](Maybe<EncoderManager> a){
                                                                                                                     if(a){
                                                                                                                         return a.get().toString();
                                                                                                                     }
                                                                                                                     return std::string("null");
                                                                                                                 }));
        s += ")";
        return s;
    }

    void ReceiveData::deserializeDigitalHdrs(std::string& input){
        if(input.find(quote("digital_hdrs")) != std::string::npos){
            try{
                digital_hdrs = deserializeList(
                    pullObject("\"digital_hdrs\"", input),
                    std::function<bool(std::string)>(stob),
                    true);
            } catch(const std::exception& ex){
                throw JSONParsingException("digital_hdrs");
            }
        }
    }

    void ReceiveData::deserializeJoysticks(std::string& input){
        if(input.find(quote("joysticks")) != std::string::npos){
            try{
                joysticks = deserializeList(
                    pullObject("\"joysticks\"", input),
                    std::function<Joystick(std::string)>(Joystick::deserialize),
                    true);
            } catch(const std::exception& ex){
                throw JSONParsingException("joysticks");
            }
        }
    }

    void ReceiveData::deserializeDigitalMXP(std::string& input){
        if(input.find(quote("digital_mxp")) != std::string::npos){
            try{
                digital_mxp = deserializeList(
                    pullObject("\"digital_mxp\"", input),
                    std::function<MXPData(std::string)>(MXPData::deserialize),
                    true);
            } catch(const std::exception& ex){
                throw JSONParsingException("digital_mxp");
            }
        }
    }

    void ReceiveData::deserializeMatchInfo(std::string& input){
        if(input.find(quote("match_info")) != std::string::npos){
            try{
                match_info = MatchInfo::deserialize(pullObject("\"match_info\"", input));
            } catch(const std::exception& ex){
                throw JSONParsingException("match_info");
            }
        }
    }

    void ReceiveData::deserializeRobotMode(std::string& input){
        if(input.find(quote("robot_mode")) != std::string::npos){
            try{
                robot_mode = RobotMode::deserialize(pullObject("\"robot_mode\"", input));
            } catch(const std::exception& ex){
                throw JSONParsingException("robot_mode");
            }
        }
    }

    void ReceiveData::deserializeEncoders(std::string& input){
        if(input.find(quote("encoders")) != std::string::npos){
            try{
                encoder_managers = deserializeList(
                    pullObject("\"encoders\"", input),
                    std::function<Maybe<EncoderManager>(std::string)>([&](std::string str){
                                                                          if(trim(str) == "null"){
                                                                              return Maybe<EncoderManager>();
                                                                          }
                                                                          Maybe<std::string> a = Maybe<std::string>(str);
                                                                          return a.fmap(detail::liftedDeserialize);
                                                                      }),
                    true);
            } catch(const std::exception& ex){
                throw JSONParsingException("encoders");
            }
        }
    }

    void ReceiveData::deserializeShallow(std::string input){
        if(input == last_received_data){
            return;
        }
        last_received_data = input;

        deserializeJoysticks(input);
        deserializeMatchInfo(input);
        deserializeRobotMode(input);
        deserializeEncoders(input);
    }
    void ReceiveData::sync(const EmulationService::RobotInputs& req) {
        for(size_t i = 0; i < std::min(this->joysticks.size(), (size_t) req.joysticks_size()); i++) {
            auto joystick = &this->joysticks[i];
            auto joystick_data = req.joysticks(i);

            joystick->setIsXBox(joystick_data.is_xbox());
            joystick->setType((uint8_t)joystick_data.type());
            joystick->setName(joystick_data.name());
            joystick->setButtons(joystick_data.buttons());
            joystick->setButtonCount((uint8_t)joystick_data.button_count());

            for (size_t j = 0; j < std::min(joystick->getAxes().size(), (size_t) joystick_data.axis_size()); j++) {
                joysticks[i].getAxes()[j] = joystick_data.axis(j);
            }

            joystick->setAxisCount(joystick_data.axis_count());

            for (size_t j = 0; j < std::min(joystick->getAxisTypes().size(), (size_t) joystick_data.axis_size()); j++) {
                joysticks[i].getAxisTypes()[j] = joystick_data.axis_types(j);
            }

            for (size_t j = 0; j < std::min(joystick->getPOVs().size(), (size_t) joystick_data.povs_size()); j++) {
                joysticks[i].getPOVs()[j] = joystick_data.povs(j);
            }

            joystick->setPOVCount(joystick_data.pov_count());
            joystick->setOutputs(joystick_data.outputs());
            joystick->setLeftRumble(joystick_data.left_rumble());
            joystick->setRightRumble(joystick_data.right_rumble());
        }

        auto match_info = &this->match_info;
        match_info->setEventName(req.match_info().event_name());
        match_info->setGameSpecificMessage(req.match_info().game_specific_message());
        match_info->setMatchType(static_cast<MatchType_t>(req.match_info().match_type()));
        match_info->setMatchNumber(req.match_info().match_number());
        match_info->setReplayNumber(req.match_info().replay_number());
        match_info->setAllianceStationID(static_cast<AllianceStationID_t>(req.match_info().alliance_station_id()));
        match_info->setMatchTime(req.match_info().match_time());

        auto robot_mode = &this->robot_mode;
        robot_mode->setEnabled(req.robot_mode().enabled());
        robot_mode->setEmergencyStopped(req.robot_mode().is_emergency_stopped());
        robot_mode->setFMSAttached(req.robot_mode().is_fms_attached());
        robot_mode->setDSAttached(req.robot_mode().is_ds_attached());
        robot_mode->setMode(static_cast<RobotMode::Mode>(req.robot_mode().mode()));

        for (size_t i = 0; i < std::min(this->encoder_managers.size(), (size_t) req.encoder_managers_size()); i++) {
            auto encoder = &this->encoder_managers[i];
            auto encoder_data = req.encoder_managers(i);
            if(!encoder) {
                auto new_encoder = EncoderManager(
                                                  encoder_data.a_channel(),
                                                  static_cast<EncoderManager::PortType>(encoder_data.a_type()),
                                                  encoder_data.b_channel(),
                                                  static_cast<EncoderManager::PortType>(encoder_data.b_type())
                                                  );
                encoder->set(new_encoder);
            } else {
                encoder->get().setAChannel(encoder_data.a_channel());
                encoder->get().setAType(static_cast<EncoderManager::PortType>(encoder_data.a_channel()));
                encoder->get().setBChannel(encoder_data.b_channel());
                encoder->get().setBType(static_cast<EncoderManager::PortType>(encoder_data.b_channel()));
            }
        }
    }

    void ReceiveData::deepSync(const EmulationService::RobotInputs& req) {
 
        for(size_t i = 0; i < std::min(this->digital_hdrs.size(), (size_t) req.digital_headers_size()); i++) {
            this->digital_hdrs[i] = req.digital_headers(i);
        }

        for(size_t i = 0; i < std::min(this->digital_mxp.size(), (size_t) req.mxp_data_size()); i++) {
            auto mxp = &this->digital_mxp[i];
            auto mxp_data = req.mxp_data(i);
            mxp->config = static_cast<MXPData::Config>(mxp_data.mxp_config());
            mxp->value = mxp_data.value();
        }

        for(size_t i = 0; i < std::min(this->joysticks.size(), (size_t) req.joysticks_size()); i++) {
            auto joystick = &this->joysticks[i];
            auto joystick_data = req.joysticks(i);

            joystick->setIsXBox(joystick_data.is_xbox());
            joystick->setType((uint8_t)joystick_data.type());
            joystick->setName(joystick_data.name());
            joystick->setButtons(joystick_data.buttons());
            joystick->setButtonCount((uint8_t)joystick_data.button_count());

            for (size_t j = 0; j < std::min(joystick->getAxes().size(), (size_t) joystick_data.axis_size()); j++) {
                joysticks[i].getAxes()[j] = joystick_data.axis(j);
            }

            joystick->setAxisCount(joystick_data.axis_count());

            for (size_t j = 0; j < std::min(joystick->getAxisTypes().size(), (size_t) joystick_data.axis_size()); j++) {
                joysticks[i].getAxisTypes()[j] = joystick_data.axis_types(j);
            }

            for (size_t j = 0; j < std::min(joystick->getPOVs().size(), (size_t) joystick_data.povs_size()); j++) {
                joysticks[i].getPOVs()[j] = joystick_data.povs(j);
            }

            joystick->setPOVCount(joystick_data.pov_count());
            joystick->setOutputs(joystick_data.outputs());
            joystick->setLeftRumble(joystick_data.left_rumble());
            joystick->setRightRumble(joystick_data.right_rumble());
        }

        auto match_info = &this->match_info;
        match_info->setEventName(req.match_info().event_name());
        match_info->setGameSpecificMessage(req.match_info().game_specific_message());
        match_info->setMatchType(static_cast<MatchType_t>(req.match_info().match_type()));
        match_info->setMatchNumber(req.match_info().match_number());
        match_info->setReplayNumber(req.match_info().replay_number());
        match_info->setAllianceStationID(static_cast<AllianceStationID_t>(req.match_info().alliance_station_id()));
        match_info->setMatchTime(req.match_info().match_time());

        auto robot_mode = &this->robot_mode;
        robot_mode->setEnabled(req.robot_mode().enabled());
        robot_mode->setEmergencyStopped(req.robot_mode().is_emergency_stopped());
        robot_mode->setFMSAttached(req.robot_mode().is_fms_attached());
        robot_mode->setDSAttached(req.robot_mode().is_ds_attached());
        robot_mode->setMode(static_cast<RobotMode::Mode>(req.robot_mode().mode()));

        for (size_t i = 0; i < std::min(this->encoder_managers.size(), (size_t) req.encoder_managers_size()); i++) {
            auto encoder = &this->encoder_managers[i];
            auto encoder_data = req.encoder_managers(i);
            if(!encoder) {
                auto new_encoder = EncoderManager(
                                                  encoder_data.a_channel(),
                                                  static_cast<EncoderManager::PortType>(encoder_data.a_type()),
                                                  encoder_data.b_channel(),
                                                  static_cast<EncoderManager::PortType>(encoder_data.b_type())
                                                  );
                encoder->set(new_encoder);
            } else {
                encoder->get().setAChannel(encoder_data.a_channel());
                encoder->get().setAType(static_cast<EncoderManager::PortType>(encoder_data.a_channel()));
                encoder->get().setBChannel(encoder_data.b_channel());
                encoder->get().setBType(static_cast<EncoderManager::PortType>(encoder_data.b_channel()));
            }
        }

    }

    void ReceiveData::deserializeDeep(std::string input){
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
