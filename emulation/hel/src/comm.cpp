#include "roborio.h"
#include "util.h"
#include "json_util.h"
using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    RoboRIO::RobotState::State RoboRIO::RobotState::getState()const{
    	return state;
    }

    void RoboRIO::RobotState::setState(RoboRIO::RobotState::State s){
    	state = s;
    }

    bool RoboRIO::RobotState::getEnabled()const{
    	return enabled;
    }

    void RoboRIO::RobotState::setEnabled(bool e){
    	enabled = e;
    }

    bool RoboRIO::RobotState::getEmergencyStopped()const{
        return emergency_stopped;
    }

    void RoboRIO::RobotState::setEmergencyStopped(bool e){
        emergency_stopped = e;
    }

    bool RoboRIO::RobotState::getFMSAttached()const{
        return fms_attached;
    }

    void RoboRIO::RobotState::setFMSAttached(bool attached){
        fms_attached = attached;
    }

    bool RoboRIO::RobotState::getDSAttached()const{
        return ds_attached;
    }

    void RoboRIO::RobotState::setDSAttached(bool attached){
        ds_attached = attached;
    }

    ControlWord_t RoboRIO::RobotState::toControlWord()const{
        ControlWord_t control_word;
        control_word.enabled = enabled;
        control_word.autonomous = state == State::AUTONOMOUS;
        control_word.test = state == State::TEST;
        control_word.eStop = emergency_stopped;
        control_word.fmsAttached = fms_attached;
        control_word.dsAttached = ds_attached;
        return control_word;
    }

    std::string RoboRIO::DriverStationInfo::getEventName()const{
    	return event_name;
    }

    void RoboRIO::DriverStationInfo::setEventName(std::string name){
    	event_name = name;
    }

    std::string RoboRIO::DriverStationInfo::getGameSpecificMessage()const{
    	return game_specific_message;
    }

    void RoboRIO::DriverStationInfo::setGameSpecificMessage(std::string message){
    	game_specific_message = message;
    }

    MatchType_t RoboRIO::DriverStationInfo::getMatchType()const{
    	return match_type;
    }

    void RoboRIO::DriverStationInfo::setMatchType(MatchType_t type){
    	match_type = type;
    }

    uint16_t RoboRIO::DriverStationInfo::getMatchNumber()const{
    	return match_number;
    }

    void RoboRIO::DriverStationInfo::setMatchNumber(uint16_t number){
    	match_number = number;
    }

    uint8_t RoboRIO::DriverStationInfo::getReplayNumber()const{
    	return replay_number;
    }

    void RoboRIO::DriverStationInfo::setReplayNumber(uint8_t number){
    	replay_number = number;
    }

    AllianceStationID_t RoboRIO::DriverStationInfo::getAllianceStationID()const{
    	return alliance_station_id;
    }

    void RoboRIO::DriverStationInfo::setAllianceStationID(AllianceStationID_t id){
    	alliance_station_id = id;
    }

    double RoboRIO::DriverStationInfo::getMatchTime()const{
    	return match_time;
    }

    void RoboRIO::DriverStationInfo::setMatchTime(double time){
    	match_time = time;
    }

    bool RoboRIO::Joystick::getIsXBox()const{
         return is_xbox;
    }

    void RoboRIO::Joystick::setIsXBox(bool xbox){
         is_xbox = xbox;
    }

    frc::GenericHID::HIDType RoboRIO::Joystick::getType()const{
         return type;
    }

    void RoboRIO::Joystick::setType(frc::GenericHID::HIDType t){
         type = t;
    }

    std::string RoboRIO::Joystick::getName()const{
         return name;
    }

    void RoboRIO::Joystick::setName(std::string n){
         name = n;
    }

    uint32_t RoboRIO::Joystick::getButtons()const{
         return buttons;
    }

    void RoboRIO::Joystick::setButtons(uint32_t b){
         buttons = b;
    }

    uint8_t RoboRIO::Joystick::getButtonCount()const{
         return button_count;
    }

    void RoboRIO::Joystick::setButtonCount(uint8_t count){
         button_count = count;
    }

    std::array<int8_t, RoboRIO::Joystick::MAX_AXIS_COUNT> RoboRIO::Joystick::getAxes()const{
         return axes;
    }

    void RoboRIO::Joystick::setAxes(std::array<int8_t, RoboRIO::Joystick::MAX_AXIS_COUNT> a){
         axes = a;
    }

    uint8_t RoboRIO::Joystick::getAxisCount()const{
         return axis_count;
    }

    void RoboRIO::Joystick::setAxisCount(uint8_t count){
         axis_count = count;
    }

    std::array<uint8_t, RoboRIO::Joystick::MAX_AXIS_COUNT> RoboRIO::Joystick::getAxisTypes()const{
         return axis_types;
    }

    void RoboRIO::Joystick::setAxisTypes(std::array<uint8_t, RoboRIO::Joystick::MAX_AXIS_COUNT> types){
         axis_types = types;
    }

    std::array<int16_t, RoboRIO::Joystick::MAX_POV_COUNT> RoboRIO::Joystick::getPOVs()const{
         return povs;
    }

    void RoboRIO::Joystick::setPOVs(std::array<int16_t, RoboRIO::Joystick::MAX_POV_COUNT> p){
         povs = p;
    }

    uint8_t RoboRIO::Joystick::getPOVCount()const{
         return pov_count;
    }

    void RoboRIO::Joystick::setPOVCount(uint8_t count){
         pov_count = count;
    }

    uint32_t RoboRIO::Joystick::getOutputs()const{
         return outputs;
    }

    void RoboRIO::Joystick::setOutputs(uint32_t outs){
         outputs = outs;
    }

    uint16_t RoboRIO::Joystick::getLeftRumble()const{
         return left_rumble;
    }

    void RoboRIO::Joystick::setLeftRumble(uint16_t rumble){
         left_rumble = rumble;
    }

    uint16_t RoboRIO::Joystick::getRightRumble()const{
         return right_rumble;
    }

    void RoboRIO::Joystick::setRightRumble(uint16_t rumble){
        right_rumble = rumble;
    }

    std::string RoboRIO::Joystick::serialize()const{
        std::string s = "\"Joystick:\": {";
        s += "\"is_xbox\":" + hel::to_string(is_xbox) + ", ";
        s += "\"type\":" + std::to_string(type) + ", ";
        s += "\"name\":" + name + ", ";
        s += "\"buttons\":" + std::to_string(buttons) + ", ";
        s += "\"button_count\":" + std::to_string((int)button_count) + ", ";
        s += hel::serializeList("\"axes\"", axes, std::function<std::string(int8_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += "\"axis_count\":" + std::to_string(axis_count) + ", ";
        s += hel::serializeList("\"axis_types\"", axis_types, std::function<std::string(uint8_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += hel::serializeList("\"povs\":", povs, std::function<std::string(int16_t)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
        s += "\"pov_count\":" + std::to_string(pov_count) +", ";
        s += "\"outputs\":" + std::to_string(outputs) + ", ";
        s += "\"left_rumble\":" + std::to_string(left_rumble) + ", ";
        s += "\"right_rumble\":" + std::to_string(right_rumble);
        s += "}";
        return s;
    }

    RoboRIO::Joystick RoboRIO::Joystick::deserialize(std::string s){
        RoboRIO::Joystick joy;
        joy.is_xbox = hel::stob(hel::pullValue("\"is_xbox\"", s));
        joy.type = static_cast<frc::GenericHID::HIDType>(std::stoi(hel::pullValue("\"type\"",s)));
        joy.name = hel::pullValue("\"name\"", s);
        joy.buttons = std::stoi(hel::pullValue("\"buttons\"", s));
        joy.button_count = std::stoi(hel::pullValue("\"button_count\"", s));
        std::vector<int8_t> axes_deserialized = hel::deserializeList(hel::pullValue("\"axes\"",s), std::function<int8_t(std::string)>([&](std::string s){ return std::stoi(s);}), true);
        if(axes_deserialized.size() == joy.axes.size()){
            std::copy(axes_deserialized.begin(), axes_deserialized.end(), joy.axes.begin());
        } else {
            //TODO error handling
        }
        joy.axis_count = std::stoi(hel::pullValue("\"axis_count\"", s));
        std::vector<uint8_t> axis_types_deserialized = hel::deserializeList(hel::pullValue("\"axis_types\"",s), std::function<uint8_t(std::string)>([&](std::string s){ return std::stoi(s);}), true);
        if(axis_types_deserialized.size() == joy.axis_types.size()){
            std::copy(axis_types_deserialized.begin(), axis_types_deserialized.end(), joy.axis_types.begin());
        } else {
            //TODO error handling
        }
        std::vector<int16_t> povs_deserialized = hel::deserializeList(hel::pullValue("\"povs\"",s), std::function<int16_t(std::string)>([&](std::string s){ return std::stoi(s);}), true);
        if(povs_deserialized.size() == joy.povs.size()){
            std::copy(povs_deserialized.begin(), povs_deserialized.end(), joy.povs.begin());
        } else {
            //TODO error handling
        }
        joy.pov_count = std::stoi(hel::pullValue("\"pov_count\"", s));
        joy.outputs = std::stoi(hel::pullValue("\"outputs\"", s));
        joy.left_rumble = std::stoi(hel::pullValue("\"left_rumble\"", s));
        joy.right_rumble = std::stoi(hel::pullValue("\"right_rumble\"", s));

        return joy;
    }
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
        instance.first->ds_errors.push_back({isError, errorCode, details, location, callStack}); //assuming isLVCode = false (not supporting LabView
        instance.second.unlock();
        return 0; //TODO retruns a status
    }

    void setNewDataSem(pthread_cond_t*){} //unnecessary for emulation

    int setNewDataOccurRef(uint32_t refnum){ 
        auto instance = hel::RoboRIOManager::getInstance();
        instance.first->net_comm.ref_num = refnum;

        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_getControlWord(struct ControlWord_t* controlWord){
        auto instance = hel::RoboRIOManager::getInstance();
        if (controlWord != nullptr) {
            *controlWord = instance.first->robot_state.toControlWord();
            controlWord->enabled = 1;
            controlWord->autonomous = 0;
            controlWord->test = 0;
            controlWord->dsAttached = 1;
            controlWord->eStop = 0;
        }

        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_getAllianceStation(enum AllianceStationID_t* allianceStation){
        auto instance = hel::RoboRIOManager::getInstance();
        if (allianceStation != nullptr)
            *allianceStation = instance.first->driver_station_info.getAllianceStationID();

        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_getMatchInfo(char* eventName, MatchType_t* matchType, uint16_t* matchNumber, uint8_t* replayNumber, uint8_t* gameSpecificMessage, uint16_t* gameSpecificMessageSize){
        auto instance = hel::RoboRIOManager::getInstance();
        std::string name = instance.first->driver_station_info.getEventName();
        std::copy(std::begin(name), std::end(name), eventName);


        if (matchType != nullptr)
            *matchType = instance.first->driver_station_info.getMatchType();
        if (matchNumber != nullptr)
            *matchNumber = instance.first->driver_station_info.getMatchNumber();
        if (replayNumber != nullptr)
            *replayNumber = instance.first->driver_station_info.getReplayNumber();

        std::string hel_message = instance.first->driver_station_info.getGameSpecificMessage();
        std::copy(std::begin(hel_message), std::end(hel_message), gameSpecificMessage);

        if (gameSpecificMessageSize != nullptr)
            *gameSpecificMessageSize = instance.first->driver_station_info.getGameSpecificMessage().size();

        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_getMatchTime(float* matchTime){
        auto instance = hel::RoboRIOManager::getInstance();
        if (matchTime != nullptr)
            *matchTime = instance.first->driver_station_info.getMatchTime();

        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_getJoystickAxes(uint8_t joystickNum, struct JoystickAxes_t* axes, uint8_t /*maxAxes*/){
        auto instance = hel::RoboRIOManager::getInstance();
        if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
            //TODO error handling
        }

        std::array<int8_t, hel::RoboRIO::Joystick::MAX_AXIS_COUNT> hel_axes = instance.first->joysticks[joystickNum].getAxes();
        std::copy(std::begin(hel_axes), std::end(hel_axes), axes->axes);

        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_getJoystickButtons(uint8_t joystickNum, uint32_t* buttons, uint8_t* count){
        auto instance = hel::RoboRIOManager::getInstance();
        if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
            //TODO error handling
        }

        if (buttons != nullptr)
            *buttons = instance.first->joysticks[joystickNum].getButtons();
        if (count != nullptr)
            *count = instance.first->joysticks[joystickNum].getButtonCount();
    
        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_getJoystickPOVs(uint8_t joystickNum, struct JoystickPOV_t* povs, uint8_t /*maxPOVs*/){
        auto instance = hel::RoboRIOManager::getInstance();
        if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
            //TODO error handling
        }

        std::array<int16_t, hel::RoboRIO::Joystick::MAX_POV_COUNT> hel_povs = instance.first->joysticks[joystickNum].getPOVs();
        std::copy(std::begin(hel_povs), std::end(hel_povs), povs->povs);
    
        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_setJoystickOutputs(uint8_t joystickNum, uint32_t hidOutputs, uint16_t leftRumble, uint16_t rightRumble){
        auto instance = hel::RoboRIOManager::getInstance();
        if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
            //TODO error handling
        }

        instance.first->joysticks[joystickNum].setOutputs(hidOutputs);
        instance.first->joysticks[joystickNum].setLeftRumble(leftRumble);
        instance.first->joysticks[joystickNum].setRightRumble(rightRumble);

        instance.second.unlock();
        return 0; //TODO returns a status
    }

    int FRC_NetworkCommunication_getJoystickDesc(uint8_t joystickNum, uint8_t* isXBox, uint8_t* type, char* name, uint8_t* axisCount, uint8_t* axisTypes, uint8_t* buttonCount, uint8_t* povCount){
        auto instance = hel::RoboRIOManager::getInstance();

        if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
            //TODO error handling
        }
        std::string hel_name = instance.first->joysticks[joystickNum].getName();
        std::copy(std::begin(hel_name), std::end(hel_name), name);

        if(isXBox != nullptr)
            *isXBox = instance.first->joysticks[joystickNum].getIsXBox();
        if(type != nullptr)
            *type = instance.first->joysticks[joystickNum].getType();
        if(axisCount)
            *axisCount = instance.first->joysticks[joystickNum].getAxisCount();
    
        std::array<uint8_t, hel::RoboRIO::Joystick::MAX_AXIS_COUNT> hel_axis_types = instance.first->joysticks[joystickNum].getAxisTypes();
        std::copy(std::begin(hel_axis_types), std::end(hel_axis_types), axisTypes);
    
        if(buttonCount != nullptr)
            *buttonCount = instance.first->joysticks[joystickNum].getButtonCount();
        if(povCount != nullptr)
            *povCount = instance.first->joysticks[joystickNum].getPOVCount();

        instance.second.unlock();
        return 0; //TODO returns a status
    }

    void FRC_NetworkCommunication_getVersionString(char* /*version*/){} //unnecessary for emulation

    int FRC_NetworkCommunication_observeUserProgramStarting(void){ //unnecessary for emulation
        return 0;
    }

    void FRC_NetworkCommunication_observeUserProgramDisabled(void){
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->robot_state.setEnabled(false);

        instance.second.unlock();
    }

    void FRC_NetworkCommunication_observeUserProgramAutonomous(void){
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->robot_state.setState(hel::RoboRIO::RobotState::State::AUTONOMOUS);
        instance.first->robot_state.setEnabled(true);

        instance.second.unlock();
    }

    void FRC_NetworkCommunication_observeUserProgramTeleop(void){
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->robot_state.setState(hel::RoboRIO::RobotState::State::TELEOPERATED);
        instance.first->robot_state.setEnabled(true);

        instance.second.unlock();
    }

    void FRC_NetworkCommunication_observeUserProgramTest(void){
        auto instance = hel::RoboRIOManager::getInstance();

        instance.first->robot_state.setState(hel::RoboRIO::RobotState::State::TEST);
        instance.first->robot_state.setEnabled(true);

        instance.second.unlock();
    }
}
