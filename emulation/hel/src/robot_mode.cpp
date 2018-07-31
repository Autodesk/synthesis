#include "robot_mode.hpp"

#include "error.hpp"
#include "json_util.hpp"
#include "util.hpp"

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

    std::string to_string(RobotMode::Mode mode){
        switch(mode){
        case RobotMode::Mode::AUTONOMOUS:
            return "AUTONOMOUS";
        case RobotMode::Mode::TELEOPERATED:
            return "TELEOPERATED";
        case RobotMode::Mode::TEST:
            return "TEST";
        default:
            throw UnhandledEnumConstantException("hel::RobotMode::Mode");
        }
    }

    RobotMode::Mode s_to_robot_mode(std::string s){
        switch(hasher(s.c_str())){
        case hasher("AUTONOMOUS"):
            return RobotMode::Mode::AUTONOMOUS;
        case hasher("TELEOPERATED"):
            return RobotMode::Mode::TELEOPERATED;
        case hasher("TEST"):
            return RobotMode::Mode::TEST;
        default:
            throw UnhandledCase();
        }
    }

    RobotMode RobotMode::deserialize(std::string s){
        RobotMode a;
        a.mode = s_to_robot_mode(unquote(hel::pullValue("\"mode\"",s)));
        a.enabled = hel::stob(hel::pullValue("\"enabled\"",s));
        a.emergency_stopped = hel::stob(hel::pullValue("\"emergency_stopped\"",s));
        a.fms_attached = hel::stob(hel::pullValue("\"fms_attached\"",s));
        a.ds_attached = hel::stob(hel::pullValue("\"ds_attached\"",s));
        return a;
    }

    std::string RobotMode::serialize()const{
        std::string s = "{";
        s += "\"mode\":" + quote(hel::to_string(mode)) + ", ";
        s += "\"enabled\":" + hel::to_string(enabled) + ", ";
        s += "\"emergency_stopped\":" + hel::to_string(emergency_stopped) + ", ";
        s += "\"fms_attached\":" + hel::to_string(fms_attached) + ", ";
        s += "\"ds_attached\":" + hel::to_string(ds_attached);
        s += "}";
        return s;
    }

    std::string RobotMode::toString()const{
        std::string s = "{";
        s += "mode:" + hel::to_string(mode) + ", ";
        s += "enabled:" + hel::to_string(enabled) + ", ";
        s += "emergency_stopped:" + hel::to_string(emergency_stopped) + ", ";
        s += "fms_attached:" + hel::to_string(fms_attached) + ", ";
        s += "ds_attached:" + hel::to_string(ds_attached);
        s += "}";
        return s;
    }

    RobotMode::RobotMode():mode(RobotMode::Mode::TELEOPERATED),enabled(false),emergency_stopped(false),fms_attached(false),ds_attached(true){}
}
