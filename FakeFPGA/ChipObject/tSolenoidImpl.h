/*
* tSolenoidImpl.h
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#ifndef TSOLENOIDIMPL_H_
#define TSOLENOIDIMPL_H_

#include <tSolenoid.h>

namespace nFPGA {
	class NiFpgaState;

	class tSolenoid_Impl: public nFPGA::nFRC_2012_1_6_4::tSolenoid {
	private:
		NiFpgaState *state;
	public:
		unsigned char solenoidState[kNumDO7_0Elements];

		tSolenoid_Impl(NiFpgaState *state);
		virtual tSystemInterface* getSystemInterface();
		virtual ~tSolenoid_Impl();

		virtual void writeDO7_0(unsigned char bitfield_index, unsigned char value, tRioStatusCode *status);
		virtual unsigned char readDO7_0(unsigned char bitfield_index, tRioStatusCode *status);
	};

}

#endif /* TSOLENOIDIMPL_H_ */
