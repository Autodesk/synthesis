/*
* NiFpgaState.cpp
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#include <ChipObject/NiFpgaState.h>

#include "ChipObject/tDIOImpl.h"
#include "ChipObject/tAIImpl.h"
#include "ChipObject/tSolenoidImpl.h"
#include "ChipObject/tGlobalImpl.h"
#include "ChipObject/tEncoderImpl.h"
#include "ChipObject/tAccumulatorImpl.h"
#include "ChipObject/tInterruptImpl.h"
#include "ChipObject/tCounterImpl.h"
#include "ChipObject/tAnalogTriggerImpl.h"
#include "ChipObject/tAlarmImpl.h"

#include "ChipObject/NiIRQImpl.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h> // memset and memcpy

namespace nFPGA {
	const uint32_t NiFpgaState::kExpectedFPGASignature[] =
	{
		0xA14C11BD,
		0xE4BB64AE,
		0xF6A86FC5,
		0x2A294CD9,
	};

	NiFpgaState::NiFpgaState() {
		irqManager = new NiIRQ_Impl();

		fpgaRAM = (char*) malloc(FPGA_RAM_SIZE);
		memset(fpgaRAM, 0, FPGA_RAM_SIZE);

		sigChunk = 0;

		solenoid = new tSolenoid_Impl(this);
		dio = new tDIO_Impl*[tDIO_Impl::kNumSystems];
		ai = new tAI_Impl*[tAI_Impl::kNumSystems];
		accum = new tAccumulator_Impl*[tAccumulator_Impl::kNumSystems];
		encoder = new tEncoder_Impl*[tEncoder_Impl::kNumSystems];
		interrupt = new tInterrupt_Impl*[tInterrupt_Impl::kNumSystems];
		counter = new tCounter_Impl*[tCounter_Impl::kNumSystems];
		analogTrigger = new tAnalogTrigger_Impl*[tAnalogTrigger_Impl::kNumSystems];
		alarm = new tAlarm_Impl(this);
		solenoid = new tSolenoid_Impl(this);
		global = new tGlobal_Impl(this);

		for (int i = 0; i < tDIO_Impl::kNumSystems; i++) {
			dio[i] = new tDIO_Impl(this, i);
		}
		for (int i = 0; i < tAI_Impl::kNumSystems;i++) {
			ai[i] = new tAI_Impl(this, i);
		}
		for (int i = 0; i < tAccumulator_Impl::kNumSystems; i++) {
			accum[i] = new tAccumulator_Impl(this, i);
		}
		for (int i = 0; i < tEncoder_Impl::kNumSystems; i++) {
			encoder[i] = new tEncoder_Impl(this, i);
		}
		for (int i = 0; i < tInterrupt_Impl::kNumSystems; i++) {
			interrupt[i] = new tInterrupt_Impl(this, i);
		}
		for (int i = 0; i < tCounter_Impl::kNumSystems; i++) {
			counter[i] = new tCounter_Impl(this, i);
		}
		for (int i = 0; i < tAnalogTrigger_Impl::kNumSystems; i++) {
			analogTrigger[i] = new tAnalogTrigger_Impl(this, i);
		}
	}

	NiFpgaState::~NiFpgaState() {
		delete[] accum;
		delete[] ai;
		delete[] dio;
		delete[] encoder;
		delete[] interrupt;
		delete[] analogTrigger;
		if (solenoid != NULL){
			delete solenoid;
		}
		if (global != NULL) {
			delete global;
		}
		if (alarm != NULL){
			delete alarm;
		}
		if (irqManager != NULL) {
			delete irqManager;
		}
		delete fpgaRAM;
	}

	NiIRQ_Impl *NiFpgaState::getIRQManager() {
		return irqManager;
	}

	tDIO_Impl *NiFpgaState::getDIO(unsigned char module) {
		if (dio[module] == NULL) {
			dio[module] = new tDIO_Impl(this, module);
		}
		return dio[module];
	}

	tAI_Impl *NiFpgaState::getAnalog(unsigned char module) {
		if (ai[module] == NULL) {
			ai[module] = new tAI_Impl(this, module);
		}
		return ai[module];
	}

	tAccumulator_Impl *NiFpgaState::getAccumulator(unsigned char sys_index) {
		if (accum[sys_index] == NULL) {
			accum[sys_index] = new tAccumulator_Impl(this, sys_index);
		}
		return accum[sys_index];
	}

	tSolenoid_Impl *NiFpgaState::getSolenoid() {
		if (solenoid == NULL) {
			solenoid = new tSolenoid_Impl(this);
		}
		return solenoid;
	}

	tGlobal_Impl *NiFpgaState::getGlobal() {
		if (global == NULL) {
			global = new tGlobal_Impl(this);
		}
		return global;
	}

	tEncoder_Impl *NiFpgaState::getEncoder(unsigned char sys_index) {
		if (encoder[sys_index] == NULL) {
			encoder[sys_index] = new tEncoder_Impl(this, sys_index);
		}
		return encoder[sys_index];
	}

	tInterrupt_Impl *NiFpgaState::getInterrupt(unsigned char sys_index) {
		if (interrupt[sys_index] == NULL) {
			interrupt[sys_index] = new tInterrupt_Impl(this, sys_index);
		}
		return interrupt[sys_index];
	}

	tCounter_Impl *NiFpgaState::getCounter(unsigned char sys_index) {
		if (counter[sys_index] == NULL) {
			counter[sys_index] = new tCounter_Impl(this, sys_index);
		}
		return counter[sys_index];
	}

	tAnalogTrigger_Impl *NiFpgaState::getAnalogTrigger(unsigned char sys_index) {
		if (analogTrigger[sys_index] == NULL) {
			analogTrigger[sys_index] = new tAnalogTrigger_Impl(this, sys_index);
		}
		return analogTrigger[sys_index];
	}

	tAlarm_Impl *NiFpgaState::getAlarm() {
		if (alarm == NULL) {
			alarm = new tAlarm_Impl(this);
		}
		return alarm;
	}

	const uint16_t NiFpgaState::getExpectedFPGAVersion() {
		return kExpectedFPGAVersion;
	}

	const uint32_t NiFpgaState::getExpectedFPGARevision() {
		return kExpectedFPGARevision;
	}

	const uint32_t* const NiFpgaState::getExpectedFPGASignature() {
		return &kExpectedFPGASignature[0];
	}

	void NiFpgaState::getHardwareFpgaSignature(uint32_t* guid_ptr,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
		memcpy(guid_ptr, kExpectedFPGASignature, sizeof(uint32_t) * 4);
	}

	uint32_t NiFpgaState::getLVHandle(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return (uint32_t) this;
	}

	uint32_t NiFpgaState::getHandle() {
		return (uint32_t) this;
	}

	uint32_t NiFpgaState::readSignatureChunk() {
		uint32_t val = kExpectedFPGASignature[sigChunk];
		sigChunk = (sigChunk + 1) % 4;
		return val;
	}
} /* namespace nFPGA */
