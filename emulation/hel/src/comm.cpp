#include "roborio_manager.hpp"
using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

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

    int64_t FRC_NetworkCommunication_nUsageReporting_report(int32_t, int32_t, int32_t, const char *){ //unnecessary for emulation
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
        }
        controlWord->enabled = instance.first->robot_mode.getEnabled();
        controlWord->autonomous = instance.first->robot_mode.getMode()==hel::RobotMode::Mode::AUTONOMOUS?1:0;
        controlWord->test = instance.first->robot_mode.getMode()==hel::RobotMode::Mode::TEST?1:0;
        std::cout<< controlWord->autonomous << controlWord->test << "\n";
        controlWord->dsAttached = instance.first->robot_mode.getDSAttached();
        controlWord->eStop = 0;

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
