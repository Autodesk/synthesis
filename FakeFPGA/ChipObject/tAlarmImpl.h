#ifndef __T_ALARM_IMPL
#define __T_ALARM_IMPL
#include <ChipObject/tAlarm.h>
#include <OSAL/OSAL.h>
#include <OSAL/WaitSemaphore.h>

#if USE_WINAPI
#include <Windows.h>
#endif

class NTTask;

namespace nFPGA
{
	class NiFpgaState;

	class tAlarm_Impl :
		public nFPGA::nFRC_2012_1_6_4::tAlarm
	{
	private:
		NiFpgaState *state;
		bool enabled;
		unsigned int triggerTime;
		NTTask *notifierTask;
		WaitSemaphore changeSemaphore;

		static DWORD WINAPI runNotifier(LPVOID arg);
	public:
		tAlarm_Impl(NiFpgaState *state);
		virtual ~tAlarm_Impl(void);

		virtual tSystemInterface* getSystemInterface();

		virtual void writeEnable(bool value, tRioStatusCode *status);
		virtual bool readEnable(tRioStatusCode *status);

		virtual void writeTriggerTime(unsigned int value, tRioStatusCode *status);
		virtual unsigned int readTriggerTime(tRioStatusCode *status);

	};

}
#endif