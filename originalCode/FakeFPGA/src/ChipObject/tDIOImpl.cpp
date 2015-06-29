/*
* tDIOImpl.cpp
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#include <stdio.h>
#include <ChipObject/tSystemInterface.h>

#include "ChipObject/tDIOImpl.h"
#include "ChipObject/NiFpgaState.h"
#include "ChipObject/tInterruptImpl.h"
#include "ChipObject/NiIRQImpl.h"
#include "ChipObject/tGlobalImpl.h"

#define TDIO_DECL_ADDRESSES(x) const int tDIO_Impl::k ## x ## _Addresses [] = {kDIO0_ ## x ## _Address, kDIO1_ ## x ## _Address }

namespace nFPGA {
	TDIO_DECL_ADDRESSES(FilterSelect);
	TDIO_DECL_ADDRESSES(DO);
	TDIO_DECL_ADDRESSES(FilterPeriod);
	TDIO_DECL_ADDRESSES(OutputEnable);
	TDIO_DECL_ADDRESSES(Pulse);
	TDIO_DECL_ADDRESSES(SlowValue);
	TDIO_DECL_ADDRESSES(DI);
	TDIO_DECL_ADDRESSES(PulseLength);
	TDIO_DECL_ADDRESSES(PWMPeriodScale);
	TDIO_DECL_ADDRESSES(DO_PWMDutyCycle);
	TDIO_DECL_ADDRESSES(DO_PWMConfig);
	TDIO_DECL_ADDRESSES(PWMValue);

	tDIO_Impl::tDIO_Impl(NiFpgaState *state, unsigned char index) {
		this->state = state;
		this->index = index;
		// Map to ram
		this->loopTiming = (uint32_t*) &state->fpgaRAM[kDIO_LoopTiming_Address];
		this->digitalOutputPort = (uint32_t*) &state->fpgaRAM[kDO_Addresses[index]];
		this->digitalOutputState = (uint32_t*) &state->fpgaRAM[kOutputEnable_Addresses[index]];
		this->digitalOutputPulse = (uint32_t*) &state->fpgaRAM[kPulse_Addresses[index]];
		this->digitalOutputPulseLength = (uint32_t*) &state->fpgaRAM[kPulseLength_Addresses[index]];
		this->slowValue = (tSlowValue*) &state->fpgaRAM[kSlowValue_Addresses[index]];
		this->pwmConfig = (tPWMConfig*) &state->fpgaRAM[kDIO_PWMConfig_Address];
		this->doPWMDutyCycle = (uint32_t*) &state->fpgaRAM[kDO_PWMDutyCycle_Addresses[index]];
		this->pwmPeriodScale = (uint32_t*) &state->fpgaRAM[kPWMPeriodScale_Addresses[index]];
		this->pwmValue = (uint32_t*) &state->fpgaRAM[kPWMValue_Addresses[index]];

		*digitalOutputPort = 0;
		*digitalOutputState = 0;
		*digitalOutputPulse = 0;
		*digitalOutputPulseLength = 0;
		(*slowValue).value = 0;
		(*pwmConfig).value = 0;

		*doPWMDutyCycle = 0;
		*pwmPeriodScale = 0;

		for (int p = 0; p < kNumPWMValueRegisters; p++) {
			pwmTypes[p] = 0;
			pwmValue[p] = 0;
		}
	}

	tDIO_Impl::~tDIO_Impl() {
		if (this->state->dio[this->index] == this) {
			this->state->dio[this->index] = NULL;
		}
	}

	nFPGA::tSystemInterface* tDIO_Impl::getSystemInterface() {
		return this->state;
	}

	unsigned char tDIO_Impl::getSystemIndex() {
		return this->index;
	}

	void tDIO_Impl::writeFilterSelect(unsigned char bitfield_index,
		unsigned char value, tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;	//  no idea what this does?
			printf("Please no filter select\n");
	}

	unsigned char tDIO_Impl::readFilterSelect(unsigned char bitfield_index,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no filter select\n");
			return 0;	//  no idea what this does?
	}

	void tDIO_Impl::writeI2CDataToSend(unsigned int value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return;	//  I'll get to this
	}

	unsigned int tDIO_Impl::readI2CDataToSend(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return 0;	//  I'll get to this...
	}

	void tDIO_Impl::writeDO(unsigned short value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		writeDigitalPort(value, *digitalOutputState);		// Only output the ones configured for output
	}

	unsigned short tDIO_Impl::readDO(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return *digitalOutputPort & *digitalOutputState;
	}

	void tDIO_Impl::writeFilterPeriod(unsigned char bitfield_index,
		unsigned char value, tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no filter period\n");
			return;	//  I'll get to this...
	}

	unsigned char tDIO_Impl::readFilterPeriod(unsigned char bitfield_index,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no filter period\n");
			return 0;	//  I'll get to this...
	}

	void tDIO_Impl::writeOutputEnable(unsigned short value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			*digitalOutputState = value;
	}

	unsigned short tDIO_Impl::readOutputEnable(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return *digitalOutputState;
	}

	void tDIO_Impl::writePulse(unsigned short value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		*digitalOutputPulse = value;
	}

	unsigned short tDIO_Impl::readPulse(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return *digitalOutputPulse;
	}

	void tDIO_Impl::writeSlowValue(tSlowValue value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		(*slowValue) = value;
	}

	void tDIO_Impl::writeSlowValue_RelayFwd(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*slowValue).RelayRev = value;
	}

	void tDIO_Impl::writeSlowValue_RelayRev(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*slowValue).RelayRev = value;
	}

	void tDIO_Impl::writeSlowValue_I2CHeader(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*slowValue).I2CHeader = value;
	}

	nFPGA::nFRC_2012_1_6_4::tDIO::tSlowValue tDIO_Impl::readSlowValue(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			return *slowValue;
	}

	unsigned char tDIO_Impl::readSlowValue_RelayFwd(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return (*slowValue).RelayFwd;
	}

	unsigned char tDIO_Impl::readSlowValue_RelayRev(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return (*slowValue).RelayRev;
	}

	unsigned char tDIO_Impl::readSlowValue_I2CHeader(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return (*slowValue).I2CHeader;
	}

	nFPGA::nFRC_2012_1_6_4::tDIO::tI2CStatus tDIO_Impl::readI2CStatus(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no I2C\t(%s)\n", __FUNCTION__);
			nFPGA::nFRC_2012_1_6_4::tDIO::tI2CStatus res = nFPGA::nFRC_2012_1_6_4::tDIO::tI2CStatus();
			return res;
	}

	unsigned char tDIO_Impl::readI2CStatus_Transaction(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return 0;
	}

	bool tDIO_Impl::readI2CStatus_Done(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return true;
	}

	bool tDIO_Impl::readI2CStatus_Aborted(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return true;
	}

	unsigned int tDIO_Impl::readI2CStatus_DataReceivedHigh(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return 0;
	}

	unsigned int tDIO_Impl::readI2CDataReceived(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return 0;
	}

	unsigned short tDIO_Impl::readDI(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return *digitalOutputPort & ~*digitalOutputState;
	}

	void tDIO_Impl::writePulseLength(unsigned char value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		*digitalOutputPulseLength = value;
	}

	unsigned char tDIO_Impl::readPulseLength(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return *digitalOutputPulseLength;
	}

	void tDIO_Impl::writePWMPeriodScale(unsigned char bitfield_index,
		unsigned char value, tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			uint32_t regValue = *pwmPeriodScale;
			regValue &= ~(kPWMPeriodScale_ElementMask << ((kPWMPeriodScale_NumElements - 1 - bitfield_index) * kPWMPeriodScale_ElementSize));
			regValue |= ((value & kPWMPeriodScale_ElementMask) << ((kPWMPeriodScale_NumElements - 1 - bitfield_index) * kPWMPeriodScale_ElementSize));
			*pwmPeriodScale  = regValue;
	}

	unsigned char tDIO_Impl::readPWMPeriodScale(unsigned char bitfield_index,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			uint32_t arrayElementValue = ((*pwmPeriodScale)
				>> ((kPWMPeriodScale_NumElements - 1 - bitfield_index) * kPWMPeriodScale_ElementSize)) & kPWMPeriodScale_ElementMask;
			return (unsigned char)((arrayElementValue) & 0x00000003);
	}

	void tDIO_Impl::writeDO_PWMDutyCycle(unsigned char bitfield_index,
		unsigned char value, tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			uint32_t regValue = *doPWMDutyCycle;
			regValue  &= ~(kDO_PWMDutyCycle_ElementMask << ((kDO_PWMDutyCycle_NumElements - 1 - bitfield_index) * kDO_PWMDutyCycle_ElementSize));
			regValue  |= ((value & kDO_PWMDutyCycle_ElementMask) << ((kDO_PWMDutyCycle_NumElements - 1 - bitfield_index) * kDO_PWMDutyCycle_ElementSize));
			*doPWMDutyCycle = regValue;
	}

	unsigned char tDIO_Impl::readDO_PWMDutyCycle(unsigned char bitfield_index,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			int arrayElementValue = ((*doPWMDutyCycle)
				>> ((kDO_PWMDutyCycle_NumElements - 1 - bitfield_index) * kDO_PWMDutyCycle_ElementSize)) & kDO_PWMDutyCycle_ElementMask;
			return (short)((arrayElementValue) & 0x000000FF);
	}

	void tDIO_Impl::writeBFL(bool value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no BFL\n");
		return;	//  no idea what this does?
	}

	bool tDIO_Impl::readBFL(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no BFL\n");
		return false;	//  no idea what this does?
	}

	void tDIO_Impl::writeI2CConfig(tI2CConfig value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return;
	}

	void tDIO_Impl::writeI2CConfig_Address(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no I2C\t(%s)\n", __FUNCTION__);
			return;
	}

	void tDIO_Impl::writeI2CConfig_BytesToRead(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no I2C\t(%s)\n", __FUNCTION__);
			return;
	}

	void tDIO_Impl::writeI2CConfig_BytesToWrite(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no I2C\t(%s)\n", __FUNCTION__);
			return;
	}

	void tDIO_Impl::writeI2CConfig_DataToSendHigh(unsigned short value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no I2C\t(%s)\n", __FUNCTION__);
			return;
	}

	void tDIO_Impl::writeI2CConfig_BitwiseHandshake(bool value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no I2C\t(%s)\n", __FUNCTION__);
			return;
	}

	nFPGA::nFRC_2012_1_6_4::tDIO::tI2CConfig tDIO_Impl::readI2CConfig(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			printf("Please no I2C\t(%s)\n", __FUNCTION__);
			nFPGA::nFRC_2012_1_6_4::tDIO::tI2CConfig cfg = nFPGA::nFRC_2012_1_6_4::tDIO::tI2CConfig();
			return cfg;
	}

	unsigned char tDIO_Impl::readI2CConfig_Address(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return 0;
	}

	unsigned char tDIO_Impl::readI2CConfig_BytesToRead(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return 0;
	}

	unsigned char tDIO_Impl::readI2CConfig_BytesToWrite(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return 0;
	}

	unsigned short tDIO_Impl::readI2CConfig_DataToSendHigh(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return 0;
	}

	bool tDIO_Impl::readI2CConfig_BitwiseHandshake(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return false;
	}

#pragma region writeDO_PWMConfig
	void tDIO_Impl::writeDO_PWMConfig(tDO_PWMConfig value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		*doPwmConfig = value;
	}

	void tDIO_Impl::writeDO_PWMConfig_PeriodPower(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*doPwmConfig).PeriodPower = value;
	}

	void tDIO_Impl::writeDO_PWMConfig_OutputSelect_0(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*doPwmConfig).OutputSelect_0 = value;
	}

	void tDIO_Impl::writeDO_PWMConfig_OutputSelect_1(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*doPwmConfig).OutputSelect_1 = value;
	}

	void tDIO_Impl::writeDO_PWMConfig_OutputSelect_2(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*doPwmConfig).OutputSelect_2 = value;
	}

	void tDIO_Impl::writeDO_PWMConfig_OutputSelect_3(unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*doPwmConfig).OutputSelect_3 = value;
	}
#pragma endregion

#pragma region readDO_PWMConfig
	nFPGA::nFRC_2012_1_6_4::tDIO::tDO_PWMConfig tDIO_Impl::readDO_PWMConfig(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			return *doPwmConfig;
	}

	unsigned char tDIO_Impl::readDO_PWMConfig_PeriodPower(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return (*doPwmConfig).PeriodPower;
	}

	unsigned char tDIO_Impl::readDO_PWMConfig_OutputSelect_0(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			return (*doPwmConfig).OutputSelect_0;
	}

	unsigned char tDIO_Impl::readDO_PWMConfig_OutputSelect_1(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			return (*doPwmConfig).OutputSelect_1;
	}

	unsigned char tDIO_Impl::readDO_PWMConfig_OutputSelect_2(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			return (*doPwmConfig).OutputSelect_2;
	}

	unsigned char tDIO_Impl::readDO_PWMConfig_OutputSelect_3(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			return (*doPwmConfig).OutputSelect_3;
	}
#pragma endregion

	void tDIO_Impl::strobeI2CStart(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		printf("Please no I2C\t(%s)\n", __FUNCTION__);
		return;
	}

	unsigned short tDIO_Impl::readLoopTiming(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return (*loopTiming = 260);	// Shouldn't change but yeah
	}

	void tDIO_Impl::writePWMConfig(nFPGA::nFRC_2012_1_6_4::tDIO::tPWMConfig value, tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		*pwmConfig = value;
	}

	void tDIO_Impl::writePWMConfig_Period(unsigned short value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*pwmConfig).Period = value;
	}

	void tDIO_Impl::writePWMConfig_MinHigh(unsigned short value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			(*pwmConfig).MinHigh = value;
	}

	nFPGA::nFRC_2012_1_6_4::tDIO::tPWMConfig tDIO_Impl::readPWMConfig(
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			return *pwmConfig;
	}

	unsigned short tDIO_Impl::readPWMConfig_Period(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return (*pwmConfig).Period;
	}

	unsigned short tDIO_Impl::readPWMConfig_MinHigh(tRioStatusCode* status) {
		*status =  NiFpga_Status_Success;
		return (*pwmConfig).MinHigh;
	}

	void tDIO_Impl::writePWMValue(unsigned char reg_index, unsigned char value,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			pwmValue[reg_index] = value;
	}

	unsigned char tDIO_Impl::readPWMValue(unsigned char reg_index,
		tRioStatusCode* status) {
			*status =  NiFpga_Status_Success;
			return pwmValue[reg_index];
	}

	void tDIO_Impl::writeDigitalPort(unsigned short nDPort, unsigned short nDMask) {
		tRioStatusCode status;
		uint32_t timestamp = state->getGlobal()->readLocalTime(&status);

		for (int i = 0; i < sizeof(nDPort) * 8; i++) {
			unsigned short tmpMask = 1 << i;
			if ((nDMask & tmpMask) && (nDPort & tmpMask) != (*digitalOutputPort & tmpMask)) {
				bool falling = !(nDPort & tmpMask) && (*digitalOutputPort & tmpMask);
				bool rising = (nDPort & tmpMask) && !(*digitalOutputPort & tmpMask);
				// Changed!  Check for triggering
				for (int t = 0; t < tInterrupt_Impl::kNumSystems; t++) {
					tInterrupt_Impl *interrupt = state->interrupt[t];
					if (interrupt != NULL && !(*(interrupt->config)).Source_AnalogTrigger && (*(interrupt->config)).Source_Module == this->index && (*(interrupt->config)).Source_Channel == i && (((*(interrupt->config)).FallingEdge && falling) || ((*(interrupt->config)).RisingEdge && rising))) {
						// Signal it!
						state->getIRQManager()->signal(1 << interrupt->sys_index);
						*(interrupt->timestamp) = timestamp;
					}
				}
			}
		}
		*digitalOutputPort = (*digitalOutputPort & ~nDMask) | (nDPort & nDMask);
	}
}
