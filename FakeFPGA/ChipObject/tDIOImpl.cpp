/*
 * tDIOImpl.cpp
 *
 *  Created on: Jul 18, 2014
 *      Author: localadmin
 */

#include "tDIOImpl.h"
#include "ChipObject/NiFpgaState.h"
#include <tSystemInterface.h>
#include <stdio.h>

namespace nFPGA {

tDIO_Impl::tDIO_Impl(NiFpgaState *state, unsigned char index) {
	this->state = state;
	this->index = index;
	this->loopTiming = 260;		// Timing!

	this->digitalOutputPort = 0;
	this->digitalOutputState = 0;
	this->digitalOutputPulse = 0;
	this->digitalOutputPulseLength = 0;
	this->i2cHeader = 0;
	this->pwmConfig.value = 0;
	this->pwmMinHigh = 0;
	this->pwmPeriod = 0;
	this->pwmPeriodScale = 0;
	for (int p = 0; p < 8; p++) {
		this->pwmDutyCycle[p] = 0;
		this->pwmValue[p] = 0;
	}
	this->relayForward = 0;
	this->relayReverse = 0;
}

tDIO_Impl::~tDIO_Impl() {
}

nFPGA::tSystemInterface* tDIO_Impl::getSystemInterface() {
	return this->state;
}

unsigned char tDIO_Impl::getSystemIndex() {
	return this->index;
}

void tDIO_Impl::writeFilterSelect(unsigned char bitfield_index,
		unsigned char value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;	//  no idea what this does?
	printf("Please no filter select\n");
}

unsigned char tDIO_Impl::readFilterSelect(unsigned char bitfield_index,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no filter select\n");
	return 0;	//  no idea what this does?
}

void tDIO_Impl::writeI2CDataToSend(unsigned int value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return;	//  I'll get to this
}

unsigned int tDIO_Impl::readI2CDataToSend(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;	//  I'll get to this...
}

void tDIO_Impl::writeDO(unsigned short value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	digitalOutputPort = value;
}

unsigned short tDIO_Impl::readDO(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return digitalOutputPort;
}

void tDIO_Impl::writeFilterPeriod(unsigned char bitfield_index,
		unsigned char value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no filter period\n");
	return;	//  I'll get to this...
}

unsigned char tDIO_Impl::readFilterPeriod(unsigned char bitfield_index,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no filter period\n");
	return 0;	//  I'll get to this...
}

void tDIO_Impl::writeOutputEnable(unsigned short value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	digitalOutputState = value;
}

unsigned short tDIO_Impl::readOutputEnable(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return digitalOutputState;
}

void tDIO_Impl::writePulse(unsigned short value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	digitalOutputPulse = value;
}

unsigned short tDIO_Impl::readPulse(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return digitalOutputPulse;
}

void tDIO_Impl::writeSlowValue(tSlowValue value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
}

void tDIO_Impl::writeSlowValue_RelayFwd(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	relayForward = value;
}

void tDIO_Impl::writeSlowValue_RelayRev(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	relayReverse = value;
}

void tDIO_Impl::writeSlowValue_I2CHeader(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	i2cHeader = value;
}

nFPGA::nFRC_2012_1_6_4::tDIO::tSlowValue tDIO_Impl::readSlowValue(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	nFPGA::nFRC_2012_1_6_4::tDIO::tSlowValue res;
	res.I2CHeader = i2cHeader;
	res.RelayFwd = relayForward;
	res.RelayRev = relayReverse;
	return res;
}

unsigned char tDIO_Impl::readSlowValue_RelayFwd(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return relayForward;
}

unsigned char tDIO_Impl::readSlowValue_RelayRev(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return relayReverse;
}

unsigned char tDIO_Impl::readSlowValue_I2CHeader(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return i2cHeader;
}

nFPGA::nFRC_2012_1_6_4::tDIO::tI2CStatus tDIO_Impl::readI2CStatus(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	nFPGA::nFRC_2012_1_6_4::tDIO::tI2CStatus res;
	return res;
}

unsigned char tDIO_Impl::readI2CStatus_Transaction(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;
}

bool tDIO_Impl::readI2CStatus_Done(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return true;
}

bool tDIO_Impl::readI2CStatus_Aborted(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return true;
}

unsigned int tDIO_Impl::readI2CStatus_DataReceivedHigh(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;
}

unsigned int tDIO_Impl::readI2CDataReceived(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;
}

unsigned short tDIO_Impl::readDI(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;
}

void tDIO_Impl::writePulseLength(unsigned char value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	digitalOutputPulseLength = value;
}

unsigned char tDIO_Impl::readPulseLength(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return digitalOutputPulseLength;
}

void tDIO_Impl::writePWMPeriodScale(unsigned char bitfield_index,
		unsigned char value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	uint16_t mask = 3 << (bitfield_index * 2);
	pwmPeriodScale = (pwmPeriodScale & (~mask))
			| ((value << (bitfield_index * 2)) & mask);
}

unsigned char tDIO_Impl::readPWMPeriodScale(unsigned char bitfield_index,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return (pwmPeriodScale >> (bitfield_index * 2)) & 3;
}

void tDIO_Impl::writeDO_PWMDutyCycle(unsigned char bitfield_index,
		unsigned char value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	pwmDutyCycle[bitfield_index] = value;
}

unsigned char tDIO_Impl::readDO_PWMDutyCycle(unsigned char bitfield_index,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return pwmDutyCycle[bitfield_index];
}

void tDIO_Impl::writeBFL(bool value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no BFL\n");
	return;	//  no idea what this does?
}

bool tDIO_Impl::readBFL(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no BFL\n");
	return false;	//  no idea what this does?
}

void tDIO_Impl::writeI2CConfig(tI2CConfig value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return;
}

void tDIO_Impl::writeI2CConfig_Address(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return;
}

void tDIO_Impl::writeI2CConfig_BytesToRead(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return;
}

void tDIO_Impl::writeI2CConfig_BytesToWrite(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return;
}

void tDIO_Impl::writeI2CConfig_DataToSendHigh(unsigned short value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return;
}

void tDIO_Impl::writeI2CConfig_BitwiseHandshake(bool value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return;
}

nFPGA::nFRC_2012_1_6_4::tDIO::tI2CConfig tDIO_Impl::readI2CConfig(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	nFPGA::nFRC_2012_1_6_4::tDIO::tI2CConfig cfg;
	return cfg;
}

unsigned char tDIO_Impl::readI2CConfig_Address(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;
}

unsigned char tDIO_Impl::readI2CConfig_BytesToRead(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;
}

unsigned char tDIO_Impl::readI2CConfig_BytesToWrite(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;
}

unsigned short tDIO_Impl::readI2CConfig_DataToSendHigh(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return 0;
}

bool tDIO_Impl::readI2CConfig_BitwiseHandshake(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return false;
}

void tDIO_Impl::writeDO_PWMConfig(tDO_PWMConfig value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	doPwmConfig = value;
}

void tDIO_Impl::writeDO_PWMConfig_PeriodPower(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	doPwmConfig.PeriodPower = value;
}

void tDIO_Impl::writeDO_PWMConfig_OutputSelect_0(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	doPwmConfig.OutputSelect_0 = value;
}

void tDIO_Impl::writeDO_PWMConfig_OutputSelect_1(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	doPwmConfig.OutputSelect_1 = value;
}

void tDIO_Impl::writeDO_PWMConfig_OutputSelect_2(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	doPwmConfig.OutputSelect_2 = value;
}

void tDIO_Impl::writeDO_PWMConfig_OutputSelect_3(unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	doPwmConfig.OutputSelect_3 = value;
}

nFPGA::nFRC_2012_1_6_4::tDIO::tDO_PWMConfig tDIO_Impl::readDO_PWMConfig(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return doPwmConfig;
}

unsigned char tDIO_Impl::readDO_PWMConfig_PeriodPower(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return doPwmConfig.PeriodPower;
}

unsigned char tDIO_Impl::readDO_PWMConfig_OutputSelect_0(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return doPwmConfig.OutputSelect_0;
}

unsigned char tDIO_Impl::readDO_PWMConfig_OutputSelect_1(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return doPwmConfig.OutputSelect_1;
}

unsigned char tDIO_Impl::readDO_PWMConfig_OutputSelect_2(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return doPwmConfig.OutputSelect_2;
}

unsigned char tDIO_Impl::readDO_PWMConfig_OutputSelect_3(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return doPwmConfig.OutputSelect_3;
}

void tDIO_Impl::strobeI2CStart(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	printf("Please no I2C\n");
	return;
}

unsigned short tDIO_Impl::readLoopTiming(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return loopTiming;
}

void tDIO_Impl::writePWMConfig(nFPGA::nFRC_2012_1_6_4::tDIO::tPWMConfig value, tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	pwmConfig = value;
}

void tDIO_Impl::writePWMConfig_Period(unsigned short value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	pwmPeriod = value;
}

void tDIO_Impl::writePWMConfig_MinHigh(unsigned short value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	pwmMinHigh = value;
}

nFPGA::nFRC_2012_1_6_4::tDIO::tPWMConfig tDIO_Impl::readPWMConfig(
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return pwmConfig;
}

unsigned short tDIO_Impl::readPWMConfig_Period(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return pwmPeriod;
}

unsigned short tDIO_Impl::readPWMConfig_MinHigh(tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return pwmMinHigh;
}

void tDIO_Impl::writePWMValue(unsigned char reg_index, unsigned char value,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	pwmValue[reg_index] = value;
}

unsigned char tDIO_Impl::readPWMValue(unsigned char reg_index,
		tRioStatusCode* status) {
	status = NiFpga_Status_Success;
	return pwmValue[reg_index];
}
}
