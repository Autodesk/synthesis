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

class NiFpgaState: public tSystemInterface {
private:
	static const int DIO_COUNT = 4;
	static const int ANALOG_COUNT = 4;

	tDIO_Impl **dio;
	tAI_Impl **ai;
	tSolenoid_Impl *solenoid;
public:
	NiFpgaState();
	virtual ~NiFpgaState();


	tDIO_Impl *getDIO(unsigned char module);
	tAI_Impl *getAnalog(unsigned char module);
	tSolenoid_Impl *getSolenoid();

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
