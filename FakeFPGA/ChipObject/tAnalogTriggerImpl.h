#ifndef __T_ANALOG_TRIGGER_H
#define __T_ANALOG_TRIGGER_H

#include <tAnalogTrigger.h>

namespace nFPGA {
	class NiFpgaState;
	class tAnalogTrigger_Impl : public nFPGA::nFRC_2012_1_6_4::tAnalogTrigger {
	private:
		NiFpgaState *state;
		unsigned char sys_index;
		tOutput output[kNumOutputElements];
		tSourceSelect source;

		signed int upperLimit, lowerLimit;
	public:
		tAnalogTrigger_Impl(NiFpgaState *state, unsigned char sys_index);
		virtual ~tAnalogTrigger_Impl();

		virtual tSystemInterface* getSystemInterface();
		virtual unsigned char getSystemIndex();

		virtual void writeSourceSelect(tSourceSelect value, tRioStatusCode *status);
		virtual void writeSourceSelect_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeSourceSelect_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeSourceSelect_Averaged(bool value, tRioStatusCode *status);
		virtual void writeSourceSelect_Filter(bool value, tRioStatusCode *status);
		virtual void writeSourceSelect_FloatingRollover(bool value, tRioStatusCode *status);
		virtual void writeSourceSelect_RolloverLimit(signed short value, tRioStatusCode *status);
		virtual tSourceSelect readSourceSelect(tRioStatusCode *status);
		virtual unsigned char readSourceSelect_Channel(tRioStatusCode *status);
		virtual unsigned char readSourceSelect_Module(tRioStatusCode *status);
		virtual bool readSourceSelect_Averaged(tRioStatusCode *status);
		virtual bool readSourceSelect_Filter(tRioStatusCode *status);
		virtual bool readSourceSelect_FloatingRollover(tRioStatusCode *status);
		virtual signed short readSourceSelect_RolloverLimit(tRioStatusCode *status);

		virtual void writeUpperLimit(signed int value, tRioStatusCode *status);
		virtual signed int readUpperLimit(tRioStatusCode *status);

		virtual void writeLowerLimit(signed int value, tRioStatusCode *status);
		virtual signed int readLowerLimit(tRioStatusCode *status);

		virtual tOutput readOutput(unsigned char bitfield_index, tRioStatusCode *status);
		virtual bool readOutput_InHysteresis(unsigned char bitfield_index, tRioStatusCode *status);
		virtual bool readOutput_OverLimit(unsigned char bitfield_index, tRioStatusCode *status);
		virtual bool readOutput_Rising(unsigned char bitfield_index, tRioStatusCode *status);
		virtual bool readOutput_Falling(unsigned char bitfield_index, tRioStatusCode *status);
	};
}

#endif