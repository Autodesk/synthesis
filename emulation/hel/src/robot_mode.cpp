#include "robot_mode.hpp"

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

    RobotMode::RobotMode():mode(RobotMode::Mode::TELEOPERATED),enabled(false),emergency_stopped(false),fms_attached(false),ds_attached(true){}
}
