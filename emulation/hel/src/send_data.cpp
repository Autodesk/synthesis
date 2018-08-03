#include "roborio_manager.hpp"
#include "util.hpp"
#include "json_util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

hel::SendData::SendData():serialized_data(""),new_data(true),pwm_hdrs(0.0), relays(RelayState::OFF), analog_outputs(0.0), digital_mxp({}), digital_hdrs(false), can_motor_controllers({}){}

hel::SendData::RelayState hel::SendData::convertRelayValue(nFPGA::nRoboRIO_FPGANamespace::tRelay::tValue value, uint8_t i)noexcept{
	bool forward = checkBitHigh(value.Forward, i);
	bool reverse  = checkBitHigh(value.Reverse, i);
	if(forward){
		if(reverse){
			return RelayState::ERROR;
		}
		return RelayState::FORWARD;
	}
	if(reverse){
		return RelayState::REVERSE;
	}
	return RelayState::OFF;
}

bool hel::SendData::hasNewData()const{
    return new_data;
}

void hel::SendData::updateShallow(){
    if(!hel::hal_is_initialized){
        return;
    }

    RoboRIO roborio = RoboRIOManager::getCopy();

    for(unsigned i = 0; i < pwm_hdrs.size(); i++){
        pwm_hdrs[i] = PWMSystem::getSpeed(roborio.pwm_system.getHdrPulseWidth(i));
    }

    for(unsigned i = 0; i < digital_mxp.size(); i++){
        digital_mxp[i].config = DigitalSystem::toMXPConfig(roborio.digital_system.getEnabledOutputs().MXP, roborio.digital_system.getMXPSpecialFunctionsEnabled(), i);

        switch(digital_mxp[i].config){
        case hel::MXPData::Config::DO:
            digital_mxp[i].value = (checkBitHigh(roborio.digital_system.getOutputs().MXP, i) | checkBitHigh(roborio.digital_system.getPulses().MXP, i));
            break;
        case hel::MXPData::Config::PWM:
            {
                int remapped_i = i;
                if(remapped_i >= 4){ //digital ports 0-3 line up with mxp pwm ports 0-3, the rest are offset by 4
                    remapped_i -= 4;
                }
                digital_mxp[i].value = PWMSystem::getSpeed(roborio.pwm_system.getMXPPulseWidth(remapped_i));
            }
            break;
        case hel::MXPData::Config::SPI:
        case hel::MXPData::Config::I2C:
        default:
            break; //do nothing
        }
    }
    can_motor_controllers = roborio.can_motor_controllers;
    new_data = true;
}

void hel::SendData::updateDeep(){
    if(!hel::hal_is_initialized){
        return;
    }

	updateShallow();

    RoboRIO roborio = RoboRIOManager::getCopy();

    for(unsigned i = 0; i < relays.size(); i++){
        relays[i] = convertRelayValue(roborio.relay_system.getValue(),i);
    }
    for(unsigned i = 0; i < analog_outputs.size(); i++){
        analog_outputs[i] = (roborio.analog_outputs.getMXPOutput(i)) * 5. / 0x1000;
    }
    {
        tDIO::tOutputEnable output_mode = roborio.digital_system.getEnabledOutputs();
        auto values = roborio.digital_system.getOutputs().Headers;
        auto pulses = roborio.digital_system.getPulses().Headers;
        for(unsigned i = 0; i < digital_hdrs.size(); i++){
            if(checkBitHigh(output_mode.Headers, i)){
                digital_hdrs[i] = (checkBitHigh(values, i) | checkBitHigh(pulses, i));
            }
        }
    }
    can_motor_controllers = roborio.can_motor_controllers;
    new_data = true;
}

std::string hel::to_string(hel::SendData::RelayState r){
    switch(r){
    case hel::SendData::RelayState::OFF:
        return "OFF";
    case hel::SendData::RelayState::REVERSE:
        return "REVERSE";
    case hel::SendData::RelayState::FORWARD:
        return "FORWARD";
    case hel::SendData::RelayState::ERROR:
        return "ERROR";
    default:
        throw UnhandledEnumConstantException("hel::SendData::RelayState");
    }
}

std::string hel::SendData::toString()const{
    std::string s = "(";
    s += "pwm_hdrs:" + hel::to_string(pwm_hdrs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string))) + ", ";
    s += "relays:" + hel::to_string(relays, std::function<std::string(hel::SendData::RelayState)>(static_cast<std::string(*)(hel::SendData::RelayState)>(hel::to_string)));
    s += "analog_outputs:" + hel::to_string(analog_outputs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
    s += "digital_mxp:" + hel::to_string(digital_mxp, std::function<std::string(MXPData)>(&MXPData::toString)) + ", ";
    s += "digital_hdrs:" + hel::to_string(digital_hdrs, std::function<std::string(bool)>(static_cast<std::string(*)(int)>(std::to_string))) + ", ";
    s += "can_motor_controllers:" + hel::to_string(can_motor_controllers, std::function<std::string(std::pair<uint32_t,CANMotorController>)>([&](std::pair<uint32_t, CANMotorController> a){ return "[" + std::to_string(a.first) + ", " + a.second.toString() + "]";}));
    s += ")";
    return s;
}

void hel::SendData::serializePWMHdrs(){
    serialized_data += serializeList("\"pwm_hdrs\"", pwm_hdrs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
}

void hel::SendData::serializeRelays(){
    serialized_data += serializeList(
        "\"relays\"",
        relays,
        std::function<std::string(RelayState)>([&](RelayState r){
            return hel::quote(hel::to_string(r));
        })
	);
}

void hel::SendData::serializeAnalogOutputs(){
    serialized_data += serializeList("\"analog_outputs\"", analog_outputs, std::function<std::string(double)>(static_cast<std::string(*)(double)>(std::to_string)));
}

void hel::SendData::serializeDigitalMXP(){
    serialized_data += serializeList(
        "\"digital_mxp\"",
        digital_mxp,
        std::function<std::string(hel::MXPData)>(&MXPData::serialize)
	);
}

void hel::SendData::serializeDigitalHdrs(){
    serialized_data += serializeList(
        "\"digital_hdrs\"",
        digital_hdrs,
        std::function<std::string(bool)>(static_cast<std::string(*)(bool)>(hel::to_string))
    );
}

void hel::SendData::serializeCANMotorControllers(){
    serialized_data += serializeList(
        "\"can_motor_controllers\"",
        can_motor_controllers,
        std::function<std::string(std::pair<uint32_t,CANMotorController>)>([&](std::pair<uint32_t, CANMotorController> a){
            return a.second.serialize();
        })
    );
}

std::string hel::SendData::serializeShallow(){
    if(!new_data){
        return serialized_data;
    }

    serialized_data = "{\"roborio\":{";
	serializePWMHdrs();
	serialized_data += ",";
	serializeCANMotorControllers();
    serialized_data += "}}";
    serialized_data += JSON_PACKET_SUFFIX;
    new_data = false;
    return serialized_data;
}

std::string hel::SendData::serializeDeep(){
    if(!new_data){
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
    new_data = false;
    return serialized_data;
}
