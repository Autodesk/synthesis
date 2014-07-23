/*
* tAIImpl.cpp
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#include "tAIImpl.h"
#include "tAI.h"
#include "NiFpgaState.h"
#include "tAnalogTriggerImpl.h"

namespace nFPGA {
	tAI_Impl::tAI_Impl(NiFpgaState *state, unsigned char sys_index) {
		this->state = state;
		this->sys_index = sys_index;

		// Default things
		this->config.ConvertRate = 100;
		this->config.ScanSize = 8;
	}

	tAI_Impl::~tAI_Impl() {
		if (this->state->ai[sys_index] == this) {
			this->state->ai[sys_index] = NULL;
		}
	}

	unsigned char tAI_Impl::getSystemIndex() {
		return sys_index;
	}

	tSystemInterface *tAI_Impl::getSystemInterface() {
		return state;
	}

	void tAI_Impl::writeConfig(tConfig value, tRioStatusCode *status){
		config = value;
		*status =  NiFpga_Status_Success;
	}
	void tAI_Impl::writeConfig_ScanSize(unsigned char value, tRioStatusCode *status){
		config.ScanSize = value;
		*status =  NiFpga_Status_Success;
	}
	void tAI_Impl::writeConfig_ConvertRate(unsigned int value, tRioStatusCode *status){
		config.ConvertRate = value;
		*status =  NiFpga_Status_Success;
	}
	tAI_Impl::tConfig tAI_Impl::readConfig(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return config;
	}
	unsigned char tAI_Impl::readConfig_ScanSize(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return config.ScanSize;
	}
	unsigned int tAI_Impl::readConfig_ConvertRate(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return config.ConvertRate;
	}

	unsigned int tAI_Impl::readLoopTiming(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return config.ConvertRate;	// Yeah this works somehow.
	}

	void tAI_Impl::writeOversampleBits(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumOversampleBitsElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return;
		}
		*status =  NiFpga_Status_Success;
		oversampleBits[bitfield_index] = value;
	}
	unsigned char tAI_Impl::readOversampleBits(unsigned char bitfield_index, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumOversampleBitsElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		return oversampleBits[bitfield_index];
	}

	void tAI_Impl::writeAverageBits(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumAverageBitsElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return;
		}
		*status =  NiFpga_Status_Success;
		averageBits[sys_index] = value;
	}
	unsigned char tAI_Impl::readAverageBits(unsigned char bitfield_index, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumAverageBitsElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		return averageBits[sys_index];
	}

	void tAI_Impl::writeScanList(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumScanListElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return;
		}
		*status =  NiFpga_Status_Success;
		scanList[bitfield_index] = value;
	}
	unsigned char tAI_Impl::readScanList(unsigned char bitfield_index, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumScanListElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		return scanList[bitfield_index];
	}

	void tAI_Impl::writeReadSelect(tReadSelect value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		readSelect= value;
	}
	void tAI_Impl::writeReadSelect_Channel(unsigned char value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		readSelect.Channel = value;
	}
	void tAI_Impl::writeReadSelect_Module(unsigned char value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		readSelect.Module = value;
	}
	void tAI_Impl::writeReadSelect_Averaged(bool value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		readSelect.Averaged = value;
	}
	tAI_Impl::tReadSelect tAI_Impl::readReadSelect(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return readSelect;
	}
	unsigned char tAI_Impl::readReadSelect_Channel(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return readSelect.Channel;
	}
	unsigned char tAI_Impl::readReadSelect_Module(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return readSelect.Module;
	}
	bool tAI_Impl::readReadSelect_Averaged(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return readSelect.Averaged;
	}

	signed int tAI_Impl::readOutput(tRioStatusCode *status){
		if (readSelect.Channel < 0 || readSelect.Channel >= kNumScanListElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		return values[readSelect.Channel];
	}
	void tAI_Impl::strobeLatchOutput(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		// Doesn't seem to do anything important.
	}

	void tAI_Impl::updateValues(signed int nvalues[]) {
		for (int i = 0; i < kNumScanListElements; i++) {
			if (nvalues[i] != values[i]) {
				// Changed!  Check for triggering
				tAnalogTrigger_Impl::tOutput changeType;
				changeType.Falling = values[i] > nvalues[i];
				changeType.Rising = values[i] < nvalues[i];
				for (int t = 0; t < tAnalogTrigger_Impl::kNumSystems; t++) {
					if (state->analogTrigger[t]->source.Module == sys_index && state->analogTrigger[t]->source.Channel == i) {
						changeType.OverLimit = nvalues[i] >= state->analogTrigger[t]->upperLimit;
						changeType.InHysteresis = nvalues[i] >= state->analogTrigger[t]->lowerLimit && !changeType.OverLimit;
						if (state->analogTrigger[t]->output[state->analogTrigger[t]->sys_index].value != changeType.value){
							state->analogTrigger[t]->output[state->analogTrigger[t]->sys_index] = changeType;
							// Signal it somehow?  Not entirely sure how this signal is routed.
							// Plan:  Make IRQs 64 bit.  if analog trigger shift left by 32.
						}
					}
				}
			}
			values[i] = nvalues[i];
		}
	}
}
