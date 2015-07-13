/*
* tAIImpl.cpp
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#include <ChipObject/tAI.h>
#include <ChipObject/tInterrupt.h>

#include "ChipObject/tAIImpl.h"
#include "ChipObject/NiFpgaState.h"
#include "ChipObject/tAnalogTriggerImpl.h"
#include "ChipObject/NiIRQImpl.h"

#define TAI_DECL_ADDRESSES(x) const int tAI_Impl::k ## x ## _Addresses [] = {kAI0_ ## x ## _Address, kAI1_ ## x ## _Address }

namespace nFPGA {
	TAI_DECL_ADDRESSES(Config);
	TAI_DECL_ADDRESSES(LoopTiming);
	TAI_DECL_ADDRESSES(OversampleBits);
	TAI_DECL_ADDRESSES(AverageBits);
	TAI_DECL_ADDRESSES(ScanList);

	tAI_Impl::tAI_Impl(NiFpgaState *state, unsigned char sys_index) {
		this->state = state;
		this->sys_index = sys_index;

		this->config = (tConfig*) &(state->fpgaRAM[kConfig_Addresses[sys_index]]);
		this->loopTiming = (uint32_t*) &(state->fpgaRAM[kLoopTiming_Addresses[sys_index]]);
		this->oversampleBits = (uint32_t*) &(state->fpgaRAM[kOversampleBits_Addresses[sys_index]]);
		this->averageBits = (uint32_t*) &(state->fpgaRAM[kAverageBits_Addresses[sys_index]]);
		this->scanListBits = (uint32_t*) &(state->fpgaRAM[kScanList_Addresses[sys_index]]);
		this->readSelect = (tReadSelect*) &(state->fpgaRAM[kAI_ReadSelect_Address]);
		this->output = (int32_t*) &(state->fpgaRAM[kAI_Output_Address]);

		// Default things
		(*config).ConvertRate = 100;
		(*config).ScanSize = 8;
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
		*config = value;
		*status =  NiFpga_Status_Success;
	}
	void tAI_Impl::writeConfig_ScanSize(unsigned char value, tRioStatusCode *status){
		(*config).ScanSize = value;
		*status =  NiFpga_Status_Success;
	}
	void tAI_Impl::writeConfig_ConvertRate(unsigned int value, tRioStatusCode *status){
		(*config).ConvertRate = value;
		*status =  NiFpga_Status_Success;
	}
	tAI_Impl::tConfig tAI_Impl::readConfig(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return *config;
	}
	unsigned char tAI_Impl::readConfig_ScanSize(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return (*config).ScanSize;
	}
	unsigned int tAI_Impl::readConfig_ConvertRate(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return (*config).ConvertRate;
	}

	unsigned int tAI_Impl::readLoopTiming(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return *loopTiming;
	}

	void tAI_Impl::writeOversampleBits(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumOversampleBitsElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return;
		}
		*status =  NiFpga_Status_Success;
		const uint32_t shift = (kNumOversampleBitsElements - 1 - bitfield_index) * kOversampleBits_ElementSize;
		uint32_t regValue = *oversampleBits;
		regValue &= ~(kOversampleBits_ElementMask << shift);
		regValue |= ((value & kOversampleBits_ElementMask) << shift);
		*oversampleBits = regValue;
	}
	unsigned char tAI_Impl::readOversampleBits(unsigned char bitfield_index, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumOversampleBitsElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		const uint32_t shift = (kNumOversampleBitsElements - 1 - bitfield_index) * kOversampleBits_ElementSize;
		uint32_t arrayElementValue = ((*oversampleBits)
			>> shift) & kOversampleBits_ElementMask;
		return (unsigned char)((arrayElementValue) & 0x0000000F);
	}

	void tAI_Impl::writeAverageBits(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumAverageBitsElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return;
		}
		*status =  NiFpga_Status_Success;
		const uint32_t shift = (kNumAverageBitsElements - 1 - bitfield_index) * kAverageBits_ElementSize;
		uint32_t regValue = *averageBits;
		regValue &= ~(kAverageBits_ElementMask << shift);
		regValue |= ((value & kAverageBits_ElementMask) << shift);
		*averageBits = regValue;
	}
	unsigned char tAI_Impl::readAverageBits(unsigned char bitfield_index, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumAverageBitsElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		const uint32_t shift = (kNumAverageBitsElements - 1 - bitfield_index) * kAverageBits_ElementSize;
		uint32_t arrayElementValue = ((*averageBits)
			>> shift) & kAverageBits_ElementMask;
		return (unsigned char)((arrayElementValue) & 0x0000000F);
	}

	void tAI_Impl::writeScanList(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumScanListElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return;
		}
		*status =  NiFpga_Status_Success;
		const uint32_t shift = (kNumScanListElements - 1 - bitfield_index) * kScanList_ElementSize;
		uint32_t regValue = *scanListBits;
		regValue &= ~(kScanList_ElementMask << shift);
		regValue |= ((value & kScanList_ElementMask) << ((kNumScanListElements - 1 - bitfield_index) * kScanList_ElementSize));
		*scanListBits = regValue;
	}
	unsigned char tAI_Impl::readScanList(unsigned char bitfield_index, tRioStatusCode *status){
		if (bitfield_index < 0 || bitfield_index >= kNumScanListElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		const uint32_t shift = (kNumScanListElements - 1 - bitfield_index) * kScanList_ElementSize;
		uint32_t arrayElementValue = (*scanListBits
			>> shift) & kScanList_ElementMask;
		return (unsigned char)((arrayElementValue) & 0x00000007);
	}

	void tAI_Impl::writeReadSelect(tReadSelect value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		*readSelect= value;
	}
	void tAI_Impl::writeReadSelect_Channel(unsigned char value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		(*readSelect).Channel = value;
	}
	void tAI_Impl::writeReadSelect_Module(unsigned char value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		(*readSelect).Module = value;
	}
	void tAI_Impl::writeReadSelect_Averaged(bool value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		(*readSelect).Averaged = value;
	}
	tAI_Impl::tReadSelect tAI_Impl::readReadSelect(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return *readSelect;
	}
	unsigned char tAI_Impl::readReadSelect_Channel(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return (*readSelect).Channel;
	}
	unsigned char tAI_Impl::readReadSelect_Module(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return (*readSelect).Module;
	}
	bool tAI_Impl::readReadSelect_Averaged(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return (*readSelect).Averaged;
	}

	signed int tAI_Impl::readOutput(tRioStatusCode *status){
		if ((*readSelect).Channel < 0 || (*readSelect).Channel >= kNumScanListElements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		if ((*readSelect).Module != sys_index) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		return *output;
	}
	void tAI_Impl::strobeLatchOutput(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		NiFpga_WriteU32(0, kAI_LatchOutput_Address, 1);		// Uses the write call so the special strobe action is taken
	}

	void tAI_Impl::updateValues(signed int nvalues[]) {
		tRioStatusCode status;
		for (int i = 0; i < kNumScanListElements; i++) {
			if (nvalues[i] != values[i]) {		// If this channel changed
				tAnalogTrigger_Impl::tOutput changeType;
				changeType.Falling = values[i] > nvalues[i];
				changeType.Rising = values[i] < nvalues[i];
				for (int t = 0; t < tAnalogTrigger_Impl::kNumSystems; t++) {	// For every analog trigger...
					tAnalogTrigger_Impl *trigger = state->analogTrigger[t];
					if (trigger != NULL &&	// If this trigger is on this channel 
						(*(trigger->source)).Module == sys_index && (*(trigger->source)).Channel == i) {	
							changeType.OverLimit = nvalues[i] >= *(trigger->upperLimit);
							changeType.InHysteresis = nvalues[i] >= *(trigger->lowerLimit) && !changeType.OverLimit;
							if (trigger->readOutput(trigger->sys_index, &status).value != changeType.value){
								trigger->writeOutput(trigger->sys_index, changeType, &status);
								// Signal it somehow?  Not entirely sure how this signal is routed. TODO
								// Plan: Shift by 8 bits
								state->getIRQManager()->signal(1 << (trigger->sys_index + nFPGA::nFRC_2012_1_6_4::tInterrupt::kNumSystems));
							}
					}
				}
			}
			values[i] = nvalues[i];
		}
	}
}
