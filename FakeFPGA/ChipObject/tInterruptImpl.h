#ifndef __T_INTERRUPT_H
#define __T_INTERRUPT_H

#include <tInterrupt.h>

namespace nFPGA {
	class NiFpgaState;
	class tDIO_Impl;

	class tInterrupt_Impl : public nFPGA::nFRC_2012_1_6_4::tInterrupt {
		friend class tDIO_Impl;
	private:
		NiFpgaState *state;
		unsigned char sys_index;
		tConfig config;
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