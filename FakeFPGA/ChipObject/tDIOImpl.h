/*
* tDIOImpl.h
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#ifndef TDIOIMPL_H_
#define TDIOIMPL_H_

#include <stdint.h>
#include <tDIO.h>

namespace nFPGA {
	class NiFpgaState;

	class tDIO_Impl: public nFPGA::nFRC_2012_1_6_4::tDIO {
	private:
		NiFpgaState *state;
		unsigned char index;
	private:
		unsigned short digitalOutputPort;
		unsigned short digitalOutputState;
		unsigned short digitalOutputPulse;
		unsigned char digitalOutputPulseLength;

		unsigned char relayForward;
		unsigned char relayReverse;
		unsigned char i2cHeader;

		uint8_t doPWMPeriod[kNumDO_PWMDutyCycleElements];
		uint8_t doPWMDutyCycle[kNumDO_PWMDutyCycleElements];
		tDO_PWMConfig doPwmConfig;

		tPWMConfig pwmConfig;
		uint8_t pwmPeriodScale[kNumPWMPeriodScaleElements];

		unsigned short loopTiming;	// This better be 260
	public:
		uint8_t pwmValue[kNumPWMValueRegisters];
		uint8_t pwmTypes[kNumPWMValueRegisters];

	public:
		tDIO_Impl(NiFpgaState *state, unsigned char index);
		virtual ~tDIO_Impl();

		virtual tSystemInterface* getSystemInterface();
		virtual unsigned char getSystemIndex();

		virtual void writeFilterSelect(unsigned char bitfield_index,
			unsigned char value, tRioStatusCode *status);
		virtual unsigned char readFilterSelect(unsigned char bitfield_index,
			tRioStatusCode *status);

		virtual void writeI2CDataToSend(unsigned int value, tRioStatusCode *status);
		virtual unsigned int readI2CDataToSend(tRioStatusCode *status);

		virtual void writeDO(unsigned short value, tRioStatusCode *status);
		virtual unsigned short readDO(tRioStatusCode *status);

		virtual void writeFilterPeriod(unsigned char bitfield_index,
			unsigned char value, tRioStatusCode *status);
		virtual unsigned char readFilterPeriod(unsigned char bitfield_index,
			tRioStatusCode *status);

		virtual void writeOutputEnable(unsigned short value,
			tRioStatusCode *status);
		virtual unsigned short readOutputEnable(tRioStatusCode *status);

		virtual void writePulse(unsigned short value, tRioStatusCode *status);
		virtual unsigned short readPulse(tRioStatusCode *status);

		virtual void writeSlowValue(tSlowValue value, tRioStatusCode *status);
		virtual void writeSlowValue_RelayFwd(unsigned char value,
			tRioStatusCode *status);
		virtual void writeSlowValue_RelayRev(unsigned char value,
			tRioStatusCode *status);
		virtual void writeSlowValue_I2CHeader(unsigned char value,
			tRioStatusCode *status);
		virtual tSlowValue readSlowValue(tRioStatusCode *status);
		virtual unsigned char readSlowValue_RelayFwd(tRioStatusCode *status);
		virtual unsigned char readSlowValue_RelayRev(tRioStatusCode *status);
		virtual unsigned char readSlowValue_I2CHeader(tRioStatusCode *status);

		virtual tI2CStatus readI2CStatus(tRioStatusCode *status);
		virtual unsigned char readI2CStatus_Transaction(tRioStatusCode *status);
		virtual bool readI2CStatus_Done(tRioStatusCode *status);
		virtual bool readI2CStatus_Aborted(tRioStatusCode *status);
		virtual unsigned int readI2CStatus_DataReceivedHigh(tRioStatusCode *status);

		virtual unsigned int readI2CDataReceived(tRioStatusCode *status);

		virtual unsigned short readDI(tRioStatusCode *status);

		virtual void writePulseLength(unsigned char value, tRioStatusCode *status);
		virtual unsigned char readPulseLength(tRioStatusCode *status);

		virtual void writePWMPeriodScale(unsigned char bitfield_index,
			unsigned char value, tRioStatusCode *status);
		virtual unsigned char readPWMPeriodScale(unsigned char bitfield_index,
			tRioStatusCode *status);

		virtual void writeDO_PWMDutyCycle(unsigned char bitfield_index,
			unsigned char value, tRioStatusCode *status);
		virtual unsigned char readDO_PWMDutyCycle(unsigned char bitfield_index,
			tRioStatusCode *status);

		virtual void writeBFL(bool value, tRioStatusCode *status);
		virtual bool readBFL(tRioStatusCode *status);

		virtual void writeI2CConfig(tI2CConfig value, tRioStatusCode *status);
		virtual void writeI2CConfig_Address(unsigned char value,
			tRioStatusCode *status);
		virtual void writeI2CConfig_BytesToRead(unsigned char value,
			tRioStatusCode *status);
		virtual void writeI2CConfig_BytesToWrite(unsigned char value,
			tRioStatusCode *status);
		virtual void writeI2CConfig_DataToSendHigh(unsigned short value,
			tRioStatusCode *status);
		virtual void writeI2CConfig_BitwiseHandshake(bool value,
			tRioStatusCode *status);
		virtual tI2CConfig readI2CConfig(tRioStatusCode *status);
		virtual unsigned char readI2CConfig_Address(tRioStatusCode *status);
		virtual unsigned char readI2CConfig_BytesToRead(tRioStatusCode *status);
		virtual unsigned char readI2CConfig_BytesToWrite(tRioStatusCode *status);
		virtual unsigned short readI2CConfig_DataToSendHigh(tRioStatusCode *status);
		virtual bool readI2CConfig_BitwiseHandshake(tRioStatusCode *status);

		virtual void writeDO_PWMConfig(tDO_PWMConfig value, tRioStatusCode *status);
		virtual void writeDO_PWMConfig_PeriodPower(unsigned char value,
			tRioStatusCode *status);
		virtual void writeDO_PWMConfig_OutputSelect_0(unsigned char value,
			tRioStatusCode *status);
		virtual void writeDO_PWMConfig_OutputSelect_1(unsigned char value,
			tRioStatusCode *status);
		virtual void writeDO_PWMConfig_OutputSelect_2(unsigned char value,
			tRioStatusCode *status);
		virtual void writeDO_PWMConfig_OutputSelect_3(unsigned char value,
			tRioStatusCode *status);
		virtual tDO_PWMConfig readDO_PWMConfig(tRioStatusCode *status);
		virtual unsigned char readDO_PWMConfig_PeriodPower(tRioStatusCode *status);
		virtual unsigned char readDO_PWMConfig_OutputSelect_0(
			tRioStatusCode *status);
		virtual unsigned char readDO_PWMConfig_OutputSelect_1(
			tRioStatusCode *status);
		virtual unsigned char readDO_PWMConfig_OutputSelect_2(
			tRioStatusCode *status);
		virtual unsigned char readDO_PWMConfig_OutputSelect_3(
			tRioStatusCode *status);

		virtual void strobeI2CStart(tRioStatusCode *status);

		virtual unsigned short readLoopTiming(tRioStatusCode *status);

		virtual void writePWMConfig(tPWMConfig value, tRioStatusCode *status);
		virtual void writePWMConfig_Period(unsigned short value,
			tRioStatusCode *status);
		virtual void writePWMConfig_MinHigh(unsigned short value,
			tRioStatusCode *status);
		virtual tPWMConfig readPWMConfig(tRioStatusCode *status);
		virtual unsigned short readPWMConfig_Period(tRioStatusCode *status);
		virtual unsigned short readPWMConfig_MinHigh(tRioStatusCode *status);

		virtual void writePWMValue(unsigned char reg_index, unsigned char value,
			tRioStatusCode *status);
		virtual unsigned char readPWMValue(unsigned char reg_index,
			tRioStatusCode *status);

		void writeDigitalPort(unsigned short nDigitalPort, unsigned short nDigitalMask);
	};

}

#endif /* TDIOIMPL_H_ */
