#include "ChipObject/tAnalogTriggerImpl.h"
#include "ChipObject/NiFpgaState.h"

#define TANALOG_TRIGGER_DECL_ADDRESSES(x) const int tAnalogTrigger_Impl::k ## x ## _Addresses [] = {kAnalogTrigger0_ ## x ## _Address, kAnalogTrigger1_ ## x ## _Address, kAnalogTrigger2_ ## x ## _Address, kAnalogTrigger3_ ## x ## _Address, kAnalogTrigger4_ ## x ## _Address, kAnalogTrigger5_ ## x ## _Address, kAnalogTrigger6_ ## x ## _Address, kAnalogTrigger7_ ## x ## _Address }

namespace nFPGA {
	TANALOG_TRIGGER_DECL_ADDRESSES(SourceSelect);
	TANALOG_TRIGGER_DECL_ADDRESSES(LowerLimit);
	TANALOG_TRIGGER_DECL_ADDRESSES(UpperLimit);

	tAnalogTrigger_Impl::tAnalogTrigger_Impl(NiFpgaState *state, unsigned char sys_index) {
		this->state = state;
		this->sys_index = sys_index;

		this->lowerLimit = (int32_t*) &(state->fpgaRAM[kLowerLimit_Addresses[sys_index]]);
		this->upperLimit = (int32_t*) &(state->fpgaRAM[kUpperLimit_Addresses[sys_index]]);
		this->source = (tSourceSelect*) &(state->fpgaRAM[kSourceSelect_Addresses[sys_index]]);

		*lowerLimit = 0;
		*upperLimit = 0;
		(*source).value = 0;
	}

	tAnalogTrigger_Impl::~tAnalogTrigger_Impl() {
		if (this->state->analogTrigger[this->sys_index] == this) {
			this->state->analogTrigger[this->sys_index] = NULL;
		}
	}

	tSystemInterface *tAnalogTrigger_Impl::getSystemInterface() {
		return state;
	}

	unsigned char tAnalogTrigger_Impl::getSystemIndex() {
		return sys_index;
	}

#pragma region writeSourceSelect
	void tAnalogTrigger_Impl::writeSourceSelect(tSourceSelect value, tRioStatusCode *status)
	{ 
		*status = NiFpga_Status_Success; 
		*source = value;
	}
	void tAnalogTrigger_Impl::writeSourceSelect_Channel(unsigned char value, tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		(*source).Channel = value; 
	}
	void tAnalogTrigger_Impl::writeSourceSelect_Module(unsigned char value, tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		(*source).Module = value; 
	}
	void tAnalogTrigger_Impl::writeSourceSelect_Averaged(bool value, tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		(*source).Averaged = value; 
	}
	void tAnalogTrigger_Impl::writeSourceSelect_Filter(bool value, tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		(*source).Filter = value; 
	}
	void tAnalogTrigger_Impl::writeSourceSelect_FloatingRollover(bool value, tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		(*source).FloatingRollover = value; 
	}
	void tAnalogTrigger_Impl::writeSourceSelect_RolloverLimit(signed short value, tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		(*source).RolloverLimit = value; 
	}
#pragma endregion

#pragma region readSourceSelect
	tAnalogTrigger_Impl::tSourceSelect tAnalogTrigger_Impl::readSourceSelect(tRioStatusCode *status){
		*status = NiFpga_Status_Success; 
		return *source;
	}
	unsigned char tAnalogTrigger_Impl::readSourceSelect_Channel(tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		return (*source).Channel; 
	}
	unsigned char tAnalogTrigger_Impl::readSourceSelect_Module(tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		return (*source).Module; 
	}
	bool tAnalogTrigger_Impl::readSourceSelect_Averaged(tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		return (*source).Averaged; 
	}
	bool tAnalogTrigger_Impl::readSourceSelect_Filter(tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		return (*source).Filter; 
	}
	bool tAnalogTrigger_Impl::readSourceSelect_FloatingRollover(tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		return (*source).FloatingRollover; 
	}
	signed short tAnalogTrigger_Impl::readSourceSelect_RolloverLimit(tRioStatusCode *status) 
	{ 
		*status = NiFpga_Status_Success; 
		return (*source).RolloverLimit; 
	}
#pragma endregion

#pragma region rwLimits
	void tAnalogTrigger_Impl::writeUpperLimit(signed int value, tRioStatusCode *status)
	{ 
		*status = NiFpga_Status_Success; 
		*upperLimit = value;
	}
	signed int tAnalogTrigger_Impl::readUpperLimit(tRioStatusCode *status)
	{ 
		*status = NiFpga_Status_Success; 
		return *upperLimit;
	}

	void tAnalogTrigger_Impl::writeLowerLimit(signed int value, tRioStatusCode *status)
	{ 
		*status = NiFpga_Status_Success; 
		*lowerLimit = value;
	}
	signed int tAnalogTrigger_Impl::readLowerLimit(tRioStatusCode *status)
	{ 
		*status = NiFpga_Status_Success; 
		return *lowerLimit;
	}
#pragma endregion

#pragma region readOutput
	tAnalogTrigger_Impl::tOutput tAnalogTrigger_Impl::readOutput(unsigned char bitfield_index, tRioStatusCode *status)
	{ 
		*status = NiFpga_Status_Success; 
		uint32_t result = *((uint32_t*)&state->fpgaRAM[kAnalogTrigger_Output_Address]);
		tOutput out;
		out.value = result  >> ((kNumOutputElements - 1 - bitfield_index) * kOutput_ElementSize);
		return out;
	}
	bool tAnalogTrigger_Impl::readOutput_InHysteresis(unsigned char bitfield_index, tRioStatusCode *status) 
	{ 
		return readOutput(bitfield_index, status).InHysteresis;
	}
	bool tAnalogTrigger_Impl::readOutput_OverLimit(unsigned char bitfield_index, tRioStatusCode *status) 
	{ 
		return readOutput(bitfield_index, status).OverLimit;
	}
	bool tAnalogTrigger_Impl::readOutput_Rising(unsigned char bitfield_index, tRioStatusCode *status) 
	{
		return readOutput(bitfield_index, status).Rising;
	}
	bool tAnalogTrigger_Impl::readOutput_Falling(unsigned char bitfield_index, tRioStatusCode *status) 
	{ 
		return readOutput(bitfield_index, status).Falling;
	}

	void tAnalogTrigger_Impl::writeOutput(unsigned char bitfield_index, tOutput output, tRioStatusCode *status) {
		*status = NiFpga_Status_Success; 
		uint32_t regValue = *((uint32_t*)&state->fpgaRAM[kAnalogTrigger_Output_Address]);
		uint32_t shift = ((kNumOutputElements - 1 - bitfield_index) * kOutput_ElementSize);
		regValue &= ~(kOutput_ElementMask << shift);
		regValue |= output.value << shift;
		*((uint32_t*)&state->fpgaRAM[kAnalogTrigger_Output_Address]) = regValue;
	}
#pragma endregion
}