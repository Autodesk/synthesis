#include "roborio_manager.hpp"
#include "util.hpp"
#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel{
    constexpr char ZEROED_SERIALIZATION_DATA[] = "{\"roborio\":{\"pwm_hdrs\":[0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000],\"relays\":[\"OFF\",\"OFF\",\"OFF\",\"OFF\"],\"analog_outputs\":[0.000000,0.000000],\"digital_mxp\":[{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000},{\"config\":\"DI\", \"value\":0.000000}],\"digital_hdrs\":[0,0,0,0,0,0,0,0,0,0],\"can_motor_controllers\":[]}}\x1B"; //TODO replace with shallow and deep versions

    SendData::SendData():serialized_data(""),new_data(true),enabled(RobotMode::DEFAULT_ENABLED_STATUS),pwm_hdrs(0.0), relays(RelaySystem::State::OFF), analog_outputs(0.0), digital_mxp({}), digital_hdrs(false), can_motor_controllers({}){}

    bool SendData::hasNewData()const{
        return new_data;
    }

    void SendData::updateShallow(){
        if(!hal_is_initialized){
            return;
        }

        RoboRIO roborio = RoboRIOManager::getCopy();

        for(unsigned i = 0; i < pwm_hdrs.size(); i++){
            pwm_hdrs[i] = roborio.pwm_system.getHdrZeroLatch(i) ? 0.0 : PWMSystem::getPercentOutput(roborio.pwm_system.getHdrPulseWidth(i));
        }

        for(unsigned i = 0; i < digital_mxp.size(); i++){
            digital_mxp[i].config = DigitalSystem::toMXPConfig(roborio.digital_system.getEnabledOutputs().MXP, roborio.digital_system.getMXPSpecialFunctionsEnabled(), i);

            switch(digital_mxp[i].config){
            case MXPData::Config::DO:
                digital_mxp[i].value = (checkBitHigh(roborio.digital_system.getOutputs().MXP, i) | checkBitHigh(roborio.digital_system.getPulses().MXP, i));
                break;
            case MXPData::Config::PWM:
            {
                int remapped_i = i;
                if(remapped_i >= 4){ //digital ports 0-3 line up with mxp pwm ports 0-3, the rest are offset by 4
                    remapped_i -= 4;
                }
                digital_mxp[i].value = roborio.pwm_system.getMXPZeroLatch(remapped_i) ? 0.0 : PWMSystem::getPercentOutput(roborio.pwm_system.getMXPPulseWidth(remapped_i));
                break;
            }
            case MXPData::Config::SPI:
            case MXPData::Config::I2C:
            default:
                break; //do nothing
            }
        }
        can_motor_controllers = roborio.can_motor_controllers;
        new_data = true;
    }

    void SendData::updateDeep(){
        if(!hal_is_initialized){
            return;
        }

        updateShallow();

        RoboRIO roborio = RoboRIOManager::getCopy();

        for(unsigned i = 0; i < relays.size(); i++){
            relays[i] = roborio.relay_system.getState(i);
        }
        for(unsigned i = 0; i < analog_outputs.size(); i++){
            analog_outputs[i] = (roborio.analog_outputs.getMXPOutput(i)) * 5.0 / 0x1000;
        }
        {
            tDIO::tOutputEnable output_mode = roborio.digital_system.getEnabledOutputs();
            auto values = roborio.digital_system.getOutputs().Headers;
            auto pulses = roborio.digital_system.getPulses().Headers;
            for(unsigned i = 0; i < digital_hdrs.size(); i++){
                if(checkBitHigh(output_mode.Headers, i)){ //if digital port is set for output, then set digital output
                    digital_hdrs[i] = (checkBitHigh(values, i) | checkBitHigh(pulses, i));
                }
            }
        }
        can_motor_controllers = roborio.can_motor_controllers;
        new_data = true;
    }

    std::string SendData::toString()const{
        std::string s = "(";
        s += "pwm_hdrs:" + asString(pwm_hdrs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string))) + ", ";
        s += "relays:" + asString(relays, std::function<std::string(RelaySystem::State)>(static_cast<std::string(*)(RelaySystem::State)>(asString))) + ", ";
        s += "analog_outputs:" + asString(analog_outputs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string))) + ", ";
        s += "digital_mxp:" + asString(digital_mxp, std::function<std::string(MXPData)>(&MXPData::toString)) + ", ";
        s += "digital_hdrs:" + asString(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(bool)>(asString))) + ", ";
        s += "can_motor_controllers:" + asString(can_motor_controllers, std::function<std::string(std::pair<uint32_t,std::shared_ptr<CANMotorControllerBase>>)>([&](std::pair<uint32_t, std::shared_ptr<CANMotorControllerBase>> a){ return "[" + std::to_string(a.first) + ", " + a.second->toString() + "]";}));
        s += ")";
        return s;
    }

    void SendData::serializePWMHdrs(){
        serialized_data += serializeList("\"pwm_hdrs\"", pwm_hdrs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
    }

    void SendData::serializeRelays(){
        serialized_data += serializeList(
            "\"relays\"",
            relays,
            std::function<std::string(RelaySystem::State)>([&](RelaySystem::State r){
                                                               return quote(asString(r));
                                                           })
            );
    }

    void SendData::serializeAnalogOutputs(){
        serialized_data += serializeList("\"analog_outputs\"", analog_outputs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
    }

    void SendData::serializeDigitalMXP(){
        serialized_data += serializeList(
            "\"digital_mxp\"",
            digital_mxp,
            std::function<std::string(MXPData)>(&MXPData::serialize)
            );
    }

    void SendData::serializeDigitalHdrs(){
        serialized_data += serializeList(
            "\"digital_hdrs\"",
            digital_hdrs,
            std::function<std::string(bool)>(static_cast<std::string(*)(bool)>(asString))
            );
    }

    void SendData::serializeCANMotorControllers(){
        serialized_data += serializeList(
            "\"can_motor_controllers\"",
            can_motor_controllers,
            std::function<std::string(std::pair<uint32_t,std::shared_ptr<CANMotorControllerBase>>)>([&](std::pair<uint32_t, std::shared_ptr<CANMotorControllerBase>> a){
                                                                                   return a.second->serialize();
                                                                               })
            );
    }

    std::string SendData::serializeShallow(){
        if(!new_data){
            return serialized_data;
        }
        new_data = false;
        if(!enabled){
            serialized_data = std::string(ZEROED_SERIALIZATION_DATA);
            return serialized_data;
        }

        serialized_data = "{\"roborio\":{";
        serializePWMHdrs();
        serialized_data += ",";
        serializeCANMotorControllers();
        serialized_data += "}}";
        serialized_data += JSON_PACKET_SUFFIX;
        return serialized_data;
    }

    std::string SendData::serializeDeep(){
        if(!new_data){
            return serialized_data;
        }
        new_data = false;
        if(!enabled){
            serialized_data = std::string(ZEROED_SERIALIZATION_DATA);
            return serialized_data;
        }

        serialized_data = "{\"roborio\":{";
        serializePWMHdrs();
        serialized_data += ",";
        serializeRelays();
        serialized_data += ",";
        serializeAnalogOutputs();
        serialized_data += ",";
        serializeDigitalMXP();
        serialized_data += ",";
        serializeDigitalHdrs();
        serialized_data += ",";
        serializeCANMotorControllers();
        serialized_data += "}}";
        serialized_data += JSON_PACKET_SUFFIX;
        return serialized_data;
    }

    void SendData::enable(bool e){
        if(e != enabled){
            new_data = true;
            enabled = e;
        }
    }
}

