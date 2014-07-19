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

namespace nFPGA {

NiFpgaState::NiFpgaState() {
	ai = NULL;
	solenoid = NULL;
	dio = new tDIO_Impl*[DIO_COUNT];
	ai = new tAI_Impl*[ANALOG_COUNT];
	solenoid = NULL;

	for (int i = 0; i < DIO_COUNT; i++) {
		dio[i] = NULL;
	}
	for (int i = 0; i < ANALOG_COUNT; i++) {
		ai[i] = NULL;
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

tSolenoid_Impl *NiFpgaState::getSolenoid() {
	if (solenoid == NULL) {
		//solenoid[module] = new tSolenoid_Impl(this, module);
	}
	return solenoid;
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
