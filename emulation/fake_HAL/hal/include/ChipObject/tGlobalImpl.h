/*
* tGlobalImpl.h
*
*  Created on: Jul 19, 2014
*      Author: localadmin
*/

#ifndef TGLOBALIMPL_H_
#define TGLOBALIMPL_H_

#include <ChipObject/tGlobal.h>

namespace nFPGA {

	class NiFpgaState;

	class tGlobal_Impl: public nFRC_2012_1_6_4::tGlobal {
	private:
		NiFpgaState *state;
		static const unsigned int FPGA_VERSION = 0;
		static const unsigned int FPGA_REVISION = 0;
	public:
		tGlobal_Impl(NiFpgaState *state);
		virtual ~tGlobal_Impl();

		virtual tSystemInterface* getSystemInterface();
		virtual unsigned short readVersion(tRioStatusCode *status);
		virtual unsigned int readLocalTime(tRioStatusCode *status);
		virtual void writeFPGA_LED(bool value, tRioStatusCode *status);
		virtual bool readFPGA_LED(tRioStatusCode *status);
		virtual unsigned int readRevision(tRioStatusCode *status);
	};

} /* namespace nFPGA */

#endif /* TGLOBALIMPL_H_ */
