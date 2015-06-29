#include "ChipObject/tEncoderImpl.h"
#include "ChipObject/NiFpgaState.h"
#include "ChipObject/tGlobalImpl.h"

#define TENCODER_DECL_ADDRESS(x) const int tEncoder_Impl::k ## x ## _Addresses [] = {kEncoder0_ ## x ## _Address, kEncoder1_ ## x ## _Address, kEncoder2_ ## x ## _Address, kEncoder3_ ## x ## _Address }

namespace nFPGA {
	TENCODER_DECL_ADDRESS(Config);
	TENCODER_DECL_ADDRESS(Output);
	TENCODER_DECL_ADDRESS(TimerConfig);
	TENCODER_DECL_ADDRESS(TimerOutput);
	TENCODER_DECL_ADDRESS(Reset);

	tEncoder_Impl::tEncoder_Impl(NiFpgaState *state, unsigned char sys_index) {
		this->state = state;
		this->sys_index = sys_index;

		this->encoderConfig = (tConfig*) &(state->fpgaRAM[kConfig_Addresses[sys_index]]);
		this->encoderOutput = (tOutput*) &(state->fpgaRAM[kOutput_Addresses[sys_index]]);
		this->timerConfig = (tTimerConfig*) &(state->fpgaRAM[kTimerConfig_Addresses[sys_index]]);
		this->timerOutput = (tTimerOutput*) &(state->fpgaRAM[kTimerOutput_Addresses[sys_index]]);

		this->outputOffset = 0;
		this->lastUpdate = 0;

		(*encoderConfig).value = 0;
		(*encoderConfig).Enable = false;
		(*encoderConfig).Reverse = false;

		(*encoderOutput).Value = 0;
		(*encoderOutput).Direction = 0;

		(*timerOutput).value = 0;
		(*timerConfig).value = 0;
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
		return *encoderOutput;
	}
	bool tEncoder_Impl::readOutput_Direction(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderOutput).Direction;
	}
	signed int tEncoder_Impl::readOutput_Value(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderOutput).Value;
	}

	void tEncoder_Impl::writeConfig(tEncoder_Impl::tConfig value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		*encoderConfig = value;
	}
	void tEncoder_Impl::writeConfig_ASource_Channel(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).ASource_Channel = value;
	}
	void tEncoder_Impl::writeConfig_ASource_Module(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).ASource_Module = value;
	}
	void tEncoder_Impl::writeConfig_ASource_AnalogTrigger(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).ASource_AnalogTrigger = value;
	}
	void tEncoder_Impl::writeConfig_BSource_Channel(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).BSource_Channel = value;
	}
	void tEncoder_Impl::writeConfig_BSource_Module(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).BSource_Module = value;
	}
	void tEncoder_Impl::writeConfig_BSource_AnalogTrigger(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).BSource_AnalogTrigger = value;
	}
	void tEncoder_Impl::writeConfig_IndexSource_Channel(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).IndexSource_Channel = value;
	}
	void tEncoder_Impl::writeConfig_IndexSource_Module(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).IndexSource_Module = value;
	}
	void tEncoder_Impl::writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).IndexSource_AnalogTrigger = value;
	}
	void tEncoder_Impl::writeConfig_IndexActiveHigh(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).IndexActiveHigh = value;
	}
	void tEncoder_Impl::writeConfig_Reverse(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).Reverse = value;
	}
	void tEncoder_Impl::writeConfig_Enable(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*encoderConfig).Enable = value;
	}
	tEncoder_Impl::tConfig tEncoder_Impl::readConfig(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return *encoderConfig;
	}
	unsigned char tEncoder_Impl::readConfig_ASource_Channel(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).ASource_Channel;
	}
	unsigned char tEncoder_Impl::readConfig_ASource_Module(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).ASource_Module;
	}
	bool tEncoder_Impl::readConfig_ASource_AnalogTrigger(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).ASource_AnalogTrigger;
	}
	unsigned char tEncoder_Impl::readConfig_BSource_Channel(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).BSource_Channel;
	}
	unsigned char tEncoder_Impl::readConfig_BSource_Module(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).BSource_Module;
	}
	bool tEncoder_Impl::readConfig_BSource_AnalogTrigger(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).BSource_AnalogTrigger;
	}
	unsigned char tEncoder_Impl::readConfig_IndexSource_Channel(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).IndexSource_Channel;
	}
	unsigned char tEncoder_Impl::readConfig_IndexSource_Module(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).IndexSource_Module;
	}
	bool tEncoder_Impl::readConfig_IndexSource_AnalogTrigger(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).IndexSource_AnalogTrigger;
	}
	bool tEncoder_Impl::readConfig_IndexActiveHigh(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).IndexActiveHigh;
	}
	bool tEncoder_Impl::readConfig_Reverse(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).Reverse;
	}
	bool tEncoder_Impl::readConfig_Enable(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*encoderConfig).Enable;
	}

	tEncoder_Impl::tTimerOutput tEncoder_Impl::readTimerOutput(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return *timerOutput;
	}
	unsigned int tEncoder_Impl::readTimerOutput_Period(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*timerOutput).Period;
	}
	signed char tEncoder_Impl::readTimerOutput_Count(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*timerOutput).Count;
	}
	bool tEncoder_Impl::readTimerOutput_Stalled(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*timerOutput).Stalled;
	}

	void tEncoder_Impl::strobeReset(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		NiFpga_WriteU32(state->getHandle(), kReset_Addresses[sys_index], 1);	// Use WriteU32 to handle strobe
	}

	void tEncoder_Impl::writeTimerConfig(tEncoder_Impl::tTimerConfig value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		*timerConfig = value;
	}
	void tEncoder_Impl::writeTimerConfig_StallPeriod(unsigned int value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*timerConfig).StallPeriod = value;
	}
	void tEncoder_Impl::writeTimerConfig_AverageSize(unsigned char value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*timerConfig).AverageSize = value;
	}
	void tEncoder_Impl::writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		(*timerConfig).UpdateWhenEmpty = value;
	}
	tEncoder_Impl::tTimerConfig tEncoder_Impl::readTimerConfig(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return *timerConfig;
	}
	unsigned int tEncoder_Impl::readTimerConfig_StallPeriod(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*timerConfig).StallPeriod;
	}
	unsigned char tEncoder_Impl::readTimerConfig_AverageSize(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*timerConfig).AverageSize;
	}
	bool tEncoder_Impl::readTimerConfig_UpdateWhenEmpty(tRioStatusCode *status){
		*status=NiFpga_Status_Success;
		return (*timerConfig).UpdateWhenEmpty;
	}

	void tEncoder_Impl::doUpdate(int32_t value) {
		tRioStatusCode status;

		int32_t reverse = ((*encoderConfig).Reverse ? -1 : 1);
		int32_t prev = (reverse * (*encoderOutput).Value + outputOffset);
		bool dir = prev < value;
		uint32_t cTime = state->getGlobal()->readLocalTime(&status);
		// period in micros = (double)(output.Period << 1) / (double)output.Count;
		if (prev != value) {
			(*timerOutput).Period = (cTime - lastUpdate);
			(*timerOutput).Count = (value - prev) << 1;
			lastUpdate = cTime;
		}
		(*timerOutput).Stalled = (*timerOutput).Period < (*timerConfig).StallPeriod;
		(*encoderOutput).Value = reverse * (value - outputOffset);
		(*encoderOutput).Direction = (*encoderConfig).Reverse ? !dir : dir;
	}

	void tEncoder_Impl::doReset() {
		int32_t prev = ((*encoderOutput).Value + outputOffset);
		outputOffset = prev;
		doUpdate(prev);
	}
}