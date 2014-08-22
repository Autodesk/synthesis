/*
* tSolenoidImpl.h
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#ifndef TSOLENOIDIMPL_H_
#define TSOLENOIDIMPL_H_

#include <ChipObject/tSolenoid.h>

namespace nFPGA {
	class NiFpgaState;

	class tSolenoid_Impl: public nFPGA::nFRC_2012_1_6_4::tSolenoid {
	private:
#pragma region ADDRESSES
		static const int kDO7_0_ElementSize = 8;
		static const int kDO7_0_ElementMask = 0xFF;
		static const int kSolenoid_DO7_0_Address = 0x8144;
#pragma endregion
	private:
		NiFpgaState *state;
	public:
		uint32_t *solenoidState;

		tSolenoid_Impl(NiFpgaState *state);
		virtual tSystemInterface* getSystemInterface();
		virtual ~tSolenoid_Impl();

		virtual void writeDO7_0(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status);
		virtual unsigned char readDO7_0(unsigned char bitfield_index, tRioStatusCode *status);
	};

}

#endif /* TSOLENOIDIMPL_H_ */
