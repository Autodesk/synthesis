#include "roborio.hpp"
#include "util.hpp"
#include "json_util.hpp"
using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    RobotMode::Mode RobotMode::getMode()const{
        return mode;
    }

    void RobotMode::setMode(RobotMode::Mode m){
        mode = m;
    }

    bool RobotMode::getEnabled()const{
        return enabled;
    }

    void RobotMode::setEnabled(bool e){
        enabled = e;
    }

    bool RobotMode::getEmergencyStopped()const{
        return emergency_stopped;
    }

    void RobotMode::setEmergencyStopped(bool e){
        emergency_stopped = e;
    }

    bool RobotMode::getFMSAttached()const{
        return fms_attached;
    }

    void RobotMode::setFMSAttached(bool attached){
        fms_attached = attached;
    }

    bool RobotMode::getDSAttached()const{
        return ds_attached;
    }

    void RobotMode::setDSAttached(bool attached){
        ds_attached = attached;
    }

    ControlWord_t RobotMode::toControlWord()const{
        ControlWord_t control_word;
        control_word.enabled = enabled;
        control_word.autonomous = mode == Mode::AUTONOMOUS;
        control_word.test = mode == Mode::TEST;
        control_word.eStop = emergency_stopped;
        control_word.fmsAttached = fms_attached;
        control_word.dsAttached = ds_attached;
        return control_word;
    }

    std::string MatchInfo::getEventName()const{
        return event_name;
    }

    void MatchInfo::setEventName(std::string name){
        event_name = name;
    }

    std::string MatchInfo::getGameSpecificMessage()const{
        return game_specific_message;
    }

    void MatchInfo::setGameSpecificMessage(std::string message){
        game_specific_message = message;
    }

    MatchType_t MatchInfo::getMatchType()const{
        return match_type;
    }

    void MatchInfo::setMatchType(MatchType_t type){
        match_type = type;
    }

    uint16_t MatchInfo::getMatchNumber()const{
        return match_number;
    }

    void MatchInfo::setMatchNumber(uint16_t number){
        match_number = number;
    }

    uint8_t MatchInfo::getReplayNumber()const{
        return replay_number;
    }

    void MatchInfo::setReplayNumber(uint8_t number){
        replay_number = number;
    }

    AllianceStationID_t MatchInfo::getAllianceStationID()const{
        return alliance_station_id;
    }

    void MatchInfo::setAllianceStationID(AllianceStationID_t id){
        alliance_station_id = id;
    }

    double MatchInfo::getMatchTime()const{
        return match_time;
    }

    void MatchInfo::setMatchTime(double time){
        match_time = time;
    }

    bool Joystick::getIsXBox()const{
        return is_xbox;
    }

    void Joystick::setIsXBox(bool xbox){
        is_xbox = xbox;
    }

    uint8_t Joystick::getType()const{
        return type;
    }

    void Joystick::setType(uint8_t t){
        type = t;
    }

    std::string Joystick::getName()const{
        return name;
    }

    void Joystick::setName(std::string n){
        name = n;
    }

    uint32_t Joystick::getButtons()const{
        return buttons;
    }

    void Joystick::setButtons(uint32_t b){
        buttons = b;
    }

    uint8_t Joystick::getButtonCount()const{
        return button_count;
    }

    void Joystick::setButtonCount(uint8_t count){
        button_count = count;
    }

    BoundsCheckedArray<int8_t, Joystick::MAX_AXIS_COUNT> Joystick::getAxes()const{
        return axes;
    }

    void Joystick::setAxes(BoundsCheckedArray<int8_t, Joystick::MAX_AXIS_COUNT> a){
        axes = a;
    }

    uint8_t Joystick::getAxisCount()const{
        return axis_count;
    }

    void Joystick::setAxisCount(uint8_t count){
        axis_count = count;
    }

    BoundsCheckedArray<uint8_t, Joystick::MAX_AXIS_COUNT> Joystick::getAxisTypes()const{
        return axis_types;
    }

    void Joystick::setAxisTypes(BoundsCheckedArray<uint8_t, Joystick::MAX_AXIS_COUNT> types){
        axis_types = types;
    }

    BoundsCheckedArray<int16_t, Joystick::MAX_POV_COUNT> Joystick::getPOVs()const{
        return povs;
    }

    void Joystick::setPOVs(BoundsCheckedArray<int16_t, Joystick::MAX_POV_COUNT> p){
        povs = p;
    }

    uint8_t Joystick::getPOVCount()const{
        return pov_count;
    }

    void Joystick::setPOVCount(uint8_t count){
        pov_count = count;
    }

    uint32_t Joystick::getOutputs()const{
        return outputs;
    }

    void Joystick::setOutputs(uint32_t outs){
        outputs = outs;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    uint16_t Joystick::getLeftRumble()const{
        return left_rumble;
    }

    void Joystick::setLeftRumble(uint16_t rumble){
        left_rumble = rumble;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    uint16_t Joystick::getRightRumble()const{
        return right_rumble;
    }

    void Joystick::setRightRumble(uint16_t rumble){
        right_rumble = rumble;
        auto instance = SendDataManager::getInstance();
        instance.first->update();
        instance.second.unlock();
    }

    std::string Joystick::toString()const{
        std::string s = "(";
        s += "is_xbox:" + hel::to_string(is_xbox) + ", ";
        s += "type:" + std::to_string(type) + ", ";
        s += "name:" + name + ", ";
        s += "buttons:" + std::to_string(buttons) + ", ";
        s += "button_count:" + std::to_string((int)button_count) + ", ";
        s += "axes:" + hel::to_string(axes, std::function<std::string(int8_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += "axis_count:" + std::to_string(axis_count) + ", ";
        s += "axis_types" + hel::to_string(axis_types, std::function<std::string(uint8_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += "povs:" + hel::to_string(povs, std::function<std::string(int16_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += "pov_count:" + std::to_string(pov_count) +", ";
        s += "outputs:" + std::to_string(outputs) + ", ";
        s += "left_rumble:" + std::to_string(left_rumble) + ", ";
        s += "right_rumble:" + std::to_string(right_rumble);
        s += ")";
        return s;
    }
    std::string Joystick::serialize()const{
        std::string s = "{";
        s += "\"is_xbox\":" + hel::to_string(is_xbox) + ", ";
        s += "\"type\":" + std::to_string(type) + ", ";
        s += "\"name\":" + hel::quote(name) + ", ";
        s += "\"buttons\":" + std::to_string(buttons) + ", ";
        s += "\"button_count\":" + std::to_string((int)button_count) + ", ";
        s += hel::serializeList("\"axes\"", axes, std::function<std::string(int8_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += "\"axis_count\":" + std::to_string(axis_count) + ", ";
        s += hel::serializeList("\"axis_types\"", axis_types, std::function<std::string(uint8_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += hel::serializeList("\"povs\"", povs, std::function<std::string(int16_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += "\"pov_count\":" + std::to_string(pov_count) +", ";
        s += "\"outputs\":" + std::to_string(outputs) + ", ";
        s += "\"left_rumble\":" + std::to_string(left_rumble) + ", ";
        s += "\"right_rumble\":" + std::to_string(right_rumble);
        s += "}";
        return s;
    }

    Joystick Joystick::deserialize(std::string s){
        Joystick joy;
        joy.is_xbox = hel::stob(hel::pullValue("\"is_xbox\"", s));
        joy.type = std::stoi(hel::pullValue("\"type\"",s));
        joy.name = hel::unquote(hel::pullValue("\"name\"", s));
        joy.buttons = std::stoi(hel::pullValue("\"buttons\"", s));
        joy.button_count = std::stoi(hel::pullValue("\"button_count\"", s));
        std::vector<int8_t> axes_deserialized = hel::deserializeList(hel::pullValue("\"axes\"",s), std::function<int8_t(std::string)>([&](std::string s){ return std::stoi(s);}), true);
        if(axes_deserialized.size() == joy.axes.size()){
            joy.axes = axes_deserialized;
        } else {
            throw std::out_of_range("Exception: deserialization resulted in array of " + std::to_string(axes_deserialized.size()) + " axes, expected " + std::to_string(joy.axes.size()));
        }
        joy.axis_count = std::stoi(hel::pullValue("\"axis_count\"", s));
        std::vector<uint8_t> axis_types_deserialized = hel::deserializeList(hel::pullValue("\"axis_types\"",s), std::function<uint8_t(std::string)>([&](std::string s){ return std::stoi(s);}), true);
        if(axis_types_deserialized.size() == joy.axis_types.size()){
            joy.axis_types = axis_types_deserialized;
        } else {
            throw std::out_of_range("Exception: deserialization resulted in array of " + std::to_string(axis_types_deserialized.size()) + " axis types, expected " + std::to_string(joy.axis_types.size()));
        }
        std::vector<int16_t> povs_deserialized = hel::deserializeList(hel::pullValue("\"povs\"",s), std::function<int16_t(std::string)>([&](std::string s){ return std::stoi(s);}), true);
        if(povs_deserialized.size() == joy.povs.size()){
            joy.povs = povs_deserialized;
        } else {
            throw std::out_of_range("Exception: deserialization resulted in array of " + std::to_string(povs_deserialized.size()) + " povs, expected " + std::to_string(joy.povs.size()));
        }
        joy.pov_count = std::stoi(hel::pullValue("\"pov_count\"", s));
        joy.outputs = std::stoi(hel::pullValue("\"outputs\"", s));
        joy.left_rumble = std::stoi(hel::pullValue("\"left_rumble\"", s));
        joy.right_rumble = std::stoi(hel::pullValue("\"right_rumble\"", s));

        return joy;
    }

    Joystick::Joystick():is_xbox(false), type(0), name(""), buttons(0), button_count(0), axes(), axis_count(0), axis_types(), povs(), pov_count(0), outputs(0), left_rumble(0), right_rumble(0){}

    RobotMode::RobotMode():mode(RobotMode::Mode::TELEOPERATED),enabled(false),emergency_stopped(false),fms_attached(false),ds_attached(true){}

    MatchInfo::MatchInfo():event_name(),game_specific_message(),match_type(),match_number(0),replay_number(0),alliance_station_id(),match_time(){}
}
extern "C" {
    int FRC_NetworkCommunication_Reserve(void* /*instance*/){ //unnecessary for emulation
        return 0;
    }

    int FRC_NetworkCommunication_sendConsoleLine(const char* /*line*/){ //unnecessary for emulation
        return 0;
    }

    int FRC_NetworkCommunication_sendError(int isError, int32_t errorCode, int /*isLVCode*/, const char* details, const char* location, const char* callStack){
        auto instance = hel::RoboRIOManager::getInstance();
        instance.first->ds_errors.push_back({(bool)isError, errorCode, details, location, callStack}); //assuming isLVCode = false (not supporting LabView
        instance.second.unlock();
        return 0;
    }

    void setNewDataSem(pthread_cond_t*){} //unnecessary for emulation

    int setNewDataOccurRef(uint32_t refnum){
        auto instance = hel::RoboRIOManager::getInstance();
        instance.first->net_comm.ref_num = refnum;

        instance.second.unlock();
        return 0;
    }

    int FRC_NetworkCommunication_getControlWord(struct ControlWord_t* controlWord){
        auto instance = hel::RoboRIOManager::getInstance();
        if (controlWord != nullptr) {
            *controlWord = instance.first->robot_mode.toControlWord();
            controlWord->enabled = 1;
            controlWord->autonomous = 0;
            controlWord->test = 0;
            controlWord->dsAttached = 1;
            controlWord->eStop = 0;
        }

        instance.second.unlock();
        return 0; //HAL does not expect error status if parameters are nullptr
    }

    int FRC_NetworkCommunication_getAllianceStation(enum AllianceStationID_t* allianceStation){
        auto instance = hel::RoboRIOManager::getInstance();
        if (allianceStation != nullptr)
            *allianceStation = instance.first->match_info.getAllianceStationID();

        instance.second.unlock();
        return 0; //HAL does not expect error status if parameters are nullptr
    }

    int FRC_NetworkCommunication_getMatchInfo(char* eventName, MatchType_t* matchType, uint16_t* matchNumber, uint8_t* replayNumber, uint8_t* gameSpecificMessage, uint16_t* gameSpecificMessageSize){
        auto instance = hel::RoboRIOManager::getInstance();
        if (eventName != nullptr){ //HAL requires this to be silently handled
            hel::copystr(instance.first->match_info.getEventName(), eventName);
        }
        if (matchType != nullptr)
            *matchType = instance.first->match_info.getMatchType();
        if (matchNumber != nullptr)
            *matchNumber = instance.first->match_info.getMatchNumber();
        if (replayNumber != nullptr)
            *replayNumber = instance.first->match_info.getReplayNumber();

        if (gameSpecificMessage != nullptr){
            hel::copystr(instance.first->match_info.getGameSpecificMessage(), reinterpret_cast<char*>(gameSpecificMessage));
        }

        if (gameSpecificMessageSize != nullptr)
            *gameSpecificMessageSize = instance.first->match_info.getGameSpecificMessage().size();

        instance.second.unlock();
        return 0; //HAL does not expect error status if parameters are nullptr
    }

    int FRC_NetworkCommunication_getMatchTime(float* matchTime){
        auto instance = hel::RoboRIOManager::getInstance();
        if (matchTime != nullptr)
            *matchTime = instance.first->match_info.getMatchTime();

        instance.second.unlock();
        return 0; //HAL does not expect error status if parameters are nullptr
    }

    int FRC_NetworkCommunication_getJoystickAxes(uint8_t joystickNum, struct JoystickAxes_t* axes, uint8_t maxAxes){
        auto instance = hel::RoboRIOManager::getInstance();

        if(joystickNum >= hel::Joystick::MAX_JOYSTICK_COUNT){
            throw std::out_of_range("Exception: unexpected joysticks index (expected 0-" + std::to_string(hel::Joystick::MAX_JOYSTICK_COUNT) + " got " + std::to_string(joystickNum) + ")");
        }

        if(axes != nullptr){
            if(maxAxes != hel::Joystick::MAX_AXIS_COUNT){
                throw std::out_of_range("Exception: mismatch maximum axis count on joystick index " + std::to_string(joystickNum) + "(Expected " + std::to_string(hel::Joystick::MAX_AXIS_COUNT) + " got " + std::to_string(maxAxes) + "))");
            }
            hel::BoundsCheckedArray<int8_t, hel::Joystick::MAX_AXIS_COUNT> hel_axes = instance.first->joysticks[joystickNum].getAxes();
            std::copy(std::begin(hel_axes), std::end(hel_axes), axes->axes);
            axes->count = instance.first->joysticks[joystickNum].getAxisCount();
        }

        instance.second.unlock();
        return 0;
    }

    int FRC_NetworkCommunication_getJoystickButtons(uint8_t joystickNum, uint32_t* buttons, uint8_t* count){
        auto instance = hel::RoboRIOManager::getInstance();

        if(joystickNum >= hel::Joystick::MAX_JOYSTICK_COUNT){
            throw std::out_of_range("Exception: unexpected joysticks index (expected 0-" + std::to_string(hel::Joystick::MAX_JOYSTICK_COUNT) + " got " + std::to_string(joystickNum) + ")");
        }

        if (buttons != nullptr)
            *buttons = instance.first->joysticks[joystickNum].getButtons();
        if (count != nullptr)
            *count = instance.first->joysticks[joystickNum].getButtonCount();

        instance.second.unlock();
        return 0;
    }

    int FRC_NetworkCommunication_getJoystickPOVs(uint8_t joystickNum, struct JoystickPOV_t* povs, uint8_t maxPOVs){
        auto instance = hel::RoboRIOManager::getInstance();

        if(joystickNum >= hel::Joystick::MAX_JOYSTICK_COUNT){
            throw std::out_of_range("Exception: unexpected joysticks index (expected 0-" + std::to_string(hel::Joystick::MAX_JOYSTICK_COUNT) + " got " + std::to_string(joystickNum) + ")");
        }

        if(povs != nullptr){
            if(maxPOVs != hel::Joystick::MAX_POV_COUNT){
                throw std::out_of_range("Exception: mismatch maximum pov count on joystick index " + std::to_string(joystickNum) + "(Expected " + std::to_string(hel::Joystick::MAX_POV_COUNT) + " got " + std::to_string(maxPOVs) + "))");
            }
            hel::BoundsCheckedArray<int16_t, hel::Joystick::MAX_POV_COUNT> hel_povs = instance.first->joysticks[joystickNum].getPOVs();

            std::copy(std::begin(hel_povs), std::end(hel_povs), povs->povs);
            povs->count = instance.first->joysticks[joystickNum].getPOVCount();
        }
        instance.second.unlock();
        return 0;
    }

    int FRC_NetworkCommunication_setJoystickOutputs(uint8_t joystickNum, uint32_t hidOutputs, uint16_t leftRumble, uint16_t rightRumble){
        auto instance = hel::RoboRIOManager::getInstance();

        if(joystickNum >= hel::Joystick::MAX_JOYSTICK_COUNT){
            throw std::out_of_range("Exception: unexpected joysticks index (expected 0-" + std::to_string(hel::Joystick::MAX_JOYSTICK_COUNT) + " got " + std::to_string(joystickNum) + ")");
        }

        instance.first->joysticks[joystickNum].setOutputs(hidOutputs);
        instance.first->joysticks[joystickNum].setLeftRumble(leftRumble);
        instance.first->joysticks[joystickNum].setRightRumble(rightRumble);

        instance.second.unlock();
        return 0;
    }

    int FRC_NetworkCommunication_getJoystickDesc(uint8_t joystickNum, uint8_t* isXBox, uint8_t* type, char* name, uint8_t* axisCount, uint8_t* axisTypes, uint8_t* buttonCount, uint8_t* povCount){
        auto instance = hel::RoboRIOManager::getInstance();

        if(joystickNum >= hel::Joystick::MAX_JOYSTICK_COUNT){
            throw std::out_of_range("Exception: unexpected joysticks index (expected 0-" + std::to_string(hel::Joystick::MAX_JOYSTICK_COUNT) + " got " + std::to_string(joystickNum) + ")");
        }

        if(name != nullptr){
            hel::copystr(instance.first->joysticks[joystickNum].getName(), name);
        }
        if(isXBox != nullptr)
            *isXBox = instance.first->joysticks[joystickNum].getIsXBox();
        if(type != nullptr)
            *type = instance.first->joysticks[joystickNum].getType();
        if(axisCount)
            *axisCount = instance.first->joysticks[joystickNum].getAxisCount();

        if(axisTypes != nullptr){
            hel::BoundsCheckedArray<uint8_t, hel::Joystick::MAX_AXIS_COUNT> hel_axis_types = instance.first->joysticks[joystickNum].getAxisTypes();
            std::copy(std::begin(hel_axis_types), std::end(hel_axis_types), axisTypes);
        }

        if(buttonCount != nullptr)
            *buttonCount = instance.first->joysticks[joystickNum].getButtonCount();
        if(povCount != nullptr)
            *povCount = instance.first->joysticks[joystickNum].getPOVCount();

        instance.second.unlock();
        return 0; //HAL does not expect error status if parameters are nullptr
    }

    void FRC_NetworkCommunication_getVersionString(char* /*version*/){} //unnecessary for emulation

    int FRC_NetworkCommunication_observeUserProgramStarting(void){ //unnecessary for emulation
        return 0;
    }

    void FRC_NetworkCommunication_observeUserProgramDisabled(void){
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->robot_mode.setEnabled(false);

        instance.second.unlock();
    }

    void FRC_NetworkCommunication_observeUserProgramAutonomous(void){
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->robot_mode.setMode(hel::RobotMode::Mode::AUTONOMOUS);
        instance.first->robot_mode.setEnabled(true);

        instance.second.unlock();
    }

    void FRC_NetworkCommunication_observeUserProgramTeleop(void){
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->robot_mode.setMode(hel::RobotMode::Mode::TELEOPERATED);
        instance.first->robot_mode.setEnabled(true);

        instance.second.unlock();
    }

    void FRC_NetworkCommunication_observeUserProgramTest(void){
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->robot_mode.setMode(hel::RobotMode::Mode::TEST);
        instance.first->robot_mode.setEnabled(true);

        instance.second.unlock();
    }
}
