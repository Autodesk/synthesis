/*
 * tSolenoidImpl.cpp
 *
 *  Created on: Jul 18, 2014
 *      Author: localadmin
 */

#include "tSolenoidImpl.h"
#include "NiFpgaState.h"

namespace nFPGA {

tSolenoid_Impl::tSolenoid_Impl(NiFpgaState *state) {
	this->state = state;
	for (int i = 0; i<MAX_SOLENOID_MODULES; i++){
		solenoidState[i] = 0;
	}
}

tSolenoid_Impl::~tSolenoid_Impl() {
	if (this->state->solenoid == this) {
		this->state->solenoid = NULL;
	}
}

tSystemInterface *tSolenoid_Impl::getSystemInterface() {
	return this->state;
}

void tSolenoid_Impl::writeDO7_0(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status) {
	if (bitfield_index < 0 || bitfield_index >= MAX_SOLENOID_MODULES) {
		*status = NiFpga_Status_ResourceNotFound;
		return;
	}
	solenoidState[bitfield_index] = value;
	status = NiFpga_Status_Success;
}

unsigned char tSolenoid_Impl::readDO7_0(unsigned char bitfield_index, tRioStatusCode *status) {
	if (bitfield_index < 0 || bitfield_index >= MAX_SOLENOID_MODULES) {
		*status = NiFpga_Status_ResourceNotFound;
		return 0;
	}
	status = NiFpga_Status_Success;
	return solenoidState[bitfield_index];
}

}
