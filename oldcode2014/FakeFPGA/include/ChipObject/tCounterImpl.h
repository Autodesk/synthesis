#ifndef __T_COUNTER_IMPL_H
#define __T_COUNTER_IMPL_H

#include <ChipObject/tCounter.h>

namespace nFPGA {
	class NiFpgaState;

	class tCounter_Impl : public nFPGA::nFRC_2012_1_6_4::tCounter {
	// TODO.  Software implement counter or source values from FPGA.
	private:
#pragma region ADDRESSES
		static const int kCounter0_Output_Address = 0x82E8;
		static const int kCounter1_Output_Address = 0x82FC;
		static const int kCounter2_Output_Address = 0x8310;
		static const int kCounter3_Output_Address = 0x8324;
		static const int kCounter4_Output_Address = 0x8338;
		static const int kCounter5_Output_Address = 0x834C;
		static const int kCounter6_Output_Address = 0x8360;
		static const int kCounter7_Output_Address = 0x8374;
		static const int kOutput_Addresses [];
		static const int kCounter0_Config_Address = 0x82E0;
		static const int kCounter1_Config_Address = 0x82F4;
		static const int kCounter2_Config_Address = 0x8308;
		static const int kCounter3_Config_Address = 0x831C;
		static const int kCounter4_Config_Address = 0x8330;
		static const int kCounter5_Config_Address = 0x8344;
		static const int kCounter6_Config_Address = 0x8358;
		static const int kCounter7_Config_Address = 0x836C;
		static const int kConfig_Addresses [];
		static const int kCounter0_TimerOutput_Address = 0x82F0;
		static const int kCounter1_TimerOutput_Address = 0x8304;
		static const int kCounter2_TimerOutput_Address = 0x8318;
		static const int kCounter3_TimerOutput_Address = 0x832C;
		static const int kCounter4_TimerOutput_Address = 0x8340;
		static const int kCounter5_TimerOutput_Address = 0x8354;
		static const int kCounter6_TimerOutput_Address = 0x8368;
		static const int kCounter7_TimerOutput_Address = 0x837C;
		static const int kTimerOutput_Addresses [] ;
public:
		static const int kCounter0_Reset_Address = 0x82E4;
		static const int kCounter1_Reset_Address = 0x82F8;
		static const int kCounter2_Reset_Address = 0x830C;
		static const int kCounter3_Reset_Address = 0x8320;
		static const int kCounter4_Reset_Address = 0x8334;
		static const int kCounter5_Reset_Address = 0x8348;
		static const int kCounter6_Reset_Address = 0x835C;
		static const int kCounter7_Reset_Address = 0x8370;
		static const int kReset_Addresses [];
private:
		static const int kCounter0_TimerConfig_Address = 0x82EC;
		static const int kCounter1_TimerConfig_Address = 0x8300;
		static const int kCounter2_TimerConfig_Address = 0x8314;
		static const int kCounter3_TimerConfig_Address = 0x8328;
		static const int kCounter4_TimerConfig_Address = 0x833C;
		static const int kCounter5_TimerConfig_Address = 0x8350;
		static const int kCounter6_TimerConfig_Address = 0x8364;
		static const int kCounter7_TimerConfig_Address = 0x8378;
		static const int kTimerConfig_Addresses [];
#pragma endregion
	private:
		NiFpgaState *state;
		unsigned char sys_index;
		tOutput *counterOutput;
		tConfig *counterConfig;
		tTimerOutput *timerOutput;
		tTimerConfig *timerConfig;
	public:
		tCounter_Impl(NiFpgaState *state, unsigned char sys_index);
		virtual ~tCounter_Impl();

		virtual tSystemInterface* getSystemInterface();
		virtual unsigned char getSystemIndex();

		virtual tOutput readOutput(tRioStatusCode *status);
		virtual bool readOutput_Direction(tRioStatusCode *status);
		virtual signed int readOutput_Value(tRioStatusCode *status);

		virtual void writeConfig(tConfig value, tRioStatusCode *status);
		virtual void writeConfig_UpSource_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_UpSource_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_UpSource_AnalogTrigger(bool value, tRioStatusCode *status);
		virtual void writeConfig_DownSource_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_DownSource_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_DownSource_AnalogTrigger(bool value, tRioStatusCode *status);
		virtual void writeConfig_IndexSource_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_IndexSource_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_IndexSource_AnalogTrigger(bool value, tRioStatusCode *status);
		virtual void writeConfig_IndexActiveHigh(bool value, tRioStatusCode *status);
		virtual void writeConfig_UpRisingEdge(bool value, tRioStatusCode *status);
		virtual void writeConfig_UpFallingEdge(bool value, tRioStatusCode *status);
		virtual void writeConfig_DownRisingEdge(bool value, tRioStatusCode *status);
		virtual void writeConfig_DownFallingEdge(bool value, tRioStatusCode *status);
		virtual void writeConfig_Mode(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_PulseLengthThreshold(unsigned short value, tRioStatusCode *status);
		virtual void writeConfig_Enable(bool value, tRioStatusCode *status);
		virtual tConfig readConfig(tRioStatusCode *status);
		virtual unsigned char readConfig_UpSource_Channel(tRioStatusCode *status);
		virtual unsigned char readConfig_UpSource_Module(tRioStatusCode *status);
		virtual bool readConfig_UpSource_AnalogTrigger(tRioStatusCode *status);
		virtual unsigned char readConfig_DownSource_Channel(tRioStatusCode *status);
		virtual unsigned char readConfig_DownSource_Module(tRioStatusCode *status);
		virtual bool readConfig_DownSource_AnalogTrigger(tRioStatusCode *status);
		virtual unsigned char readConfig_IndexSource_Channel(tRioStatusCode *status);
		virtual unsigned char readConfig_IndexSource_Module(tRioStatusCode *status);
		virtual bool readConfig_IndexSource_AnalogTrigger(tRioStatusCode *status);
		virtual bool readConfig_IndexActiveHigh(tRioStatusCode *status);
		virtual bool readConfig_UpRisingEdge(tRioStatusCode *status);
		virtual bool readConfig_UpFallingEdge(tRioStatusCode *status);
		virtual bool readConfig_DownRisingEdge(tRioStatusCode *status);
		virtual bool readConfig_DownFallingEdge(tRioStatusCode *status);
		virtual unsigned char readConfig_Mode(tRioStatusCode *status);
		virtual unsigned short readConfig_PulseLengthThreshold(tRioStatusCode *status);
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
	};
}

#endif
