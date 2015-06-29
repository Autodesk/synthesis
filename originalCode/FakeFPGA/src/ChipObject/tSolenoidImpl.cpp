/*
* tSolenoidImpl.cpp
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#include "ChipObject/tSolenoidImpl.h"
#include "ChipObject/NiFpgaState.h"

namespace nFPGA {
	tSolenoid_Impl::tSolenoid_Impl(NiFpgaState *state) {
		this->state = state;
		this->solenoidState = (uint32_t*) &(state->fpgaRAM[kSolenoid_DO7_0_Address]);

		*solenoidState = 0;
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
		if (bitfield_index < 0 || bitfield_index >= kNumDO7_0Elements) {
			*status = NiFpga_Status_ResourceNotFound;
			return;
		}
		*status =  NiFpga_Status_Success;
		uint32_t shift = (kNumDO7_0Elements - 1 - bitfield_index) * kDO7_0_ElementSize;
		uint32_t regValue = *solenoidState;
		regValue &= ~(kDO7_0_ElementMask << shift);
		regValue |= ((value & kDO7_0_ElementMask) << shift);
		*solenoidState = regValue;
	}

	unsigned char tSolenoid_Impl::readDO7_0(unsigned char bitfield_index, tRioStatusCode *status) {
		if (bitfield_index < 0 || bitfield_index >= kNumDO7_0Elements) {
			*status = NiFpga_Status_ResourceNotFound;
			return 0;
		}
		*status =  NiFpga_Status_Success;
		uint32_t shift = (kNumDO7_0Elements - 1 - bitfield_index) * kDO7_0_ElementSize;
		uint32_t arrayElementValue = ((*solenoidState) >> shift) & kDO7_0_ElementMask;
		return arrayElementValue & 0x000000FF;
	}

}
