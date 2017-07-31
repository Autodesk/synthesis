#ifndef __WATCHCAT_IMPL_H
#define __WATCHCAT_IMPL_H

#include <ChipObject/tWatchdog.h>

namespace nFPGA {
	class NiFpgaState;

	/// <summary>Schrodinger's cat.  It won't die unless you ask if it is alive.</summary>
	class tWatchcat_Impl :
		public nFPGA::nFRC_2012_1_6_4::tWatchdog
	{
	private:
		NiFpgaState *state;

		bool immortal;
		unsigned int expiration;
		unsigned int lastFed;
		tStatus status;
	public:
		tWatchcat_Impl(NiFpgaState *state);
		~tWatchcat_Impl(void);

		virtual tSystemInterface* getSystemInterface();

		virtual tStatus readStatus(tRioStatusCode *status);
		virtual bool readStatus_SystemActive(tRioStatusCode *status);
		virtual bool readStatus_Alive(tRioStatusCode *status);
		virtual unsigned short readStatus_SysDisableCount(tRioStatusCode *status);
		virtual unsigned short readStatus_DisableCount(tRioStatusCode *status);


		virtual void strobeKill(tRioStatusCode *status);

		virtual void strobeFeed(tRioStatusCode *status);

		virtual unsigned int readTimer(tRioStatusCode *status);

		virtual void writeExpiration(unsigned int value, tRioStatusCode *status);
		virtual unsigned int readExpiration(tRioStatusCode *status);

		virtual void writeImmortal(bool value, tRioStatusCode *status);
		virtual bool readImmortal(tRioStatusCode *status);
	};
}
#endif