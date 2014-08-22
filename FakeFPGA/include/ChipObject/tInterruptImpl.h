#ifndef __T_INTERRUPT_H
#define __T_INTERRUPT_H

#include <ChipObject/tInterrupt.h>

namespace nFPGA {
	class NiFpgaState;
	class tDIO_Impl;

	class tInterrupt_Impl : public nFPGA::nFRC_2012_1_6_4::tInterrupt {
		friend class tDIO_Impl;
	private:
#pragma region ADDRESSES
		static const int kInterrupt0_TimeStamp_Address = 0x83D4;
		static const int kInterrupt1_TimeStamp_Address = 0x83DC;
		static const int kInterrupt2_TimeStamp_Address = 0x83E4;
		static const int kInterrupt3_TimeStamp_Address = 0x83EC;
		static const int kInterrupt4_TimeStamp_Address = 0x83F4;
		static const int kInterrupt5_TimeStamp_Address = 0x83FC;
		static const int kInterrupt6_TimeStamp_Address = 0x8404;
		static const int kInterrupt7_TimeStamp_Address = 0x840C;
		static const int kTimeStamp_Addresses [] ;
		static const int kInterrupt0_Config_Address = 0x83D0;
		static const int kInterrupt1_Config_Address = 0x83D8;
		static const int kInterrupt2_Config_Address = 0x83E0;
		static const int kInterrupt3_Config_Address = 0x83E8;
		static const int kInterrupt4_Config_Address = 0x83F0;
		static const int kInterrupt5_Config_Address = 0x83F8;
		static const int kInterrupt6_Config_Address = 0x8400;
		static const int kInterrupt7_Config_Address = 0x8408;
		static const int kConfig_Addresses [];
#pragma endregion
	private:
		NiFpgaState *state;
		unsigned char sys_index;
		tConfig *config;
		uint32_t *timestamp;
	public:
		tInterrupt_Impl(NiFpgaState *state, unsigned char sys_index);
		~tInterrupt_Impl();


		virtual tSystemInterface* getSystemInterface();
		virtual unsigned char getSystemIndex();

		virtual unsigned int readTimeStamp(tRioStatusCode *status);

		virtual void writeConfig(tConfig value, tRioStatusCode *status);
		virtual void writeConfig_Source_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_Source_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_Source_AnalogTrigger(bool value, tRioStatusCode *status);
		virtual void writeConfig_RisingEdge(bool value, tRioStatusCode *status);
		virtual void writeConfig_FallingEdge(bool value, tRioStatusCode *status);
		virtual void writeConfig_WaitForAck(bool value, tRioStatusCode *status);

		virtual tConfig readConfig(tRioStatusCode *status);
		virtual unsigned char readConfig_Source_Channel(tRioStatusCode *status);
		virtual unsigned char readConfig_Source_Module(tRioStatusCode *status);
		virtual bool readConfig_Source_AnalogTrigger(tRioStatusCode *status);
		virtual bool readConfig_RisingEdge(tRioStatusCode *status);
		virtual bool readConfig_FallingEdge(tRioStatusCode *status);
		virtual bool readConfig_WaitForAck(tRioStatusCode *status);
	};
}

#endif