#include "robot_outputs.hpp"
#include "roborio_manager.hpp"
#include "util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel {

const inline EmulationService::RobotOutputs generateZeroedOutput() {
	auto res = EmulationService::RobotOutputs{};
	for (auto i = 0u; i < PWMSystem::NUM_HDRS; i++) {
		res.add_pwm_headers(0);
	}
	for (auto i = 0u; i < RoboRIOManager::getInstance().first->can_motor_controllers.size(); i++) {
		res.add_can_motor_controllers();
	}
	return res;
}


RobotOutputs::RobotOutputs()
	: enabled(RobotMode::DEFAULT_ENABLED_STATUS),
	  pwm_hdrs(0.0),
	  relays(RelaySystem::State::OFF),
	  analog_outputs(0.0),
	  digital_mxp({}),
	  digital_hdrs({DigitalSystem::HeaderConfig::DI, false}),
	  can_motor_controllers({}) {
		  output = generateZeroedOutput();
	  }

EmulationService::RobotOutputs RobotOutputs::syncShallow() {
	if(!enabled){
		output = generateZeroedOutput();
		return output;
	}

	for (auto i = 0u; i < pwm_hdrs.size(); i++) {
		while(i >= (unsigned)output.pwm_headers_size()){
			output.add_pwm_headers(0);
		}
		output.set_pwm_headers(i, pwm_hdrs[i]);
	}
	for (auto i = 0u; i < digital_mxp.size(); i++) {
		switch (digital_mxp[i].config) {
			case MXPData::Config::DI:
			case MXPData::Config::DO:
			case MXPData::Config::PWM:
				{
					EmulationService::MXPData mxp;
					while(i >= (unsigned)output.mxp_data_size()){
						output.add_mxp_data();
					}
					mxp.set_config(static_cast<EmulationService::MXPData::Config>(digital_mxp[i].config));
					mxp.set_value(digital_mxp[i].value);
					*output.mutable_mxp_data(i) = mxp;
					break;
				}
			default:
				break;
		}
	}

	for (auto i = 0u; i < can_motor_controllers.size(); i++) {
		EmulationService::RobotOutputs::CANMotorController can;
		can.set_can_type(static_cast<EmulationService::RobotOutputs::CANType>(
			can_motor_controllers[i]->getType()));
		can.set_id(can_motor_controllers[i]->getID());
		can.set_percent_output(can_motor_controllers[i]->getPercentOutput());
		while(i >= (unsigned)output.can_motor_controllers_size()){
			output.add_can_motor_controllers();
		}
		*output.mutable_can_motor_controllers(i) = can;
	}

	return output;
}

void RobotOutputs::updateShallow() {
	if (!hal_is_initialized) {
		return;
	}

	RoboRIO roborio = RoboRIOManager::getCopy();

	for (unsigned i = 0; i < pwm_hdrs.size(); i++) {
		pwm_hdrs[i] = PWMSystem::getPercentOutput(roborio.pwm_system.getHdrPulseWidth(i));
	}

	for (unsigned i = 0; i < digital_mxp.size(); i++) {
		digital_mxp[i].config = roborio.digital_system.getMXPConfig(i);

		switch (digital_mxp[i].config) {
			case MXPData::Config::DO:
				digital_mxp[i].value =
					(checkBitHigh(roborio.digital_system.getOutputs().MXP, i) |
					 checkBitHigh(roborio.digital_system.getPulses().MXP, i));
				break;
			case MXPData::Config::PWM: {
				int remapped_i = i;
				if (remapped_i >= 4) {  // digital ports 0-3 line up with mxp pwm ports 0-3, the rest are offset by 4
					remapped_i -= 4;
				}
				digital_mxp[i].value = PWMSystem::getPercentOutput(roborio.pwm_system.getMXPPulseWidth(remapped_i));
				break;
			}
			case MXPData::Config::SPI:
			case MXPData::Config::I2C:
			default:
				break;  // do nothing
		}
	}
	can_motor_controllers = roborio.can_motor_controllers;
}

EmulationService::RobotOutputs RobotOutputs::syncDeep() {
	syncShallow();
	for (auto i = 0u; i < relays.size(); i++) {
		while(i >= (unsigned)output.relays_size()){
			output.add_relays(EmulationService::RobotOutputs_RelayState_OFF);
		}
		output.set_relays(i, static_cast<EmulationService::RobotOutputs::RelayState>(relays[i]));
	}
	for (auto i = 0u; i < analog_outputs.size(); i++) {
		while(i >= (unsigned)output.analog_outputs_size()){
			output.add_analog_outputs(0);
		}
		output.set_analog_outputs(i, analog_outputs[i]);
	}
	for (auto i = 0u; i < digital_hdrs.size(); i++) {
		while(i >= (unsigned)output.digital_headers_size()){
			output.add_digital_headers();
		}
		EmulationService::DIOData dio;
		dio.set_config(static_cast<EmulationService::DIOData::Config>(digital_hdrs[i].first));
		dio.set_value(digital_hdrs[i].second);
		*output.mutable_digital_headers(i) = dio;
	}
	return output;
}

void RobotOutputs::updateDeep() {
	updateShallow();

	RoboRIO roborio = RoboRIOManager::getCopy();

	for (unsigned i = 0; i < relays.size(); i++) {
		relays[i] = roborio.relay_system.getState(i);
	}
	for (unsigned i = 0; i < analog_outputs.size(); i++) {
		analog_outputs[i] =
			(roborio.analog_outputs.getMXPOutput(i)) * 5.0 / 0x1000;
	}
	{
		tDIO::tOutputEnable output_mode =
			roborio.digital_system.getEnabledOutputs();
		auto values = roborio.digital_system.getOutputs().Headers;
		auto pulses = roborio.digital_system.getPulses().Headers;
		for (unsigned i = 0; i < digital_hdrs.size(); i++) {
			digital_hdrs[i].first = checkBitHigh(output_mode.Headers, i) ? DigitalSystem::HeaderConfig::DO : DigitalSystem::HeaderConfig::DI;
			if (digital_hdrs[i].first == DigitalSystem::HeaderConfig::DO) {  // if digital port is set for output, then set digital output
				digital_hdrs[i].second = (checkBitHigh(values, i) | checkBitHigh(pulses, i));
			}
		}
	}
	can_motor_controllers = roborio.can_motor_controllers;
}

std::string RobotOutputs::toString() const {
	std::string s = "(";
	s += "pwm_hdrs:" +
		 asString(pwm_hdrs,
				  std::function<std::string(double)>(
					  static_cast<std::string (*)(double)>(std::to_string))) +
		 ", ";
	s += "relays:" +
		 asString(
			 relays,
			 std::function<std::string(RelaySystem::State)>(
				 static_cast<std::string (*)(RelaySystem::State)>(asString))) +
		 ", ";
	s += "analog_outputs:" +
		 asString(analog_outputs,
				  std::function<std::string(double)>(
					  static_cast<std::string (*)(double)>(std::to_string))) +
		 ", ";
	s += "digital_mxp:" +
		 asString(digital_mxp,
				  std::function<std::string(MXPData)>(&MXPData::toString)) +
		 ", ";
	s += "digital_hdrs:" +
		 asString(digital_hdrs,
			std::function<std::string(
				std::pair<DigitalSystem::HeaderConfig, bool>)>(
					[&](std::pair<DigitalSystem::HeaderConfig, bool> a) {
						return "(" + asString(a.first) + ", " + std::to_string(a.second) + ")";
				})) +
		 ", ";
	s +=
		"can_motor_controllers:" +
		asString(
			can_motor_controllers,
			std::function<std::string(
				std::pair<uint32_t, std::shared_ptr<CANMotorControllerBase>>)>(
				[&](std::pair<uint32_t, std::shared_ptr<CANMotorControllerBase>>
						a) {
					return "(" + std::to_string(a.first) + ", " +
						   a.second->toString() + ")";
				}));
	s += ")";
	return s;
}

void RobotOutputs::setEnable(bool e) {
	enabled = e;
}
}  // namespace hel
