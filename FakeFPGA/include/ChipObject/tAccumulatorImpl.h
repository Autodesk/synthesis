/*
* tAccumulatorImpl.h
*
*  Created on: Jul 19, 2014
*      Author: localadmin
*/

#ifndef TACCUMULATORIMPL_H_
#define TACCUMULATORIMPL_H_

#include <ChipObject/tAccumulator.h>

namespace nFPGA {
	class NiFpgaState;
	class tAccumulator_Impl: public nFRC_2012_1_6_4::tAccumulator {
	private:
#pragma region ADDRESSES
	public:
		static const int kAccumulator0_Output_Address = 0x8184;
		static const int kAccumulator1_Output_Address = 0x8194;
	private:
		static const int kOutput_Addresses [];
		static const int kAccumulator0_Center_Address = 0x817C;
		static const int kAccumulator1_Center_Address = 0x818C;
		static const int kCenter_Addresses [] ;
		static const int kAccumulator0_Deadband_Address = 0x8188;
		static const int kAccumulator1_Deadband_Address = 0x8198;
		static const int kDeadband_Addresses []; 
	public:
		static const int kAccumulator0_Reset_Address = 0x8180;
		static const int kAccumulator1_Reset_Address = 0x8190;
		static const int kReset_Addresses [];
	private:
#pragma endregion
	private:
		NiFpgaState *state;
		unsigned char sys_index;

		int32_t *deadband;
		int32_t *center;

		uint32_t outputChunk;
	public:
		// Please kill me.
		tOutput output;
		uint32_t readOutputChunk();

		tAccumulator_Impl(NiFpgaState *state, unsigned char sys_index);
		virtual ~tAccumulator_Impl();

		virtual tSystemInterface* getSystemInterface();
		static tAccumulator* create(unsigned char sys_index, tRioStatusCode *status);
		virtual unsigned char getSystemIndex();

		virtual tOutput readOutput(tRioStatusCode *status);
		virtual signed long long readOutput_Value(tRioStatusCode *status);
		virtual unsigned int readOutput_Count(tRioStatusCode *status);

		virtual void writeCenter(signed int value, tRioStatusCode *status);
		virtual signed int readCenter(tRioStatusCode *status);

		virtual void writeDeadband(signed int value, tRioStatusCode *status);
		virtual signed int readDeadband(tRioStatusCode *status);

		virtual void strobeReset(tRioStatusCode *status);
	};

} /* namespace nFPGA */

#endif /* TACCUMULATORIMPL_H_ */
