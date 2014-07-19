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
#include <stdio.h>

namespace nFPGA {

NiFpgaState::NiFpgaState() {
	ai = NULL;
	solenoid = NULL;
	dio = new tDIO_Impl*[DIO_COUNT];
	ai = new tAI_Impl*[ANALOG_COUNT];
	accum = new tAccumulator_Impl*[ACCUM_COUNT];
	solenoid = NULL;
	global = NULL;

	for (int i = 0; i < DIO_COUNT; i++) {
		dio[i] = NULL;
	}
	for (int i = 0; i < ANALOG_COUNT;i++) {
		ai[i] = NULL;
	}
	for (int i = 0; i < ACCUM_COUNT; i++) {
		accum[i] = NULL;
	}
}

NiFpgaState::~NiFpgaState() {
}

tDIO_Impl *NiFpgaState::getDIO(unsigned char module) {
	if (dio[module] == NULL) {
		dio[module] = new tDIO_Impl(this, module);
	}
	return dio[module];
}

tAI_Impl *NiFpgaState::getAnalog(unsigned char module) {
	if (ai[module] == NULL) {
		//ai[module] = new tAI_Impl(this, module);
	}
	return ai[module];
}

tAccumulator_Impl *NiFpgaState::getAccumulator(unsigned char sys_index) {
	if (accum[sys_index] == NULL) {
		//accum[sys_index] = new tAccumulator_Impl(this, sys_index);
	}
	return accum[sys_index];
}

tSolenoid_Impl *NiFpgaState::getSolenoid() {
	if (solenoid == NULL) {
		//solenoid[module] = new tSolenoid_Impl(this);
	}
	return solenoid;
}

tGlobal_Impl *NiFpgaState::getGlobal() {
	if (global == NULL) {
		global = new tGlobal_Impl(this);
	}
	return global;
}

const uint16_t NiFpgaState::getExpectedFPGAVersion() {
	return 0;
}

const uint32_t NiFpgaState::getExpectedFPGARevision() {
	return 0;
}

const uint32_t* const NiFpgaState::getExpectedFPGASignature() {
	return new uint32_t[3];
}

void NiFpgaState::getHardwareFpgaSignature(uint32_t* guid_ptr,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
}

uint32_t NiFpgaState::getLVHandle(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return 0;
}

uint32_t NiFpgaState::getHandle() {
	return 0;
}

} /* namespace nFPGA */
