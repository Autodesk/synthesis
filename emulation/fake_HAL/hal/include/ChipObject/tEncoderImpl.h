#ifndef __TENCODER_IMPL
#define __TENCODER_IMPL

#include <ChipObject/tEncoder.h>

namespace nFPGA {
	class NiFpgaState;

	class tEncoder_Impl : public nFPGA::nFRC_2012_1_6_4::tEncoder {
	private:
#pragma region ADDRESSES
		static const int kEncoder0_Output_Address = 0x8388;
		static const int kEncoder1_Output_Address = 0x839C;
		static const int kEncoder2_Output_Address = 0x83B0;
		static const int kEncoder3_Output_Address = 0x83C4;
	public:
		static const int kOutput_Addresses [];
	private:
		static const int kEncoder0_Config_Address = 0x8380;
		static const int kEncoder1_Config_Address = 0x8394;
		static const int kEncoder2_Config_Address = 0x83A8;
		static const int kEncoder3_Config_Address = 0x83BC;
		static const int kConfig_Addresses [] ;
		static const int kEncoder0_TimerOutput_Address = 0x8390;
		static const int kEncoder1_TimerOutput_Address = 0x83A4;
		static const int kEncoder2_TimerOutput_Address = 0x83B8;
		static const int kEncoder3_TimerOutput_Address = 0x83CC;
		static const int kTimerOutput_Addresses [];
	public:
		static const int kEncoder0_Reset_Address = 0x8384;
		static const int kEncoder1_Reset_Address = 0x8398;
		static const int kEncoder2_Reset_Address = 0x83AC;
		static const int kEncoder3_Reset_Address = 0x83C0;
		static const int kReset_Addresses [];
	private:
		static const int kEncoder0_TimerConfig_Address = 0x838C;
		static const int kEncoder1_TimerConfig_Address = 0x83A0;
		static const int kEncoder2_TimerConfig_Address = 0x83B4;
		static const int kEncoder3_TimerConfig_Address = 0x83C8;
		static const int kTimerConfig_Addresses [];
#pragma endregion
	private:
		NiFpgaState *state;
		unsigned char sys_index;

		tOutput *encoderOutput;
		tConfig *encoderConfig;
		tTimerOutput *timerOutput;
		tTimerConfig *timerConfig;

		int32_t outputOffset;
		uint32_t lastUpdate;
	public:

		tEncoder_Impl(NiFpgaState *state, unsigned char sys_index);
		virtual ~tEncoder_Impl();

		virtual tSystemInterface* getSystemInterface();
		virtual unsigned char getSystemIndex();

		virtual tOutput readOutput(tRioStatusCode *status);
		virtual bool readOutput_Direction(tRioStatusCode *status);
		virtual signed int readOutput_Value(tRioStatusCode *status);

		virtual void writeConfig(tConfig value, tRioStatusCode *status);
		virtual void writeConfig_ASource_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_ASource_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_ASource_AnalogTrigger(bool value, tRioStatusCode *status);
		virtual void writeConfig_BSource_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_BSource_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_BSource_AnalogTrigger(bool value, tRioStatusCode *status);
		virtual void writeConfig_IndexSource_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_IndexSource_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode *status);
		virtual void writeConfig_IndexActiveHigh(bool value, tRioStatusCode *status);
		virtual void writeConfig_Reverse(bool value, tRioStatusCode *status);
		virtual void writeConfig_Enable(bool value, tRioStatusCode *status);
		virtual tConfig readConfig(tRioStatusCode *status);
		virtual unsigned char readConfig_ASource_Channel(tRioStatusCode *status);
		virtual unsigned char readConfig_ASource_Module(tRioStatusCode *status);
		virtual bool readConfig_ASource_AnalogTrigger(tRioStatusCode *status);
		virtual unsigned char readConfig_BSource_Channel(tRioStatusCode *status);
		virtual unsigned char readConfig_BSource_Module(tRioStatusCode *status);
		virtual bool readConfig_BSource_AnalogTrigger(tRioStatusCode *status);
		virtual unsigned char readConfig_IndexSource_Channel(tRioStatusCode *status);
		virtual unsigned char readConfig_IndexSource_Module(tRioStatusCode *status);
		virtual bool readConfig_IndexSource_AnalogTrigger(tRioStatusCode *status);
		virtual bool readConfig_IndexActiveHigh(tRioStatusCode *status);
		virtual bool readConfig_Reverse(tRioStatusCode *status);
		virtual bool readConfig_Enable(tRioStatusCode *status);

		virtual tTimerOutput readTimerOutput(tRioStatusCode *status);
		virtual unsigned int readTimerOutput_Period(tRioStatusCode *status);
		virtual signed char readTimerOutput_Count(tRioStatusCode *status);
		virtual bool readTimerOutput_Stalled(tRioStatusCode *status);

		virtual void strobeReset(tRioStatusCode *status);

		virtual void writeTimerConfig(tTimerConfig value, tRioStatusCode *status);
		virtual void writeTimerConfig_StallPeriod(unsigned int value, tRioStatusCode *status);
		virtual void writeTimerConfig_AverageSize(unsigned char value, tRioStatusCode *status);
		virtual void writeTimerConfig_UpdateWhenEmpty(bool value, tRioStatusCode *status);
		virtual tTimerConfig readTimerConfig(tRioStatusCode *status);
		virtual unsigned int readTimerConfig_StallPeriod(tRioStatusCode *status);
		virtual unsigned char readTimerConfig_AverageSize(tRioStatusCode *status);
		virtual bool readTimerConfig_UpdateWhenEmpty(tRioStatusCode *status);

		void doUpdate(int32_t value);
		void doReset();
	};
}

#endif