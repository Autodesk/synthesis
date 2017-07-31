/*
* tAIImpl.h
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#ifndef TAIIMPL_H_
#define TAIIMPL_H_

#include <ChipObject/tAI.h>

namespace nFPGA {
	class NiFpgaState;
	class tAI_Impl: public nFPGA::nFRC_2012_1_6_4::tAI {
	private:
#pragma region ADDRESSES
		static const int kConfig_ScanSize_BitfieldMask = 0x1C000000;
		static const int kConfig_ScanSize_BitfieldOffset = 26;
		static const int kConfig_ConvertRate_BitfieldMask = 0x03FFFFFF;
		static const int kConfig_ConvertRate_BitfieldOffset = 0;
		static const int kAI0_Config_Address = 0x8154;
		static const int kAI1_Config_Address = 0x8168;
		static const int kConfig_Addresses [];
		static const int kAI0_LoopTiming_Address = 0x8164;
		static const int kAI1_LoopTiming_Address = 0x8178;
		static const int kLoopTiming_Addresses [];
		static const int kOversampleBits_ElementSize = 4;
		static const int kOversampleBits_ElementMask = 0xF;
		static const int kAI0_OversampleBits_Address = 0x815C;
		static const int kAI1_OversampleBits_Address = 0x8170;
		static const int kOversampleBits_Addresses [];
		static const int kAverageBits_ElementSize = 4;
		static const int kAverageBits_ElementMask = 0xF;
		static const int kAI0_AverageBits_Address = 0x8160;
		static const int kAI1_AverageBits_Address = 0x8174;
		static const int kAverageBits_Addresses [];
		static const int kScanList_ElementSize = 3;
		static const int kScanList_ElementMask = 0x7;
		static const int kAI0_ScanList_Address = 0x8158;
		static const int kAI1_ScanList_Address = 0x816C;
		static const int kScanList_Addresses [] ;
	public:
		static const int kAI_Output_Address = 0x8150;
		static const int kAI_LatchOutput_Address = 0x814C;
	private:
		static const int kAI_ReadSelect_Address = 0x8148;
#pragma endregion
	private:
		unsigned char sys_index;
		NiFpgaState *state;

		tConfig *config;
		uint32_t *loopTiming;
		uint32_t *averageBits;
		uint32_t *oversampleBits;
		uint32_t *scanListBits;

		tReadSelect *readSelect;

		int32_t *output;

	public:
		// Please don't assign
		// Actual backing sensor values
		int32_t values[kNumScanListElements];

		tAI_Impl(NiFpgaState *state, unsigned char sys_index);
		virtual ~tAI_Impl();
		virtual tSystemInterface* getSystemInterface();
		virtual unsigned char getSystemIndex();

		virtual void writeConfig(tConfig value, tRioStatusCode *status);
		virtual void writeConfig_ScanSize(unsigned char value, tRioStatusCode *status);
		virtual void writeConfig_ConvertRate(unsigned int value, tRioStatusCode *status);
		virtual tConfig readConfig(tRioStatusCode *status);
		virtual unsigned char readConfig_ScanSize(tRioStatusCode *status);
		virtual unsigned int readConfig_ConvertRate(tRioStatusCode *status);

		virtual unsigned int readLoopTiming(tRioStatusCode *status);

		virtual void writeOversampleBits(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status);
		virtual unsigned char readOversampleBits(unsigned char bitfield_index, tRioStatusCode *status);

		virtual void writeAverageBits(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status);
		virtual unsigned char readAverageBits(unsigned char bitfield_index, tRioStatusCode *status);

		virtual void writeScanList(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status);
		virtual unsigned char readScanList(unsigned char bitfield_index, tRioStatusCode *status);

		virtual void writeReadSelect(tReadSelect value, tRioStatusCode *status);
		virtual void writeReadSelect_Channel(unsigned char value, tRioStatusCode *status);
		virtual void writeReadSelect_Module(unsigned char value, tRioStatusCode *status);
		virtual void writeReadSelect_Averaged(bool value, tRioStatusCode *status);
		virtual tReadSelect readReadSelect(tRioStatusCode *status);
		virtual unsigned char readReadSelect_Channel(tRioStatusCode *status);
		virtual unsigned char readReadSelect_Module(tRioStatusCode *status);
		virtual bool readReadSelect_Averaged(tRioStatusCode *status);

		virtual signed int readOutput(tRioStatusCode *status);
		virtual void strobeLatchOutput(tRioStatusCode *status);

		/// Callback to set the sensors to the given state and call any interrupts.
		/// @param values The new sensor state
		void updateValues(signed int values[kNumScanListElements]);
	};
}

#endif /* TAIIMPL_H_ */
