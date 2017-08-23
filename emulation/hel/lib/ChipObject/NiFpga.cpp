#include <stdlib.h>
#include <string.h>	//memcpy and memset

// Definitions
#include <ChipObject/NiFpga.h>
#include <ChipObject/tAlarm.h>
#include <ChipObject/tDIO.h>
#include <ChipObject/tGlobal.h>
#include <ChipObject/tSolenoid.h>
#include <ChipObject/tAI.h>
#include <ChipObject/tAccumulator.h>
#include <ChipObject/tWatchdog.h>
#include <ChipObject/tEncoder.h>
#include <ChipObject/tInterrupt.h>
#include <ChipObject/tCounter.h>
#include <ChipObject/tAnalogTrigger.h>

// Implementations
#include "ChipObject/NiFakeFpga.h"
#include "ChipObject/NiFpgaState.h"
#include "ChipObject/NiIRQImpl.h"
#include "ChipObject/tAIImpl.h"
#include "ChipObject/tGlobalImpl.h"
#include "ChipObject/tAlarmImpl.h"
#include "ChipObject/tWatchcatImpl.h"
#include "ChipObject/tEncoderImpl.h"
#include "ChipObject/tInterruptImpl.h"
#include "ChipObject/tCounterImpl.h"
#include "ChipObject/tAnalogTriggerImpl.h"
#include "ChipObject/tAccumulatorImpl.h"

extern "C" {
	nFPGA::NiFpgaState *frcFPGAInstance = NULL;
}

nFPGA::NiFpgaState *GetFakeFPGA() {
	if (frcFPGAInstance == NULL) {
		frcFPGAInstance = new nFPGA::NiFpgaState();
	}
	return frcFPGAInstance;
}

nFPGA::nFRC_2012_1_6_4::tDIO *nFPGA::nFRC_2012_1_6_4::tDIO::create(
	unsigned char sys_index, tRioStatusCode *status) {
		*status =  NiFpga_Status_Success;
		return (nFPGA::nFRC_2012_1_6_4::tDIO*) GetFakeFPGA()->getDIO(sys_index);
}

nFPGA::nFRC_2012_1_6_4::tAI *nFPGA::nFRC_2012_1_6_4::tAI::create(
	unsigned char sys_index, tRioStatusCode *status) {
		*status =  NiFpga_Status_Success;
		return (nFPGA::nFRC_2012_1_6_4::tAI*) GetFakeFPGA()->getAnalog(sys_index);
}

nFPGA::nFRC_2012_1_6_4::tSolenoid *nFPGA::nFRC_2012_1_6_4::tSolenoid::create(
	tRioStatusCode *status) {
		*status =  NiFpga_Status_Success;
		return (nFPGA::nFRC_2012_1_6_4::tSolenoid*) GetFakeFPGA()->getSolenoid();
}

nFPGA::nFRC_2012_1_6_4::tAccumulator *nFPGA::nFRC_2012_1_6_4::tAccumulator::create(
	unsigned char sys_index, tRioStatusCode *status) {
		*status =  NiFpga_Status_Success;
		return (nFPGA::nFRC_2012_1_6_4::tAccumulator*) GetFakeFPGA()->getAccumulator(
			sys_index);
}

nFPGA::nFRC_2012_1_6_4::tGlobal *nFPGA::nFRC_2012_1_6_4::tGlobal::create(
	tRioStatusCode *status) {
		*status =  NiFpga_Status_Success;
		//return (nFPGA::nFRC_2012_1_6_4::tGlobal *) GetFakeFPGA()->getGlobal();
		// This causes issues with invalid access when another thread deletes it while another one tries to read it.
		// Globals are typically acquired, then immediately deleted.  Possible mem leak tho
		// Very slow mem leak.  4k over 5 minutes
		return new tGlobal_Impl(GetFakeFPGA());
}

nFPGA::nFRC_2012_1_6_4::tAlarm *nFPGA::nFRC_2012_1_6_4::tAlarm::create(
	tRioStatusCode *status) {
		*status =  NiFpga_Status_Success;
		return GetFakeFPGA()->getAlarm();
}

nFPGA::nFRC_2012_1_6_4::tWatchdog *nFPGA::nFRC_2012_1_6_4::tWatchdog::create(
	tRioStatusCode *status) {
		*status = NiFpga_Status_Success;
		return new tWatchcat_Impl(GetFakeFPGA());
}

nFPGA::nFRC_2012_1_6_4::tEncoder *nFPGA::nFRC_2012_1_6_4::tEncoder::create(unsigned char sys_index, tRioStatusCode *status) {
	*status = NiFpga_Status_Success;
	return GetFakeFPGA()->getEncoder(sys_index);
}

nFPGA::nFRC_2012_1_6_4::tInterrupt *nFPGA::nFRC_2012_1_6_4::tInterrupt::create(unsigned char sys_index, tRioStatusCode *status) {
	*status = NiFpga_Status_Success;
	return GetFakeFPGA()->getInterrupt(sys_index);
}

nFPGA::nFRC_2012_1_6_4::tCounter *nFPGA::nFRC_2012_1_6_4::tCounter::create(unsigned char sys_index, tRioStatusCode *status) {
	*status = NiFpga_Status_Success;
	return GetFakeFPGA()->getCounter(sys_index);
}

nFPGA::nFRC_2012_1_6_4::tAnalogTrigger *nFPGA::nFRC_2012_1_6_4::tAnalogTrigger::create(unsigned char sys_index, tRioStatusCode *status) {
	*status = NiFpga_Status_Success;
	return GetFakeFPGA()->getAnalogTrigger(sys_index);
}


NiFpga_Status NiFpga_Initialize(void) {
	// Setup FPGA
	GetFakeFPGA();

	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_Finalize(void) {
	// Should delete the fake FPGA.
	delete frcFPGAInstance;
	frcFPGAInstance = NULL;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_Open(const char* bitfile, const char* signature, const char* resource, uint32_t attribute, NiFpga_Session* session) {
	exit(1);
	return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_Close(NiFpga_Session session, uint32_t attribute) {
	exit(1);
	return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_Run(NiFpga_Session session, uint32_t attribute) {
	exit(1);
	return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_Abort(NiFpga_Session session) {
	exit(1);
	return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_Reset(NiFpga_Session session) {
	exit(1);
	return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_Download(NiFpga_Session session) {
	exit(1);
	return NiFpga_Status_ResourceNotFound;
}

#pragma region MEM_PRIMITIVE
NiFpga_Status NiFpga_ReadBool(NiFpga_Session session, uint32_t indicator, NiFpga_Bool* value) {
	*value = GetFakeFPGA()->fpgaRAM[indicator];
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadI8(NiFpga_Session session, uint32_t indicator, int8_t* value) {
	*value = GetFakeFPGA()->fpgaRAM[indicator];
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadU8(NiFpga_Session session, uint32_t indicator, uint8_t* value) {
	*value = GetFakeFPGA()->fpgaRAM[indicator];
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadI16(NiFpga_Session session, uint32_t indicator, int16_t* value) {
	*value = *((int16_t*) &(GetFakeFPGA()->fpgaRAM[indicator]));
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadU16(NiFpga_Session session, uint32_t indicator, uint16_t* value) {
	*value = *((uint16_t*) &(GetFakeFPGA()->fpgaRAM[indicator]));
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadI32(NiFpga_Session session, uint32_t indicator, int32_t* value) {
	*value = *((int32_t*) &(GetFakeFPGA()->fpgaRAM[indicator]));
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadU32(NiFpga_Session session, uint32_t indicator, uint32_t* value) {
	*value = *((uint32_t*) &(GetFakeFPGA()->fpgaRAM[indicator]));
	switch (indicator) {
	case nFPGA::tAccumulator_Impl::kAccumulator0_Output_Address:
		*value = GetFakeFPGA()->getAccumulator(0)->readOutputChunk();
		break;
	case nFPGA::tAccumulator_Impl::kAccumulator1_Output_Address:
		*value = GetFakeFPGA()->getAccumulator(1)->readOutputChunk();
		break;
	case nFPGA::NiFpgaState::kFPGA_SIGNATURE_REGISTER:
		*value = GetFakeFPGA()->readSignatureChunk();
		break;
	}
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadI64(NiFpga_Session session, uint32_t indicator, int64_t* value) {
	*value = *((int64_t*) &(GetFakeFPGA()->fpgaRAM[indicator]));
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadU64(NiFpga_Session session, uint32_t indicator, uint64_t* value) {
	*value = *((uint64_t*) &(GetFakeFPGA()->fpgaRAM[indicator]));
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteBool(NiFpga_Session session, uint32_t control, NiFpga_Bool value) {
	GetFakeFPGA()->fpgaRAM[control] = value;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteI8(NiFpga_Session session, uint32_t control, int8_t value) {
	GetFakeFPGA()->fpgaRAM[control] = value;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteU8(NiFpga_Session session, uint32_t control, uint8_t value) {
	GetFakeFPGA()->fpgaRAM[control] = value;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteI16(NiFpga_Session session, uint32_t control, int16_t value) {
	*((int16_t*)(&GetFakeFPGA()->fpgaRAM[control])) = value;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteU16(NiFpga_Session session, uint32_t control, uint16_t value) {
	*((uint16_t*)(&GetFakeFPGA()->fpgaRAM[control])) = value;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteI32(NiFpga_Session session, uint32_t control, int32_t value) {
	*((int32_t*)(&GetFakeFPGA()->fpgaRAM[control])) = value;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteU32(NiFpga_Session session, uint32_t control, uint32_t value) {
	// Special strobe cases
	switch (control) {
	case nFPGA::tAI_Impl::kAI_LatchOutput_Address:
		if (value) {	// We are latching to a read select
			tRioStatusCode status;
			nFPGA::tAI_Impl::tReadSelect readSelect = GetFakeFPGA()->getAnalog(0)->readReadSelect(&status);
			NiFpga_WriteI32(session, nFPGA::tAI_Impl::kAI_Output_Address, GetFakeFPGA()->getAnalog(readSelect.Module)->values[readSelect.Channel]);
			value = 0;	// The strobe action is taken, don't set the bit
		}
		break;	
	case nFPGA::tCounter_Impl::kCounter0_Reset_Address:
	case nFPGA::tCounter_Impl::kCounter1_Reset_Address:
	case nFPGA::tCounter_Impl::kCounter2_Reset_Address:
	case nFPGA::tCounter_Impl::kCounter3_Reset_Address:
	case nFPGA::tCounter_Impl::kCounter4_Reset_Address:
	case nFPGA::tCounter_Impl::kCounter5_Reset_Address:
	case nFPGA::tCounter_Impl::kCounter6_Reset_Address:
	case nFPGA::tCounter_Impl::kCounter7_Reset_Address:
		{
			for (int cid = 0; cid<nFPGA::tCounter_Impl::kNumSystems; cid++) {
				if (control == nFPGA::tCounter_Impl::kReset_Addresses[cid]) {
					// TODO Do the reset action
					value = 0;	// Strobe performed
					break;
				}
			}
		}
		break;
#pragma region ENCODER_RESET
	case nFPGA::tEncoder_Impl::kEncoder0_Reset_Address:
		// Do the reset action
		GetFakeFPGA()->getEncoder(0)->doReset();
		value = 0;	// Strobe performed
		break;
	case nFPGA::tEncoder_Impl::kEncoder1_Reset_Address:
		// Do the reset action
		GetFakeFPGA()->getEncoder(1)->doReset();
		value = 0;	// Strobe performed
		break;
	case nFPGA::tEncoder_Impl::kEncoder2_Reset_Address:
		// Do the reset action
		GetFakeFPGA()->getEncoder(2)->doReset();
		value = 0;	// Strobe performed
		break;
	case nFPGA::tEncoder_Impl::kEncoder3_Reset_Address:
		// Do the reset action
		GetFakeFPGA()->getEncoder(3)->doReset();
		value = 0;	// Strobe performed
		break;
#pragma endregion
#pragma region ACCUMULATOR_RESET
	case nFPGA::tAccumulator_Impl::kAccumulator0_Reset_Address:
		{
			GetFakeFPGA()->getAccumulator(0)->output.Value = 0;
			GetFakeFPGA()->getAccumulator(0)->output.Count = 0;
			value = 0; // Strobe performed.
		}
		break;
	case nFPGA::tAccumulator_Impl::kAccumulator1_Reset_Address:
		{
			GetFakeFPGA()->getAccumulator(1)->output.Value = 0;
			GetFakeFPGA()->getAccumulator(1)->output.Count = 0;
			value = 0; // Strobe performed.
		}
		break;
#pragma endregion
	default:
		{}
	}
	// Update value
	*((uint32_t*)(&GetFakeFPGA()->fpgaRAM[control])) = value;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteI64(NiFpga_Session session, uint32_t control, int64_t value) {
	*((int64_t*)(&GetFakeFPGA()->fpgaRAM[control])) = value;
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteU64(NiFpga_Session session, uint32_t control, uint64_t value) {
	*((uint64_t*)(&GetFakeFPGA()->fpgaRAM[control])) = value;
	return NiFpga_Status_Success;
}
#pragma endregion

#pragma region MEM_ARRAY
NiFpga_Status NiFpga_ReadArrayBool(NiFpga_Session session, uint32_t indicator, NiFpga_Bool* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(NiFpga_Bool) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadArrayI8(NiFpga_Session session, uint32_t indicator, int8_t* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(int8_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadArrayU8(NiFpga_Session session, uint32_t indicator, uint8_t* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(uint8_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadArrayI16(NiFpga_Session session, uint32_t indicator, int16_t* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(int16_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadArrayU16(NiFpga_Session session, uint32_t indicator, uint16_t* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(uint16_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadArrayI32(NiFpga_Session session, uint32_t indicator, int32_t* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(int32_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadArrayU32(NiFpga_Session session, uint32_t indicator, uint32_t* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(uint32_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadArrayI64(NiFpga_Session session, uint32_t indicator, int64_t* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(int64_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_ReadArrayU64(NiFpga_Session session, uint32_t indicator, uint64_t* array, size_t size) {
	memcpy(array, &(GetFakeFPGA()->fpgaRAM[indicator]), sizeof(uint64_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayBool(NiFpga_Session session, uint32_t control, const NiFpga_Bool* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(NiFpga_Bool) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayI8(NiFpga_Session session, uint32_t control, const int8_t* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(int8_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayU8(NiFpga_Session session, uint32_t control, const uint8_t* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(uint8_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayI16(NiFpga_Session session, uint32_t control, const int16_t* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(int16_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayU16(NiFpga_Session session, uint32_t control, const uint16_t* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(uint16_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayI32(NiFpga_Session session, uint32_t control, const int32_t* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(int32_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayU32(NiFpga_Session session, uint32_t control, const uint32_t* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(uint32_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayI64(NiFpga_Session session, uint32_t control, const int64_t* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(int64_t) * size);
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_WriteArrayU64(NiFpga_Session session, uint32_t control, const uint64_t* array, size_t size) {
	memcpy(&(GetFakeFPGA()->fpgaRAM[control]), array, sizeof(uint64_t) * size);
	return NiFpga_Status_Success;
}
#pragma endregion

NiFpga_Status NiFpga_ReserveIrqContext(NiFpga_Session session, NiFpga_IrqContext* context) {
	return NiFpga_Status_Success;	// This isn't needed with my sketchy implementation!
}

NiFpga_Status NiFpga_UnreserveIrqContext(NiFpga_Session session, NiFpga_IrqContext context) {
	return NiFpga_Status_Success;	// This isn't needed with my sketchy implementation!
}

NiFpga_Status NiFpga_WaitOnIrqs(NiFpga_Session session, NiFpga_IrqContext context, uint32_t irqs, uint32_t timeout, uint32_t* irqsAsserted, NiFpga_Bool* timedOut) {
	/*exit(1);
	return NiFpga_Status_ResourceNotFound;*/
	GetFakeFPGA()->getIRQManager()->waitFor(irqs, timeout, irqsAsserted, timedOut);
	if (timedOut) {
		return NiFpga_Status_IrqTimeout;
	}
	return NiFpga_Status_Success;
}

NiFpga_Status NiFpga_AcknowledgeIrqs(NiFpga_Session session, uint32_t irqs) {
	return NiFpga_Status_Success;	// This isn't needed with my sketchy implementation!
}

#pragma region FIFO
NiFpga_Status NiFpga_ConfigureFifo(NiFpga_Session session, uint32_t fifo,
								   size_t depth) {
									   exit(1);
									   return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ConfigureFifo2(NiFpga_Session session, uint32_t fifo,
									size_t requestedDepth, size_t* actualDepth) {
										exit(1);
										return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_StartFifo(NiFpga_Session session, uint32_t fifo) {
	exit(1);
	return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_StopFifo(NiFpga_Session session, uint32_t fifo) {
	exit(1);
	return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoBool(NiFpga_Session session, uint32_t fifo,
								  NiFpga_Bool* data, size_t numberOfElements, uint32_t timeout,
								  size_t* elementsRemaining) {
									  exit(1);
									  return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoI8(NiFpga_Session session, uint32_t fifo,
								int8_t* data, size_t numberOfElements, uint32_t timeout,
								size_t* elementsRemaining) {
									exit(1);
									return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoU8(NiFpga_Session session, uint32_t fifo,
								uint8_t* data, size_t numberOfElements, uint32_t timeout,
								size_t* elementsRemaining) {
									exit(1);
									return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoI16(NiFpga_Session session, uint32_t fifo,
								 int16_t* data, size_t numberOfElements, uint32_t timeout,
								 size_t* elementsRemaining) {
									 exit(1);
									 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoU16(NiFpga_Session session, uint32_t fifo,
								 uint16_t* data, size_t numberOfElements, uint32_t timeout,
								 size_t* elementsRemaining) {
									 exit(1);
									 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoI32(NiFpga_Session session, uint32_t fifo,
								 int32_t* data, size_t numberOfElements, uint32_t timeout,
								 size_t* elementsRemaining) {
									 exit(1);
									 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoU32(NiFpga_Session session, uint32_t fifo,
								 uint32_t* data, size_t numberOfElements, uint32_t timeout,
								 size_t* elementsRemaining) {
									 exit(1);
									 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoI64(NiFpga_Session session, uint32_t fifo,
								 int64_t* data, size_t numberOfElements, uint32_t timeout,
								 size_t* elementsRemaining) {
									 exit(1);
									 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReadFifoU64(NiFpga_Session session, uint32_t fifo,
								 uint64_t* data, size_t numberOfElements, uint32_t timeout,
								 size_t* elementsRemaining) {
									 exit(1);
									 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoBool(NiFpga_Session session, uint32_t fifo,
								   const NiFpga_Bool* data, size_t numberOfElements, uint32_t timeout,
								   size_t* emptyElementsRemaining) {
									   exit(1);
									   return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoI8(NiFpga_Session session, uint32_t fifo,
								 const int8_t* data, size_t numberOfElements, uint32_t timeout,
								 size_t* emptyElementsRemaining) {
									 exit(1);
									 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoU8(NiFpga_Session session, uint32_t fifo,
								 const uint8_t* data, size_t numberOfElements, uint32_t timeout,
								 size_t* emptyElementsRemaining) {
									 exit(1);
									 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoI16(NiFpga_Session session, uint32_t fifo,
								  const int16_t* data, size_t numberOfElements, uint32_t timeout,
								  size_t* emptyElementsRemaining) {
									  exit(1);
									  return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoU16(NiFpga_Session session, uint32_t fifo,
								  const uint16_t* data, size_t numberOfElements, uint32_t timeout,
								  size_t* emptyElementsRemaining) {
									  exit(1);
									  return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoI32(NiFpga_Session session, uint32_t fifo,
								  const int32_t* data, size_t numberOfElements, uint32_t timeout,
								  size_t* emptyElementsRemaining) {
									  exit(1);
									  return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoU32(NiFpga_Session session, uint32_t fifo,
								  const uint32_t* data, size_t numberOfElements, uint32_t timeout,
								  size_t* emptyElementsRemaining) {
									  exit(1);
									  return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoI64(NiFpga_Session session, uint32_t fifo,
								  const int64_t* data, size_t numberOfElements, uint32_t timeout,
								  size_t* emptyElementsRemaining) {
									  exit(1);
									  return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_WriteFifoU64(NiFpga_Session session, uint32_t fifo,
								  const uint64_t* data, size_t numberOfElements, uint32_t timeout,
								  size_t* emptyElementsRemaining) {
									  exit(1);
									  return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsBool(NiFpga_Session session,
												 uint32_t fifo, NiFpga_Bool** elements, size_t elementsRequested,
												 uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													 exit(1);
													 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsI8(NiFpga_Session session,
											   uint32_t fifo, int8_t** elements, size_t elementsRequested,
											   uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
												   exit(1);
												   return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsU8(NiFpga_Session session,
											   uint32_t fifo, uint8_t** elements, size_t elementsRequested,
											   uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
												   exit(1);
												   return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsI16(NiFpga_Session session,
												uint32_t fifo, int16_t** elements, size_t elementsRequested,
												uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													exit(1);
													return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsU16(NiFpga_Session session,
												uint32_t fifo, uint16_t** elements, size_t elementsRequested,
												uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													exit(1);
													return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsI32(NiFpga_Session session,
												uint32_t fifo, int32_t** elements, size_t elementsRequested,
												uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													exit(1);
													return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsU32(NiFpga_Session session,
												uint32_t fifo, uint32_t** elements, size_t elementsRequested,
												uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													exit(1);
													return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsI64(NiFpga_Session session,
												uint32_t fifo, int64_t** elements, size_t elementsRequested,
												uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													exit(1);
													return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoReadElementsU64(NiFpga_Session session,
												uint32_t fifo, uint64_t** elements, size_t elementsRequested,
												uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													exit(1);
													return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsBool(NiFpga_Session session,
												  uint32_t fifo, NiFpga_Bool** elements, size_t elementsRequested,
												  uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													  exit(1);
													  return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsI8(NiFpga_Session session,
												uint32_t fifo, int8_t** elements, size_t elementsRequested,
												uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													exit(1);
													return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsU8(NiFpga_Session session,
												uint32_t fifo, uint8_t** elements, size_t elementsRequested,
												uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													exit(1);
													return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsI16(NiFpga_Session session,
												 uint32_t fifo, int16_t** elements, size_t elementsRequested,
												 uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													 exit(1);
													 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsU16(NiFpga_Session session,
												 uint32_t fifo, uint16_t** elements, size_t elementsRequested,
												 uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													 exit(1);
													 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsI32(NiFpga_Session session,
												 uint32_t fifo, int32_t** elements, size_t elementsRequested,
												 uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													 exit(1);
													 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsU32(NiFpga_Session session,
												 uint32_t fifo, uint32_t** elements, size_t elementsRequested,
												 uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													 exit(1);
													 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsI64(NiFpga_Session session,
												 uint32_t fifo, int64_t** elements, size_t elementsRequested,
												 uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													 exit(1);
													 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_AcquireFifoWriteElementsU64(NiFpga_Session session,
												 uint32_t fifo, uint64_t** elements, size_t elementsRequested,
												 uint32_t timeout, size_t* elementsAcquired, size_t* elementsRemaining) {
													 exit(1);
													 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_ReleaseFifoElements(NiFpga_Session session, uint32_t fifo,
										 size_t elements) {
											 exit(1);
											 return NiFpga_Status_ResourceNotFound;
}

NiFpga_Status NiFpga_GetPeerToPeerFifoEndpoint(NiFpga_Session session,
											   uint32_t fifo, uint32_t* endpoint) {
												   exit(1);
												   return NiFpga_Status_ResourceNotFound;
}

#pragma endregion
