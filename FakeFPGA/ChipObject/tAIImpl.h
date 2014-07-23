/*
* tAIImpl.h
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#ifndef TAIIMPL_H_
#define TAIIMPL_H_

#include <tAI.h>

namespace nFPGA {
	class NiFpgaState;
	class tAI_Impl: public nFPGA::nFRC_2012_1_6_4::tAI {
	private:
		unsigned char sys_index;
		NiFpgaState *state;

		tConfig config;
		unsigned char averageBits[kNumAverageBitsElements];
		unsigned char oversampleBits[kNumOversampleBitsElements];
		unsigned char scanList[kNumScanListElements];

		tReadSelect readSelect;
		
		signed int values[kNumScanListElements];
	public:
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

		void updateValues(signed int values[kNumScanListElements]);
	};
}

#endif /* TAIIMPL_H_ */
