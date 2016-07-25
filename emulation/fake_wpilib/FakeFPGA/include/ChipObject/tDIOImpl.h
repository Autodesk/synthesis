/*
* tDIOImpl.h
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#ifndef TDIOIMPL_H_
#define TDIOIMPL_H_

#include <stdint.h>
#include <ChipObject/tDIO.h>

namespace nFPGA {
	class NiFpgaState;

	class tDIO_Impl: public nFPGA::nFRC_2012_1_6_4::tDIO {
		// Addresses
	private:
#pragma region ADDRESSES
		static const int kDIO_LoopTiming_Address = 0x8200;
		static const int kDIO0_FilterSelect_Address = 0x8268;
		static const int kDIO1_FilterSelect_Address = 0x82D4;
		static const int kFilterSelect_Addresses[];
		static const int kDIO0_DO_Address = 0x8208;
		static const int kDIO1_DO_Address = 0x8274;
		static const int kDO_Addresses [];
		static const int kFilterPeriod_NumElements = 3;
		static const int kFilterPeriod_ElementSize = 8;
		static const int kFilterPeriod_ElementMask = 0xFF;
		static const int kDIO0_FilterPeriod_Address = 0x8264;
		static const int kDIO1_FilterPeriod_Address = 0x82D0;
		static const int kFilterPeriod_Addresses [];
		static const int kDIO0_OutputEnable_Address = 0x8210;
		static const int kDIO1_OutputEnable_Address = 0x827C;
		static const int kOutputEnable_Addresses [];
		static const int kDIO0_Pulse_Address = 0x825C;
		static const int kDIO1_Pulse_Address = 0x82C8;
		static const int kPulse_Addresses [];
		static const int kDIO0_SlowValue_Address = 0x8254;
		static const int kDIO1_SlowValue_Address = 0x82C0;
		static const int kSlowValue_Addresses [];
		static const int kDIO0_DI_Address = 0x820C;
		static const int kDIO1_DI_Address = 0x8278;
		static const int kDI_Addresses [];
		static const int kDIO0_PulseLength_Address = 0x8260;
		static const int kDIO1_PulseLength_Address = 0x82CC;
		static const int kPulseLength_Addresses [];  
		static const int kPWMPeriodScale_NumElements = 10;
		static const int kPWMPeriodScale_ElementSize = 2;
		static const int kPWMPeriodScale_ElementMask = 0x3;
		static const int kDIO0_PWMPeriodScale_Address = 0x823C;
		static const int kDIO1_PWMPeriodScale_Address = 0x82A8;
		static const int kPWMPeriodScale_Addresses [];
		static const int kDO_PWMDutyCycle_NumElements = 4;
		static const int kDO_PWMDutyCycle_ElementSize = 8;
		static const int kDO_PWMDutyCycle_ElementMask = 0xFF;
		static const int kDIO0_DO_PWMDutyCycle_Address = 0x826C;
		static const int kDIO1_DO_PWMDutyCycle_Address = 0x82D8;
		static const int kDO_PWMDutyCycle_Addresses [];
		static const int kDIO0_DO_PWMConfig_Address = 0x8270;
		static const int kDIO1_DO_PWMConfig_Address = 0x82DC;
		static const int kDO_PWMConfig_Addresses [] ;
		static const int kDIO_PWMConfig_Address = 0x8204;
		static const int kDIO0_PWMValue_Address = 0x8214;
		static const int kDIO1_PWMValue_Address = 0x8280;
		static const int kPWMValue_Addresses[];
#pragma endregion
	private:
		NiFpgaState *state;
		unsigned char index;
	private:
		uint32_t *digitalOutputPort;
		uint32_t *digitalOutputState;
		uint32_t *digitalOutputPulse;
		uint32_t *digitalOutputPulseLength;

		tSlowValue *slowValue;

		uint32_t *doPWMDutyCycle;
		tDO_PWMConfig *doPwmConfig;

		tPWMConfig *pwmConfig;
		uint32_t *pwmPeriodScale;

		uint32_t *loopTiming;	// This better be 260
	public:
		uint32_t *pwmValue;
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

		/// Updates the state of the digital IO port and calls the correct interrupts.
		/// @param nDigitalPort The new state of the digital port
		/// @param nDigitalMask The port mask to update
		void writeDigitalPort(unsigned short nDigitalPort, unsigned short nDigitalMask);
	};

}

#endif /* TDIOIMPL_H_ */
