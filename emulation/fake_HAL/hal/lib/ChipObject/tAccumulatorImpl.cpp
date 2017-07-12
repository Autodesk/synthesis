#include <ChipObject/tAccumulatorImpl.h>
#include "ChipObject/NiFpgaState.h"
#include <stdio.h>

#define TACCUMULATOR_DECL_ADDRESS(x) const int tAccumulator_Impl::k ## x ## _Addresses [] = {kAccumulator0_ ## x ## _Address, kAccumulator1_ ## x ## _Address }
namespace nFPGA {
	TACCUMULATOR_DECL_ADDRESS(Output);
	TACCUMULATOR_DECL_ADDRESS(Center);
	TACCUMULATOR_DECL_ADDRESS(Deadband);
	TACCUMULATOR_DECL_ADDRESS(Reset);

	tAccumulator_Impl::tAccumulator_Impl(NiFpgaState *state, unsigned char sys_index) {
		this->state = state;
		this->sys_index = sys_index;

		this->deadband = (int32_t*) &(state->fpgaRAM[kDeadband_Addresses[sys_index]]);
		this->center = (int32_t*) &(state->fpgaRAM[kCenter_Addresses[sys_index]]);

		*deadband = 0;
		*center = 0;
		output.Value = 0;
		output.Count = 0;

		outputChunk = 0;	// Freaking WPILib hacks.
	}

	tAccumulator_Impl::~tAccumulator_Impl() {
		if (state->accum[sys_index] == this) {
			state->accum[sys_index] = NULL;
		}
	}

	tSystemInterface *tAccumulator_Impl::getSystemInterface() {
		return state;
	}

	unsigned char tAccumulator_Impl::getSystemIndex() {
		return sys_index;
	}

	tAccumulator_Impl::tOutput tAccumulator_Impl::readOutput(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return output;
	}
	signed long long tAccumulator_Impl::readOutput_Value(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return output.Value;
	}
	unsigned int tAccumulator_Impl::readOutput_Count(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return output.Count;
	}

	void tAccumulator_Impl::writeCenter(signed int value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		*center = value;
	}
	signed int tAccumulator_Impl::readCenter(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return *center;
	}

	void tAccumulator_Impl::writeDeadband(signed int value, tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		*deadband = value;
	}
	signed int tAccumulator_Impl::readDeadband(tRioStatusCode *status){
		*status =  NiFpga_Status_Success;
		return *deadband;
	}

	void tAccumulator_Impl::strobeReset(tRioStatusCode *status){
		NiFpga_WriteU32(state->getHandle(), kReset_Addresses[sys_index], 1);		// Use WriteU32 to trigger strobe
		*status =  NiFpga_Status_Success;
	}

	uint32_t tAccumulator_Impl::readOutputChunk() {
		outputChunk++;
		switch (outputChunk) {
		case 1:
			return output.value;
		case 2:
			return output.value2;
		case 3:
			outputChunk = 0;
			return output.value3;
		default:
			outputChunk = 0; // Something died
		}
	}
} /* namespace nFPGA */
