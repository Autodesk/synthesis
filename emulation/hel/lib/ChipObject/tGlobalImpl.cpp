/*
* tGlobalImpl.cpp
*
*  Created on: Jul 19, 2014
*      Author: localadmin
*/

#include <ChipObject/NiFpgaState.h>
#include <ChipObject/tGlobalImpl.h>
#include <OSAL/System.h>
#include <stdio.h>

namespace nFPGA {

	tGlobal_Impl::tGlobal_Impl(NiFpgaState* state) {
		this->state = state;
	}

	tGlobal_Impl::~tGlobal_Impl() {
		if (this->state->global == this) {
			this->state->global = NULL;
			fprintf(stderr, "DELETED THE SHARED INSTANCE OF tGlobal.  THIS SHOULD NEVER HAPPEN.  DON'T DO THIS.\n");
		}
	}

	tSystemInterface* tGlobal_Impl::getSystemInterface() {
		return state;
	}

	unsigned short tGlobal_Impl::readVersion(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return FPGA_VERSION;
	}

	unsigned int tGlobal_Impl::readLocalTime(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		// Time in microseconds since start.
		return threadTimeMicros();
	}

	void tGlobal_Impl::writeFPGA_LED(bool value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
	}

	bool tGlobal_Impl::readFPGA_LED(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return false;
	}

	unsigned int tGlobal_Impl::readRevision(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return FPGA_REVISION;
	}

} /* namespace nFPGA */
