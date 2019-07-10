#include "roborio_manager.hpp"
#include "robot_outputs.hpp"
#include "util.hpp"

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel {

RobotOutputs::RobotOutputs()
	: new_data(true),
	  enabled(RobotMode::DEFAULT_ENABLED_STATUS),
	  pwm_hdrs(0.0),
	  relays(RelaySystem::State::OFF),
	  analog_outputs(0.0),
	  digital_mxp({}),
	  digital_hdrs(false),
	  can_motor_controllers({}) {
	output = EmulationService::RobotOutputs();
	for (auto i = 0u; i < pwm_hdrs.size(); i++) {
		output.add_pwm_headers(0);
	}
}

bool RobotOutputs::hasNewData() const { return new_data; }

EmulationService::RobotOutputs RobotOutputs::syncShallow() {
	if (!new_data) {
		return output;
	}

	for (auto i = 0u; i < pwm_hdrs.size(); i++) {
		output.set_pwm_headers(i, pwm_hdrs[i]);
	}
	for (auto i = 0u; i < digital_mxp.size(); i++) {
		switch (digital_mxp[i].config) {
			case MXPData::Config::PWM:
			case MXPData::Config::DO: {
				auto elem = output.mutable_mxp_data(i);
				EmulationService::MXPData mxp;
				mxp.set_mxp_config(
					static_cast<EmulationService::MXPData::MXPConfig>(
						digital_mxp[i].config));
				mxp.set_value(digital_mxp[i].value);
				*elem = mxp;
				break;
			}
			default:
				break;
		}
	}

	for (auto i = 0u; i < can_motor_controllers.size(); i++) {
		auto elem = output.mutable_can_device(i);
		EmulationService::RobotOutputs::CANDevice can;
		can.set_can_type(static_cast<EmulationService::RobotOutputs::CANType>(
			can_motor_controllers[i]->getType()));
		can.set_id(can_motor_controllers[i]->getID());
		can.set_inverted(false);
		can.set_percent_output(can_motor_controllers[i]->getPercentOutput());
		*elem = can;
	}

	new_data = false;
	return output;
}

void RobotOutputs::updateShallow() {
	if (!hal_is_initialized) {
		return;
	}

	RoboRIO roborio = RoboRIOManager::getCopy();

	for (unsigned i = 0; i < pwm_hdrs.size(); i++) {
		pwm_hdrs[i] =
			PWMSystem::getPercentOutput(roborio.pwm_system.getHdrPulseWidth(i));
	}

	for (unsigned i = 0; i < digital_mxp.size(); i++) {
		digital_mxp[i].config = DigitalSystem::toMXPConfig(
			roborio.digital_system.getEnabledOutputs().MXP,
			roborio.digital_system.getMXPSpecialFunctionsEnabled(), i);

		switch (digital_mxp[i].config) {
			case MXPData::Config::DO:
				digital_mxp[i].value =
					(checkBitHigh(roborio.digital_system.getOutputs().MXP, i) |
					 checkBitHigh(roborio.digital_system.getPulses().MXP, i));
				break;
			case MXPData::Config::PWM: {
				int remapped_i = i;
				if (remapped_i >=
					4) {  // digital ports 0-3 line up with mxp pwm ports 0-3,
						  // the rest are offset by 4
					remapped_i -= 4;
				}
				digital_mxp[i].value = PWMSystem::getPercentOutput(
					roborio.pwm_system.getMXPPulseWidth(remapped_i));
				break;
			}
			case MXPData::Config::SPI:
			case MXPData::Config::I2C:
			default:
				break;  // do nothing
		}
	}
	can_motor_controllers = roborio.can_motor_controllers;
	new_data = true;
}

EmulationService::RobotOutputs RobotOutputs::syncDeep() {
	syncShallow();
	for (auto i = 0u; i < relays.size(); i++) {
		output.set_relay(
			i,
			static_cast<EmulationService::RobotOutputs::RelayState>(relays[i]));
	}
	for (auto i = 0u; i < analog_outputs.size(); i++) {
		output.set_analog_outputs(i, analog_outputs[i]);
	}
	for (auto i = 0u; i < digital_hdrs.size(); i++) {
		output.set_digital_headers(i, digital_hdrs[i]);
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
			if (checkBitHigh(output_mode.Headers,
							 i)) {  // if digital port is set for output, then
									// set digital output
				digital_hdrs[i] =
					(checkBitHigh(values, i) | checkBitHigh(pulses, i));
			}
		}
	}
	can_motor_controllers = roborio.can_motor_controllers;
	new_data = true;
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
				  std::function<std::string(bool)>(
					  static_cast<std::string (*)(bool)>(asString))) +
		 ", ";
	s +=
		"can_motor_controllers:" +
		asString(
			can_motor_controllers,
			std::function<std::string(std::pair<uint32_t, std::shared_ptr<CANMotorControllerBase>>)>(
                [&](std::pair<uint32_t, std::shared_ptr<CANMotorControllerBase>> a) {
					return "[" + std::to_string(a.first) + ", " +
						   a.second->toString() + "]";
				}));
	s += ")";
	return s;
}

void RobotOutputs::enable(bool e) {
	if (e != enabled) {
		new_data = true;
		enabled = e;
	}
}
}  // namespace hel
