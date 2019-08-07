#include "robot_inputs.hpp"
#include "roborio_manager.hpp"
#include "util.hpp"

#include <algorithm>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel {

RobotInputs::RobotInputs()
	: digital_hdrs({DigitalSystem::HeaderConfig::DI, false}),
	  digital_mxp({}),
	  joysticks({}),
	  match_info({}),
	  robot_mode({}),
	  encoder_managers({}) {}

void RobotInputs::updateShallow() const {
	if (!hal_is_initialized) {
		return;
	}
	auto instance = RoboRIOManager::getInstance();

	instance.first->joysticks = joysticks;
	instance.first->match_info = match_info;
	instance.first->robot_mode = robot_mode;
	instance.first->encoder_managers = encoder_managers;
	for (Maybe<EncoderManager>& a : instance.first->encoder_managers) {
		if (a.isValid()) {
			a.get().update();
		}
	}
	instance.second.unlock();
}

void RobotInputs::updateDeep() const {
	if (!hal_is_initialized) {
		return;
	}
	auto instance = RoboRIOManager::getInstance();

	updateShallow();
	{
		tDIO::tDI di = instance.first->digital_system.getInputs();
		for (unsigned i = 0; i < digital_hdrs.size(); i++) {
			if (digital_hdrs[i].first == DigitalSystem::HeaderConfig::DI) {  // if set for input, then read in the inputs
				di.Headers = setBit(di.Headers, digital_hdrs[i].second, i);
			}
		}
		// TODO add MXP digital inputs
		instance.first->digital_system.setInputs(di);
	}
	instance.second.unlock();
}

std::string RobotInputs::toString() const {
	std::string s = "(";
	s += "digital_hdrs:" +
		asString(digital_hdrs, [](auto d) { return "(" + asString(d.first) + " " + std::to_string(d.second) + ")"; }) +
		 ", ";
	s += "joysticks:" +
		 asString(joysticks, [](auto j) { return j.toString(); }) + ", ";
	s += "digital_mxp:" +
		 asString(digital_mxp, [](auto m) { return m.toString(); }) + ", ";
	s += "match_info:" + match_info.toString() + ", ";
	s += "robot_mode:" + robot_mode.toString() + ", ";
	s += "encoder_managers:" +
		 asString(encoder_managers, [&](Maybe<EncoderManager> a) {
			 if (a.isValid()) {
				 return a.get().toString();
			 }
			 return std::string("null");
		 });
	s += ")";
	return s;
}
void RobotInputs::sync(const EmulationService::RobotInputs& req) {
	for (size_t i = 0;
		 i < std::min(this->joysticks.size(), (size_t) req.joysticks_size());
		 i++) {
		auto joystick = &this->joysticks[i];
		auto joystick_data = req.joysticks(i);

		joystick->setIsXBox(joystick_data.is_xbox());
		joystick->setType((uint8_t) joystick_data.type());
		joystick->setName(joystick_data.name());
		joystick->setButtons(joystick_data.buttons());
		joystick->setButtonCount((uint8_t) joystick_data.button_count());
		joystick->setAxes(joystick_data.axes());
		joystick->setAxisCount(joystick_data.axis_count());
		joystick->setAxisTypes(joystick_data.axis_types());
		joystick->setPOVs(joystick_data.povs());
		joystick->setPOVCount(joystick_data.pov_count());
		joystick->setOutputs(joystick_data.outputs());
		joystick->setLeftRumble(joystick_data.left_rumble());
		joystick->setRightRumble(joystick_data.right_rumble());
	}

	auto match_info = &this->match_info;
	match_info->setEventName(req.match_info().event_name());
	match_info->setGameSpecificMessage(
		req.match_info().game_specific_message());
	match_info->setMatchType(
		static_cast<MatchType_t>(req.match_info().match_type()));
	match_info->setMatchNumber(req.match_info().match_number());
	match_info->setReplayNumber(req.match_info().replay_number());
	match_info->setAllianceStationID(static_cast<AllianceStationID_t>(
		req.match_info().alliance_station_id()));
	match_info->setMatchTime(req.match_info().match_time());

	auto robot_mode = &this->robot_mode;
	robot_mode->setEnabled(req.robot_mode().enabled());
	robot_mode->setEmergencyStopped(req.robot_mode().is_emergency_stopped());
	robot_mode->setFMSAttached(req.robot_mode().is_fms_attached());
	robot_mode->setDSAttached(req.robot_mode().is_ds_attached());
	robot_mode->setMode(static_cast<RobotMode::Mode>(req.robot_mode().mode()));

	for (size_t i = 0; i < std::min(this->encoder_managers.size(),
									(size_t) req.encoder_managers_size());
		 i++) {
		auto encoder = &this->encoder_managers[i];
		auto encoder_data = req.encoder_managers(i);
		if (!encoder->isValid()) {
			auto new_encoder = EncoderManager(
				encoder_data.a_channel(),
				static_cast<EncoderManager::PortType>(encoder_data.a_type()),
				encoder_data.b_channel(),
				static_cast<EncoderManager::PortType>(encoder_data.b_type()));
			encoder->set(new_encoder);
		} else {
			encoder->get().setAChannel(encoder_data.a_channel());
			encoder->get().setAType(static_cast<EncoderManager::PortType>(
				encoder_data.a_channel()));
			encoder->get().setBChannel(encoder_data.b_channel());
			encoder->get().setBType(static_cast<EncoderManager::PortType>(
				encoder_data.b_channel()));
		}
	}
}

void RobotInputs::syncDeep(const EmulationService::RobotInputs& req) {
	sync(req);

	// Update digital header config from internal state
	auto instance = RoboRIOManager::getInstance();
	tDIO::tOutputEnable output_mode = instance.first->digital_system.getEnabledOutputs();
	for (unsigned i = 0; i < digital_hdrs.size(); i++) {
		digital_hdrs[i].first = checkBitHigh(output_mode.Headers, i) ? DigitalSystem::HeaderConfig::DO : DigitalSystem::HeaderConfig::DI;
	}

	for (size_t i = 0; i < std::min(this->digital_hdrs.size(), (size_t) req.digital_headers_size()); i++) {
		this->digital_hdrs[i].second = req.digital_headers(i).value();
	}

	for (size_t i = 0; i < std::min(this->digital_mxp.size(), (size_t) req.mxp_data_size()); i++) {
		auto mxp_data = req.mxp_data(i);
		digital_mxp[i].value = mxp_data.value();
		// Don't sync MXP config, let robot code handle that
	}

	for (unsigned i = 0; i < digital_mxp.size(); i++) {
		digital_mxp[i].config = instance.first->digital_system.getMXPConfig(i);
	}
	instance.second.unlock();
}
}  // namespace hel
