/*
* NiFpgaState.h
*
*  Created on: Jul 18, 2014
*      Author: localadmin
*/

#ifndef NIFPGASTATE_H_
#define NIFPGASTATE_H_

#include <NiRio.h>
#include <tSystemInterface.h>

namespace nFPGA {

	class tDIO_Impl;
	class tAI_Impl;
	class tSolenoid_Impl;
	class tAccumulator_Impl;
	class tGlobal_Impl;

	class NiFpgaState: public tSystemInterface {
		friend class tDIO_Impl;
		friend class tAI_Impl;
		friend class tSolenoid_Impl;
		friend class tAccumulator_Impl;
		friend class tGlobal_Impl;
	private:
		static const int DIO_COUNT = 4;
		static const int ANALOG_COUNT = 4;
		static const int ACCUM_COUNT = 8;

		tDIO_Impl **dio;
		tAI_Impl **ai;
		tAccumulator_Impl **accum;
		tSolenoid_Impl *solenoid;
		tGlobal_Impl *global;
	public:
		NiFpgaState();
		virtual ~NiFpgaState();

		tDIO_Impl *getDIO(unsigned char module);
		tAI_Impl *getAnalog(unsigned char module);
		tAccumulator_Impl *getAccumulator(unsigned char sys_index);
		tSolenoid_Impl *getSolenoid();
		tGlobal_Impl *getGlobal();

		virtual const uint16_t getExpectedFPGAVersion();
		virtual const uint32_t getExpectedFPGARevision();
		virtual const uint32_t * const getExpectedFPGASignature();
		virtual void getHardwareFpgaSignature(uint32_t *guid_ptr,
			tRioStatusCode *status);
		virtual uint32_t getLVHandle(tRioStatusCode *status);
		virtual uint32_t getHandle();
	};

} /* namespace nFPGA */

#endif /* NIFPGASTATE_H_ */
