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
}

int FRC_NetworkCommunication_Reserve(void* /*instance*/){ //unnecessary for emulation
	return 0;
}

int FRC_NetworkCommunication_sendConsoleLine(const char* /*line*/){ //unnecessary for emulation
	return 0;
}

int FRC_NetworkCommunication_sendError(int isError, int32_t errorCode, int isLVCode, const char* details, const char* location, const char* callStack){
	//TODO
}

void setNewDataSem(pthread_cond_t*){} //unnecessary for emulation

int setNewDataOccurRef(uint32_t /*refnum*/){} //unnecessary for emulation

int FRC_NetworkCommunication_getControlWord(struct ControlWord_t* controlWord){
	//TODO
}

int FRC_NetworkCommunication_getAllianceStation(enum AllianceStationID_t* allianceStation){
	//TODO
}

int FRC_NetworkCommunication_getMatchInfo(char* eventName, MatchType_t* matchType, uint16_t* matchNumber, uint8_t* replayNumber, uint8_t* gameSpecificMessage, uint16_t* gameSpecificMessageSize){
	std::string name = hel::RoboRIOManager::getInstance()->driver_station_info.getEventName();
	std::copy(std::begin(name), std::end(name), eventName);
	return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getMatchTime(float* matchTime){
	*matchTime = hel::RoboRIOManager::getInstance()->driver_station_info.getMatchTime();
	return 0; //TODO returns a status
}

int FRC_NetworkCommunication_getJoystickAxes(uint8_t joystickNum, struct JoystickAxes_t* axes, uint8_t maxAxes){
	//TODO
}

int FRC_NetworkCommunication_getJoystickButtons(uint8_t joystickNum, uint32_t* buttons, uint8_t* count){
	//TODO
}

int FRC_NetworkCommunication_getJoystickPOVs(uint8_t joystickNum, struct JoystickPOV_t* povs, uint8_t maxPOVs){
	//TODO
}

int FRC_NetworkCommunication_setJoystickOutputs(uint8_t joystickNum, uint32_t hidOutputs, uint16_t leftRumble, uint16_t rightRumble){
	//TODO
}

int FRC_NetworkCommunication_getJoystickDesc(uint8_t joystickNum, uint8_t* isXBox, uint8_t* type, char* name, uint8_t* axisCount, uint8_t* axisTypes, uint8_t* buttonCount, uint8_t* povCount){
	//TODO
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
