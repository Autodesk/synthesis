#include "roborio.h"

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
}

int FRC_NetworkCommunication_Reserve(void* /*instance*/){ //unnecessary for emulation
    return 0;
}

int FRC_NetworkCommunication_sendConsoleLine(const char* /*line*/){ //unnecessary for emulation
    return 0;
}

int FRC_NetworkCommunication_sendError(int isError, int32_t errorCode, int /*isLVCode*/, const char* details, const char* location, const char* callStack){
    hel::RoboRIOManager::getInstance()->ds_errors.push_back({isError, errorCode, details, location, callStack}); //assuming isLVCode = false (not supporting LabView
    return 0; //TODO retruns a status
}

void setNewDataSem(pthread_cond_t*){} //unnecessary for emulation

int setNewDataOccurRef(uint32_t /*refnum*/){ //unnecessary for emulation
    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getControlWord(struct ControlWord_t* controlWord){
    *controlWord = hel::RoboRIOManager::getInstance()->robot_state.toControlWord();
    controlWord->enabled = 1;
    controlWord->autonomous = 0;
    controlWord->test = 0;
    controlWord->dsAttached = 1;
    controlWord->eStop = 0;
    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getAllianceStation(enum AllianceStationID_t* allianceStation){
    *allianceStation = hel::RoboRIOManager::getInstance()->driver_station_info.getAllianceStationID();
    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getMatchInfo(char* eventName, MatchType_t* matchType, uint16_t* matchNumber, uint8_t* replayNumber, uint8_t* gameSpecificMessage, uint16_t* gameSpecificMessageSize){
    std::string name = hel::RoboRIOManager::getInstance()->driver_station_info.getEventName();
    std::copy(std::begin(name), std::end(name), eventName);

    *matchType = hel::RoboRIOManager::getInstance()->driver_station_info.getMatchType();
    *matchNumber = hel::RoboRIOManager::getInstance()->driver_station_info.getMatchNumber();
    *replayNumber = hel::RoboRIOManager::getInstance()->driver_station_info.getReplayNumber();

    std::string hel_message = hel::RoboRIOManager::getInstance()->driver_station_info.getGameSpecificMessage();
    std::copy(std::begin(hel_message), std::end(hel_message), gameSpecificMessage);

    *gameSpecificMessageSize = hel::RoboRIOManager::getInstance()->driver_station_info.getGameSpecificMessage().size();

    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getMatchTime(float* matchTime){
    *matchTime = hel::RoboRIOManager::getInstance()->driver_station_info.getMatchTime();
    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getJoystickAxes(uint8_t joystickNum, struct JoystickAxes_t* axes, uint8_t /*maxAxes*/){
    if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
        //TODO error handling
    }

    std::array<int8_t, hel::RoboRIO::Joystick::MAX_AXIS_COUNT> hel_axes = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getAxes();
    std::copy(std::begin(hel_axes), std::end(hel_axes), axes->axes);
    
    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getJoystickButtons(uint8_t joystickNum, uint32_t* buttons, uint8_t* count){
    if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
        //TODO error handling
    }

    *buttons = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getButtons();
    *count = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getButtonCount();
    
    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getJoystickPOVs(uint8_t joystickNum, struct JoystickPOV_t* povs, uint8_t /*maxPOVs*/){
    if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
        //TODO error handling
    }

    std::array<int16_t, hel::RoboRIO::Joystick::MAX_POV_COUNT> hel_povs = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getPOVs();
    std::copy(std::begin(hel_povs), std::end(hel_povs), povs->povs);
    
    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_setJoystickOutputs(uint8_t joystickNum, uint32_t hidOutputs, uint16_t leftRumble, uint16_t rightRumble){
    if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
        //TODO error handling
    }

    hel::RoboRIOManager::getInstance()->joysticks[joystickNum].setOutputs(hidOutputs);
    hel::RoboRIOManager::getInstance()->joysticks[joystickNum].setLeftRumble(leftRumble);
    hel::RoboRIOManager::getInstance()->joysticks[joystickNum].setRightRumble(rightRumble);

    return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getJoystickDesc(uint8_t joystickNum, uint8_t* isXBox, uint8_t* type, char* name, uint8_t* axisCount, uint8_t* axisTypes, uint8_t* buttonCount, uint8_t* povCount){
    if(joystickNum <= hel::RoboRIO::Joystick::MAX_JOYSTICK_COUNT){
        //TODO error handling
    }
    std::string hel_name = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getName();
    std::copy(std::begin(hel_name), std::end(hel_name), name);

    *isXBox = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getIsXBox();
    *type = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getType();
    *axisCount = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getAxisCount();
    
    std::array<uint8_t, hel::RoboRIO::Joystick::MAX_AXIS_COUNT> hel_axis_types = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getAxisTypes();
    std::copy(std::begin(hel_axis_types), std::end(hel_axis_types), axisTypes);
    
    *buttonCount = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getButtonCount();
    *povCount = hel::RoboRIOManager::getInstance()->joysticks[joystickNum].getPOVCount();

    return 0; //TODO returns a status
}

void FRC_NetworkCommunication_getVersionString(char* /*version*/){} //unnecessary for emulation

int FRC_NetworkCommunication_observeUserProgramStarting(void){ //unnecessary for emulation
    return 0;
}

void FRC_NetworkCommunication_observeUserProgramDisabled(void){
    hel::RoboRIOManager::getInstance()->robot_state.setEnabled(false);
}

void FRC_NetworkCommunication_observeUserProgramAutonomous(void){
    hel::RoboRIOManager::getInstance()->robot_state.setState(hel::RoboRIO::RobotState::State::AUTONOMOUS);
    hel::RoboRIOManager::getInstance()->robot_state.setEnabled(true);
}

void FRC_NetworkCommunication_observeUserProgramTeleop(void){
    hel::RoboRIOManager::getInstance()->robot_state.setState(hel::RoboRIO::RobotState::State::TELEOPERATED);
    hel::RoboRIOManager::getInstance()->robot_state.setEnabled(true);
}

void FRC_NetworkCommunication_observeUserProgramTest(void){
    hel::RoboRIOManager::getInstance()->robot_state.setState(hel::RoboRIO::RobotState::State::TEST);
    hel::RoboRIOManager::getInstance()->robot_state.setEnabled(true);
}
