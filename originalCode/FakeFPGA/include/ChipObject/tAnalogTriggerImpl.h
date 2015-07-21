#ifndef __T_ANALOG_TRIGGER_H
#define __T_ANALOG_TRIGGER_H

#include <ChipObject/tAnalogTrigger.h>

namespace nFPGA {
	class NiFpgaState;
	class tAI_Impl;
	class tAnalogTrigger_Impl : public nFPGA::nFRC_2012_1_6_4::tAnalogTrigger {
		friend class tAI_Impl;
	private:
#pragma region ADDRESSES
		static const int kAnalogTrigger0_SourceSelect_Address = 0x81A0;
		static const int kAnalogTrigger1_SourceSelect_Address = 0x81AC;
		static const int kAnalogTrigger2_SourceSelect_Address = 0x81B8;
		static const int kAnalogTrigger3_SourceSelect_Address = 0x81C4;
		static const int kAnalogTrigger4_SourceSelect_Address = 0x81D0;
		static const int kAnalogTrigger5_SourceSelect_Address = 0x81DC;
		static const int kAnalogTrigger6_SourceSelect_Address = 0x81E8;
		static const int kAnalogTrigger7_SourceSelect_Address = 0x81F4;
		static const int kSourceSelect_Addresses [];
		static const int kAnalogTrigger0_UpperLimit_Address = 0x81A4;
		static const int kAnalogTrigger1_UpperLimit_Address = 0x81B0;
		static const int kAnalogTrigger2_UpperLimit_Address = 0x81BC;
		static const int kAnalogTrigger3_UpperLimit_Address = 0x81C8;
		static const int kAnalogTrigger4_UpperLimit_Address = 0x81D4;
		static const int kAnalogTrigger5_UpperLimit_Address = 0x81EC;
		static const int kAnalogTrigger6_UpperLimit_Address = 0x81E0;
		static const int kAnalogTrigger7_UpperLimit_Address = 0x81F8;
		static const int kUpperLimit_Addresses [];
		static const int kAnalogTrigger0_LowerLimit_Address = 0x81A8;
		static const int kAnalogTrigger1_LowerLimit_Address = 0x81B4;
		static const int kAnalogTrigger2_LowerLimit_Address = 0x81C0;
		static const int kAnalogTrigger3_LowerLimit_Address = 0x81CC;
		static const int kAnalogTrigger4_LowerLimit_Address = 0x81D8;
		static const int kAnalogTrigger5_LowerLimit_Address = 0x81F0;
		static const int kAnalogTrigger6_LowerLimit_Address = 0x81E4;
		static const int kAnalogTrigger7_LowerLimit_Address = 0x81FC;
		static const int kLowerLimit_Addresses [] ;
		static const int kOutput_ElementSize = 4;
		static const int kOutput_ElementMask = 0xF;
		static const int kOutput_InHysteresis_BitfieldMask = 0x00000008;
		static const int kOutput_InHysteresis_BitfieldOffset = 3;
		static const int kOutput_OverLimit_BitfieldMask = 0x00000004;
		static const int kOutput_OverLimit_BitfieldOffset = 2;
		static const int kOutput_Rising_BitfieldMask = 0x00000002;
		static const int kOutput_Rising_BitfieldOffset = 1;
		static const int kOutput_Falling_BitfieldMask = 0x00000001;
		static const int kOutput_Falling_BitfieldOffset = 0;
		static const int kAnalogTrigger_Output_Address = 0x819C;
#pragma endregion
	private:
		NiFpgaState *state;
		unsigned char sys_index;

		tSourceSelect *source;
		signed int *upperLimit, *lowerLimit;
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

		// This isn't associated with this object, but rather ALL analog triggers
		virtual tOutput readOutput(unsigned char bitfield_index, tRioStatusCode *status);
		virtual bool readOutput_InHysteresis(unsigned char bitfield_index, tRioStatusCode *status);
		virtual bool readOutput_OverLimit(unsigned char bitfield_index, tRioStatusCode *status);
		virtual bool readOutput_Rising(unsigned char bitfield_index, tRioStatusCode *status);
		virtual bool readOutput_Falling(unsigned char bitfield_index, tRioStatusCode *status);

		// For tAIImpl to write trigger states
	private:
		void writeOutput(unsigned char bitfield_index, tOutput output, tRioStatusCode *status);
	};
}

#endif