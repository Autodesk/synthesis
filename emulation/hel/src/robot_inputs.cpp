#include "robot_inputs.hpp"
#include "roborio_manager.hpp"
#include "util.hpp"

#include <algorithm>

using namespace nFPGA;
using namespace nRoboRIO_FPGANamespace;

namespace hel {

RobotInputs::RobotInputs()
	: digital_hdrs(false),
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
		tDIO::tOutputEnable output_mode =
			instance.first->digital_system.getEnabledOutputs();
		for (unsigned i = 0; i < digital_hdrs.size(); i++) {
			if (checkBitLow(output_mode.Headers,
							i)) {  // if set for input, then read in the inputs
				di.Headers = setBit(di.Headers, digital_hdrs[i], i);
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
		 asString(digital_hdrs, [](auto d) { return std::to_string(d); }) +
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

		for (size_t j = 0; j < std::min(joystick->getAxes().size(),
										(size_t) joystick_data.axes_size());
			 j++) {
			joysticks[i].getAxes()[j] = joystick_data.axes(j);
		}

		joystick->setAxisCount(joystick_data.axis_count());

		for (size_t j = 0; j < std::min(joystick->getAxisTypes().size(),
										(size_t) joystick_data.axes_size());
			 j++) {
			joysticks[i].getAxisTypes()[j] = joystick_data.axis_types(j);
		}

		for (size_t j = 0; j < std::min(joystick->getPOVs().size(),
										(size_t) joystick_data.povs_size());
			 j++) {
			joysticks[i].getPOVs()[j] = joystick_data.povs(j);
		}

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
	for (size_t i = 0; i < std::min(this->digital_hdrs.size(),
									(size_t) req.digital_headers_size());
		 i++) {
		this->digital_hdrs[i] = req.digital_headers(i);
	}

	for (size_t i = 0;
		 i < std::min(this->digital_mxp.size(), (size_t) req.mxp_data_size());
		 i++) {
		auto mxp = &this->digital_mxp[i];
		auto mxp_data = req.mxp_data(i);
		mxp->config = static_cast<MXPData::Config>(mxp_data.mxp_config());
		mxp->value = mxp_data.value();
	}

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

		for (size_t j = 0; j < std::min(joystick->getAxes().size(),
										(size_t) joystick_data.axes_size());
			 j++) {
			joysticks[i].getAxes()[j] = joystick_data.axes(j);
		}

		joystick->setAxisCount(joystick_data.axis_count());

		for (size_t j = 0; j < std::min(joystick->getAxisTypes().size(),
										(size_t) joystick_data.axes_size());
			 j++) {
			joysticks[i].getAxisTypes()[j] = joystick_data.axis_types(j);
		}

		for (size_t j = 0; j < std::min(joystick->getPOVs().size(),
										(size_t) joystick_data.povs_size());
			 j++) {
			joysticks[i].getPOVs()[j] = joystick_data.povs(j);
		}

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
		if (!encoder) {
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

}  // namespace hel
