#include "robot_mode.hpp"

#include "error.hpp"
#include "json_util.hpp"
#include "send_data.hpp"
#include "util.hpp"

namespace hel{
    RobotMode::Mode RobotMode::getMode()const noexcept{
        return mode;
    }

    void RobotMode::setMode(RobotMode::Mode m)noexcept{
        mode = m;
    }

    bool RobotMode::getEnabled()const noexcept{
        return enabled;
    }

    void RobotMode::setEnabled(bool e)noexcept{
        enabled = e;
        auto instance = SendDataManager::getInstance();
        instance.first->enable(e);
        instance.second.unlock();
    }

    bool RobotMode::getEmergencyStopped()const noexcept{
        return emergency_stopped;
    }

    void RobotMode::setEmergencyStopped(bool e)noexcept{
        emergency_stopped = e;
    }

    bool RobotMode::getFMSAttached()const noexcept{
        return fms_attached;
    }

    void RobotMode::setFMSAttached(bool attached)noexcept{
        fms_attached = attached;
    }

    bool RobotMode::getDSAttached()const noexcept{
        return ds_attached;
    }

    void RobotMode::setDSAttached(bool attached)noexcept{
        ds_attached = attached;
    }

    ControlWord_t RobotMode::toControlWord()const noexcept{
        ControlWord_t control_word;
        control_word.enabled = enabled;
        control_word.autonomous = mode == Mode::AUTONOMOUS;
        control_word.test = mode == Mode::TEST;
        control_word.eStop = emergency_stopped;
        control_word.fmsAttached = fms_attached;
        control_word.dsAttached = ds_attached;
        //controlWord->autonomous = instance.first->robot_mode.getMode()==hel::RobotMode::Mode::AUTONOMOUS?1:0;
        //controlWord->test = instance.first->robot_mode.getMode()==hel::RobotMode::Mode::TEST?1:0;
        return control_word;
    }

    std::string as_string(RobotMode::Mode mode){
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
        a.mode = s_to_robot_mode(unquote(hel::pullObject("\"mode\"",s)));
        a.enabled = hel::stob(hel::pullObject("\"enabled\"",s));
        a.emergency_stopped = hel::stob(hel::pullObject("\"emergency_stopped\"",s));
        a.fms_attached = hel::stob(hel::pullObject("\"fms_attached\"",s));
        a.ds_attached = hel::stob(hel::pullObject("\"ds_attached\"",s));
        return a;
    }

    std::string RobotMode::serialize()const{
        std::string s = "{";
        s += "\"mode\":" + quote(as_string(mode)) + ", ";
        s += "\"enabled\":" + as_string(enabled) + ", ";
        s += "\"emergency_stopped\":" + as_string(emergency_stopped) + ", ";
        s += "\"fms_attached\":" + as_string(fms_attached) + ", ";
        s += "\"ds_attached\":" + as_string(ds_attached);
        s += "}";
        return s;
    }

    std::string RobotMode::toString()const{
        std::string s = "(";
        s += "mode:" + as_string(mode) + ", ";
        s += "enabled:" + as_string(enabled) + ", ";
        s += "emergency_stopped:" + as_string(emergency_stopped) + ", ";
        s += "fms_attached:" + as_string(fms_attached) + ", ";
        s += "ds_attached:" + as_string(ds_attached);
        s += ")";
        return s;
    }

    RobotMode::RobotMode()noexcept:mode(RobotMode::Mode::TELEOPERATED),enabled(true),emergency_stopped(false),fms_attached(false),ds_attached(true){}
    RobotMode::RobotMode(const RobotMode& source)noexcept{
#define COPY(NAME) NAME = source.NAME
        COPY(mode);
        COPY(enabled);
        COPY(emergency_stopped);
        COPY(fms_attached);
        COPY(ds_attached);
#undef COPY
    }
}
