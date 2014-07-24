#include "tEncoderImpl.h"
#include "NiFpgaState.h"

namespace nFPGA {
	tEncoder_Impl::tEncoder_Impl(NiFpgaState *state, unsigned char sys_index) {
		this->state = state;
		this->sys_index = sys_index;
		this->encoderConfig.value = 0;
		this->encoderConfig.Enable = false;

		this->encoderOutput.Value = 0;
		this->encoderOutput.Direction = 0;

		this->timerOutput.value = 0;
		this->timerConfig.value = 0;
		// TODO Actually Implement
	}

	tEncoder_Impl::~tEncoder_Impl() {
		if (this->state->encoder[this->sys_index] == this) {
			this->state->encoder[this->sys_index] = NULL;
		}
	}

	tSystemInterface *tEncoder_Impl::getSystemInterface() {
		return state;
	}

	unsigned char tEncoder_Impl::getSystemIndex() {
		return sys_index;
	}

	tEncoder_Impl::tOutput tEncoder_Impl::readOutput(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderOutput;
	}
	bool tEncoder_Impl::readOutput_Direction(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderOutput.Direction;
	}
	signed int tEncoder_Impl::readOutput_Value(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderOutput.Value;
	}

	void tEncoder_Impl::writeConfig(tEncoder_Impl::tConfig value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig = value;
	}
	void tEncoder_Impl::writeConfig_ASource_Channel(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.ASource_Channel = value;
	}
	void tEncoder_Impl::writeConfig_ASource_Module(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.ASource_Module = value;
	}
	void tEncoder_Impl::writeConfig_ASource_AnalogTrigger(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.ASource_AnalogTrigger = value;
	}
	void tEncoder_Impl::writeConfig_BSource_Channel(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.BSource_Channel = value;
	}
	void tEncoder_Impl::writeConfig_BSource_Module(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.BSource_Module = value;
	}
	void tEncoder_Impl::writeConfig_BSource_AnalogTrigger(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.BSource_AnalogTrigger = value;
	}
	void tEncoder_Impl::writeConfig_IndexSource_Channel(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.IndexSource_Channel = value;
	}
	void tEncoder_Impl::writeConfig_IndexSource_Module(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.IndexSource_Module = value;
	}
	void tEncoder_Impl::writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.IndexSource_AnalogTrigger = value;
	}
	void tEncoder_Impl::writeConfig_IndexActiveHigh(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.IndexActiveHigh = value;
	}
	void tEncoder_Impl::writeConfig_Reverse(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.Reverse = value;
	}
	void tEncoder_Impl::writeConfig_Enable(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		encoderConfig.Enable = value;
	}
	tEncoder_Impl::tConfig tEncoder_Impl::readConfig(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig;
	}
	unsigned char tEncoder_Impl::readConfig_ASource_Channel(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.ASource_Channel;
	}
	unsigned char tEncoder_Impl::readConfig_ASource_Module(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.ASource_Module;
	}
	bool tEncoder_Impl::readConfig_ASource_AnalogTrigger(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.ASource_AnalogTrigger;
	}
	unsigned char tEncoder_Impl::readConfig_BSource_Channel(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.BSource_Channel;
	}
	unsigned char tEncoder_Impl::readConfig_BSource_Module(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.BSource_Module;
	}
	bool tEncoder_Impl::readConfig_BSource_AnalogTrigger(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.BSource_AnalogTrigger;
	}
	unsigned char tEncoder_Impl::readConfig_IndexSource_Channel(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.IndexSource_Channel;
	}
	unsigned char tEncoder_Impl::readConfig_IndexSource_Module(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.IndexSource_Module;
	}
	bool tEncoder_Impl::readConfig_IndexSource_AnalogTrigger(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.IndexSource_AnalogTrigger;
	}
	bool tEncoder_Impl::readConfig_IndexActiveHigh(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.IndexActiveHigh;
	}
	bool tEncoder_Impl::readConfig_Reverse(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.Reverse;
	}
	bool tEncoder_Impl::readConfig_Enable(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return encoderConfig.Enable;
	}

	tEncoder_Impl::tTimerOutput tEncoder_Impl::readTimerOutput(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return timerOutput;
	}
	unsigned int tEncoder_Impl::readTimerOutput_Period(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return timerOutput.Period;
	}
	signed char tEncoder_Impl::readTimerOutput_Count(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return timerOutput.Count;
	}
	bool tEncoder_Impl::readTimerOutput_Stalled(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return timerOutput.Stalled;
	}

	void tEncoder_Impl::strobeReset(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		// TODO
	}

	void tEncoder_Impl::writeTimerConfig(tEncoder_Impl::tTimerConfig value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		timerConfig = value;
	}
	void tEncoder_Impl::writeTimerConfig_StallPeriod(unsigned int value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		timerConfig.StallPeriod = value;
	}
	void tEncoder_Impl::writeTimerConfig_AverageSize(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		timerConfig.AverageSize = value;
	}
	void tEncoder_Impl::writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		timerConfig.UpdateWhenEmpty = value;
	}
	tEncoder_Impl::tTimerConfig tEncoder_Impl::readTimerConfig(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return timerConfig;
	}
	unsigned int tEncoder_Impl::readTimerConfig_StallPeriod(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return timerConfig.StallPeriod;
	}
	unsigned char tEncoder_Impl::readTimerConfig_AverageSize(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return timerConfig.AverageSize;
	}
	bool tEncoder_Impl::readTimerConfig_UpdateWhenEmpty(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return timerConfig.UpdateWhenEmpty;
	}

}